using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using LibMobiclip.Utils;

namespace LibMobiclip.Codec.Mobiclip.Encoder
{
    public class MacroBlock
    {
        public unsafe MacroBlock(BitmapData Bd, int X, int Y)
        {
            YData16x16 = new byte[256];
            YData8x8 = new byte[4][];
            //YData8x8[0] = new byte[64];
            //YData8x8[1] = new byte[64];
            //YData8x8[2] = new byte[64];
            //YData8x8[3] = new byte[64];
            YData4x4 = new byte[4][][];
            YData4x4[0] = new byte[4][];
            YData4x4[1] = new byte[4][];
            YData4x4[2] = new byte[4][];
            YData4x4[3] = new byte[4][];
            /*for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    YData4x4[i][j] = new byte[16];
                }
            }*/
            YData2x2 = new byte[64][];
            for (int i = 0; i < 64; i++)
            {
                YData2x2[i] = new byte[4];
            }
            UData8x8 = new byte[64];
            UData4x4 = new byte[4][];
            UData4x4[0] = new byte[16];
            UData4x4[1] = new byte[16];
            UData4x4[2] = new byte[16];
            UData4x4[3] = new byte[16];
            VData8x8 = new byte[64];
            VData4x4 = new byte[4][];
            VData4x4[0] = new byte[16];
            VData4x4[1] = new byte[16];
            VData4x4[2] = new byte[16];
            VData4x4[3] = new byte[16];
            //Y
            for (int y = 0; y < 16; y++)
            {
                int* line = (int*)(((byte*)Bd.Scan0) + (Y + y) * Bd.Stride + X * 4);
                for (int x = 0; x < 16; x++)
                {
                    int c = line[x];
                    int r1 = ((c >> 16) & 0xFF) * (255 - 16) / 255 + 16;
                    int g1 = ((c >> 8) & 0xFF) * (255 - 16) / 255 + 16;
                    int b1 = (c & 0xFF) * (255 - 16) / 255 + 16;
                    YData16x16[y * 16 + x] = (byte)((r1 * 299 + g1 * 587 + b1 * 114) / 1000);
                }
            }
            YData8x8[0] = FrameUtil.GetBlockPixels8x8(YData16x16, 0, 0, 16, 0);
            YData8x8[1] = FrameUtil.GetBlockPixels8x8(YData16x16, 8, 0, 16, 0);
            YData8x8[2] = FrameUtil.GetBlockPixels8x8(YData16x16, 0, 8, 16, 0);
            YData8x8[3] = FrameUtil.GetBlockPixels8x8(YData16x16, 8, 8, 16, 0);

            for (int b = 0; b < 4; b++)
            {
                int b2 = 0;
                YData4x4[b][0] = FrameUtil.GetBlockPixels4x4(YData8x8[b], 0, 0, 8, 0);
                YData4x4[b][1] = FrameUtil.GetBlockPixels4x4(YData8x8[b], 4, 0, 8, 0);
                YData4x4[b][2] = FrameUtil.GetBlockPixels4x4(YData8x8[b], 0, 4, 8, 0);
                YData4x4[b][3] = FrameUtil.GetBlockPixels4x4(YData8x8[b], 4, 4, 8, 0);
            }
            for (int y = 0; y < 16; y += 2)
            {
                for (int x = 0; x < 16; x += 2)
                {
                    YData2x2[(y / 2) * 8 + x / 2][0] = YData16x16[y * 16 + x];
                    YData2x2[(y / 2) * 8 + x / 2][1] = YData16x16[y * 16 + x + 1];
                    YData2x2[(y / 2) * 8 + x / 2][2] = YData16x16[(y + 1) * 16 + x];
                    YData2x2[(y / 2) * 8 + x / 2][3] = YData16x16[(y + 1) * 16 + x + 1];
                }
            }
            //UV
            for (int y3 = 0; y3 < 16; y3 += 2)
            {
                int* line = (int*)(((byte*)Bd.Scan0) + (Y + y3) * Bd.Stride + X * 4);
                int* line2 = (int*)(((byte*)Bd.Scan0) + (Y + y3 + 1) * Bd.Stride + X * 4);
                for (int x3 = 0; x3 < 16; x3 += 2)
                {
                    int c = line[x3];
                    int c2 = line[x3 + 1];
                    int c3 = line2[x3];
                    int c4 = line2[x3 + 1];
                    int r1 = ((((c >> 16) & 0xFF) + ((c2 >> 16) & 0xFF) + ((c3 >> 16) & 0xFF) + ((c4 >> 16) & 0xFF) + 2) / 4) * (255 - 16) / 255 + 16;
                    int g1 = ((((c >> 8) & 0xFF) + ((c2 >> 8) & 0xFF) + ((c3 >> 8) & 0xFF) + ((c4 >> 8) & 0xFF) + 2) / 4) * (255 - 16) / 255 + 16;
                    int b1 = ((((c >> 0) & 0xFF) + ((c2 >> 0) & 0xFF) + ((c3 >> 0) & 0xFF) + ((c4 >> 0) & 0xFF) + 2) / 4) * (255 - 16) / 255 + 16;
                    UData8x8[(y3 / 2) * 8 + (x3 / 2)] = (byte)((r1 * -169 + g1 * -331 + b1 * 500) / 1000 + 128);
                    VData8x8[(y3 / 2) * 8 + (x3 / 2)] = (byte)((r1 * 500 + g1 * -419 + b1 * -81) / 1000 + 128);
                }
            }
            {
                int b2 = 0;
                for (int y2 = 0; y2 < 8; y2 += 4)
                {
                    for (int x2 = 0; x2 < 8; x2 += 4)
                    {
                        for (int y3 = 0; y3 < 4; y3++)
                        {
                            for (int x3 = 0; x3 < 4; x3++)
                            {
                                UData4x4[b2][y3 * 4 + x3] = UData8x8[(y2 + y3) * 8 + x3 + x2];
                                VData4x4[b2][y3 * 4 + x3] = VData8x8[(y2 + y3) * 8 + x3 + x2];
                            }
                        }
                        b2++;
                    }
                }
            }
            this.X = X;
            this.Y = Y;
            YUseComplex8x8 = new bool[4];
            YUse4x4 = new bool[4];
            YUseDCT4x4 = new bool[4][];
            YUseDCT4x4[0] = new bool[4];
            YUseDCT4x4[1] = new bool[4];
            YUseDCT4x4[2] = new bool[4];
            YUseDCT4x4[3] = new bool[4];

            YDCT8x8 = new int[4][];
            YDCT4x4 = new int[4][][];
            YDCT4x4[0] = new int[4][];
            YDCT4x4[1] = new int[4][];
            YDCT4x4[2] = new int[4][];
            YDCT4x4[3] = new int[4][];

            UVUseComplex8x8 = new bool[2];
            UVUse4x4 = new bool[2];
            UVUseDCT4x4 = new bool[2][];
            UVUseDCT4x4[0] = new bool[4];
            UVUseDCT4x4[1] = new bool[4];

            UVDCT8x8 = new int[2][];
            UVDCT4x4 = new int[2][][];
            UVDCT4x4[0] = new int[4][];
            UVDCT4x4[1] = new int[4][];
        }
        public int X { get; private set; }
        public int Y { get; private set; }
        //Intra
        public byte[] YData16x16 { get; set; }
        public byte[][] YData8x8 { get; set; }
        public byte[][][] YData4x4 { get; set; }
        public byte[][] YData2x2 { get; set; }
        public byte[] UData8x8 { get; set; }
        public byte[][] UData4x4 { get; set; }
        public byte[] VData8x8 { get; set; }
        public byte[][] VData4x4 { get; set; }
        //Y
        public int YPredictionMode { get; set; }
        public int YPredict16x16Arg { get; set; }
        public bool[] YUseComplex8x8 { get; private set; }
        public bool[] YUse4x4 { get; private set; }//false = DCT, true = 4x4
        public bool[][] YUseDCT4x4 { get; private set; }

        public int[][] YDCT8x8 { get; private set; }
        public int[][][] YDCT4x4 { get; private set; }
        //UV
        public int UVPredictionMode { get; set; }
        public int UVPredict8x8ArgU { get; set; }
        public int UVPredict8x8ArgV { get; set; }
        public bool[] UVUseComplex8x8 { get; private set; }
        public bool[] UVUse4x4 { get; private set; }//false = DCT, true = 4x4
        public bool[][] UVUseDCT4x4 { get; private set; }

        public int[][] UVDCT8x8 { get; private set; }
        public int[][][] UVDCT4x4 { get; private set; }

        public bool UseInterPrediction { get; set; }
        public Analyzer.PBlock InterPredictionConfig { get; set; }

        public bool UseIntraSubBlockMode { get; set; }
        public int[][] YIntraSubBlockModeTypes { get; set; }

        public void ReInit()
        {
            YPredictionMode = 0;
            YPredict16x16Arg = 0;
            UVPredictionMode = 0;
            UVPredict8x8ArgU = 0;
            UVPredict8x8ArgV = 0;
            YUseComplex8x8 = new bool[4];
            YUse4x4 = new bool[4];
            YUseDCT4x4 = new bool[4][];
            YUseDCT4x4[0] = new bool[4];
            YUseDCT4x4[1] = new bool[4];
            YUseDCT4x4[2] = new bool[4];
            YUseDCT4x4[3] = new bool[4];

            YDCT8x8 = new int[4][];
            YDCT4x4 = new int[4][][];
            YDCT4x4[0] = new int[4][];
            YDCT4x4[1] = new int[4][];
            YDCT4x4[2] = new int[4][];
            YDCT4x4[3] = new int[4][];

            UVUseComplex8x8 = new bool[2];
            UVUse4x4 = new bool[2];
            UVUseDCT4x4 = new bool[2][];
            UVUseDCT4x4[0] = new bool[4];
            UVUseDCT4x4[1] = new bool[4];

            UVDCT8x8 = new int[2][];
            UVDCT4x4 = new int[2][][];
            UVDCT4x4[0] = new int[4][];
            UVDCT4x4[1] = new int[4][];

            UseInterPrediction = false;
            InterPredictionConfig = null;
        }

        public int SetupDCTs(MobiEncoder Context, bool PFrame)
        {
            int NrBits = 0;
            byte[] plane = null;
            if (YPredictionMode == 2)
            {
                plane = PredictIntraPlane16x16(Context.YDec, Y * Context.Stride + X, Context.Stride, YPredict16x16Arg);
                NrBits += (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntSigned(YPredict16x16Arg);
            }
            byte[] predictioncompvalsY = null;
            if (UseInterPrediction)
            {
                predictioncompvalsY = InterPredictionConfig.GetCompvalsY(Context, this, 0, 0);
                Point pstackentry = new Point();
                NrBits += InterPredictionConfig.GetNrBitsRequired(0, 0, ref pstackentry);
            }
            if (PFrame && !UseInterPrediction) NrBits += 5;
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    if (YUseComplex8x8[x + y * 2] && !YUse4x4[x + y * 2])
                    {
                        int[] Block2 = new int[64];
                        byte[] CompVals = null;
                        if (UseInterPrediction)
                            CompVals = FrameUtil.GetBlockPixels8x8(predictioncompvalsY, x * 8, y * 8, 16, 0);
                        //CompVals = FrameUtil.GetPBlock(Context.PastFramesY[InterPredictionFrame], InterPredictionDelta.X, InterPredictionDelta.Y, 8, 8, (Y + y * 8) * Context.Stride + X + x * 8, Context.Stride);
                        //CompVals = FrameUtil.GetBlockPixels8x8(Context.PastFramesY[InterPredictionFrame], X + x * 8 + InterPredictionDelta.X / 2, Y + y * 8 + InterPredictionDelta.Y / 2, Context.Stride, 0);
                        else if (YPredictionMode == 2) CompVals = FrameUtil.GetBlockPixels8x8(plane, x * 8, y * 8, 16, 0);
                        else CompVals = GetCompvals8x8(YPredictionMode, Context.YDec, X + x * 8, Y + y * 8, Context.Stride, 0);
                        for (int i = 0; i < 64; i++)
                        {
                            Block2[i] = YData8x8[x + y * 2][i] - CompVals[i];
                        }
                        int[] dct = MobiEncoder.DCT64(Block2);
                        YDCT8x8[x + y * 2] = new int[64];
                        for (int i = 0; i < 64; i++)
                        {
                            YDCT8x8[x + y * 2][MobiConst.ZigZagTable8x8[i]] = (int)Math.Round(dct[i] / Context.QTable8x8[i]);
                        }
                        NrBits += MobiEncoder.CalculateNrBitsDCT(YDCT8x8[x + y * 2], 0);
                        int lastnonzero = 0;
                        for (int i = 0; i < 64; i++)
                        {
                            if (YDCT8x8[x + y * 2][i] != 0) lastnonzero = i;
                        }
                        if (lastnonzero == 0 && YDCT8x8[x + y * 2][0] == 0) YUseComplex8x8[x + y * 2] = false;
                        byte[] decresult;
                        if (YUseComplex8x8[x + y * 2])
                        {
                            int[] realdct = new int[64];
                            for (int i = 0; i < 64; i++)
                            {
                                realdct[i] = (int)Math.Round(dct[i] / Context.QTable8x8[i]);
                                realdct[i] = realdct[i] * (int)Context.QTable8x8[i];
                            }
                            decresult = MobiEncoder.IDCT64(realdct, CompVals);
                        }
                        else decresult = CompVals;
                        FrameUtil.SetBlockPixels8x8(Context.YDec, X + x * 8, Y + y * 8, Context.Stride, 0, decresult);
                    }
                    else if (YUseComplex8x8[x + y * 2] && YUse4x4[x + y * 2])
                    {
                        NrBits += BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4 + (UseIntraSubBlockMode ? (2 * 16) : 0);
                        for (int y2 = 0; y2 < 2; y2++)
                        {
                            for (int x2 = 0; x2 < 2; x2++)
                            {
                                if (YUseDCT4x4[x + y * 2][x2 + y2 * 2])
                                {
                                    int[] Block2 = new int[16];
                                    byte[] CompVals;
                                    if (UseInterPrediction)
                                        CompVals = FrameUtil.GetBlockPixels4x4(predictioncompvalsY, x * 8 + x2 * 4, y * 8 + y2 * 4, 16, 0);
                                    else CompVals = GetCompvals4x4(10 + (UseIntraSubBlockMode ? YIntraSubBlockModeTypes[x + y * 2][x2 + y2 * 2] : YPredictionMode), Context.YDec, X + x * 8 + x2 * 4, Y + y * 8 + y2 * 4, Context.Stride, 0);
                                    for (int i = 0; i < 16; i++)
                                    {
                                        Block2[i] = YData4x4[x + y * 2][x2 + y2 * 2][i] - CompVals[i];
                                    }
                                    int[] dct = MobiEncoder.DCT16(Block2);
                                    YDCT4x4[x + y * 2][x2 + y2 * 2] = new int[16];
                                    for (int i = 0; i < 16; i++)
                                    {
                                        YDCT4x4[x + y * 2][x2 + y2 * 2][MobiConst.ZigZagTable4x4[i]] = (int)Math.Round(dct[i] / Context.QTable4x4[i]);
                                    }
                                    NrBits += MobiEncoder.CalculateNrBitsDCT(YDCT4x4[x + y * 2][x2 + y2 * 2], 0);
                                    int lastnonzero = 0;
                                    for (int i = 0; i < 16; i++)
                                    {
                                        if (YDCT4x4[x + y * 2][x2 + y2 * 2][i] != 0) lastnonzero = i;
                                    }
                                    if (lastnonzero == 0 && YDCT4x4[x + y * 2][x2 + y2 * 2][0] == 0) YUseDCT4x4[x + y * 2][x2 + y2 * 2] = false;
                                    byte[] decresult;
                                    if (YUseDCT4x4[x + y * 2][x2 + y2 * 2])
                                    {
                                        int[] realdct = new int[16];
                                        for (int i = 0; i < 16; i++)
                                        {
                                            realdct[i] = (int)Math.Round(dct[i] / Context.QTable4x4[i]);
                                            realdct[i] = realdct[i] * (int)Context.QTable4x4[i];
                                        }
                                        decresult = MobiEncoder.IDCT16(realdct, CompVals);
                                    }
                                    else decresult = CompVals;
                                    FrameUtil.SetBlockPixels4x4(Context.YDec, X + x * 8 + x2 * 4, Y + y * 8 + y2 * 4, Context.Stride, 0, decresult);
                                }
                            }
                        }
                    }
                }
            }
            if (UVUseComplex8x8[0] && !UVUse4x4[0])
            {
                int[] Block2 = new int[64];
                byte[] CompVals = null;
                if (UseInterPrediction)
                    CompVals = InterPredictionConfig.GetCompvalsU(Context, this, 0, 0);
                //CompVals = FrameUtil.GetPBlock(Context.PastFramesUV[InterPredictionFrame], InterPredictionDelta.X >> 1, InterPredictionDelta.Y >> 1, 8, 8, (Y / 2) * Context.Stride + X / 2, Context.Stride);
                //CompVals = FrameUtil.GetBlockPixels8x8(Context.PastFramesUV[InterPredictionFrame], X / 2 + (InterPredictionDelta.X >> 2), Y / 2 + (InterPredictionDelta.Y >> 2), Context.Stride, 0);
                else CompVals = GetCompvals8x8(UVPredictionMode, Context.UVDec, X / 2, Y / 2, Context.Stride, 0);
                for (int i = 0; i < 64; i++)
                {
                    Block2[i] = UData8x8[i] - CompVals[i];
                }
                int[] dct = MobiEncoder.DCT64(Block2);
                UVDCT8x8[0] = new int[64];
                for (int i = 0; i < 64; i++)
                {
                    UVDCT8x8[0][MobiConst.ZigZagTable8x8[i]] = (int)Math.Round(dct[i] / Context.QTable8x8[i]);
                }
                NrBits += MobiEncoder.CalculateNrBitsDCT(UVDCT8x8[0], 0);
                int lastnonzero = 0;
                for (int i = 0; i < 64; i++)
                {
                    if (UVDCT8x8[0][i] != 0) lastnonzero = i;
                }
                if (lastnonzero == 0 && UVDCT8x8[0][0] == 0) UVUseComplex8x8[0] = false;
                byte[] decresult;
                if (UVUseComplex8x8[0])
                {
                    int[] realdct = new int[64];
                    for (int i = 0; i < 64; i++)
                    {

                        realdct[i] = (int)Math.Round(dct[i] / Context.QTable8x8[i]);
                        realdct[i] = realdct[i] * (int)Context.QTable8x8[i];
                    }
                    decresult = MobiEncoder.IDCT64(realdct, CompVals);
                }
                else decresult = CompVals;
                FrameUtil.SetBlockPixels8x8(Context.UVDec, X / 2, Y / 2, Context.Stride, 0, decresult);
            }
            else if (UVUseComplex8x8[0] && UVUse4x4[0])
            {
                NrBits += BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                for (int y2 = 0; y2 < 2; y2++)
                {
                    for (int x2 = 0; x2 < 2; x2++)
                    {
                        if (UVUseDCT4x4[0][x2 + y2 * 2])
                        {
                            int[] Block2 = new int[16];
                            byte[] CompVals = GetCompvals4x4(10 + UVPredictionMode, Context.UVDec, X / 2 + x2 * 4, Y / 2 + y2 * 4, Context.Stride, 0);
                            for (int i = 0; i < 16; i++)
                            {
                                Block2[i] = UData4x4[x2 + y2 * 2][i] - CompVals[i];
                            }
                            int[] dct = MobiEncoder.DCT16(Block2);
                            UVDCT4x4[0][x2 + y2 * 2] = new int[16];
                            for (int i = 0; i < 16; i++)
                            {
                                UVDCT4x4[0][x2 + y2 * 2][MobiConst.ZigZagTable4x4[i]] = (int)Math.Round(dct[i] / Context.QTable4x4[i]);
                            }
                            NrBits += MobiEncoder.CalculateNrBitsDCT(UVDCT4x4[0][x2 + y2 * 2], 0);
                            int lastnonzero = 0;
                            for (int i = 0; i < 16; i++)
                            {
                                if (UVDCT4x4[0][x2 + y2 * 2][i] != 0) lastnonzero = i;
                            }
                            if (lastnonzero == 0 && UVDCT4x4[0][x2 + y2 * 2][0] == 0) UVUseDCT4x4[0][x2 + y2 * 2] = false;
                            byte[] decresult;
                            if (UVUseDCT4x4[0][x2 + y2 * 2])
                            {
                                int[] realdct = new int[16];
                                for (int i = 0; i < 16; i++)
                                {
                                    realdct[i] = (int)Math.Round(dct[i] / Context.QTable4x4[i]);
                                    realdct[i] = realdct[i] * (int)Context.QTable4x4[i];
                                }
                                decresult = MobiEncoder.IDCT16(realdct, CompVals);
                            }
                            else decresult = CompVals;
                            FrameUtil.SetBlockPixels4x4(Context.UVDec, X / 2 + x2 * 4, Y / 2 + y2 * 4, Context.Stride, 0, decresult);
                        }
                    }
                }
            }
            if (UVUseComplex8x8[1] && !UVUse4x4[1])
            {
                int[] Block2 = new int[64];
                byte[] CompVals = null;
                if (UseInterPrediction)
                    CompVals = InterPredictionConfig.GetCompvalsV(Context, this, 0, 0);
                //CompVals = FrameUtil.GetPBlock(Context.PastFramesUV[InterPredictionFrame], InterPredictionDelta.X >> 1, InterPredictionDelta.Y >> 1, 8, 8, (Y / 2) * Context.Stride + X / 2 + Context.Stride / 2, Context.Stride);
                //CompVals = FrameUtil.GetBlockPixels8x8(Context.PastFramesUV[InterPredictionFrame], X / 2 + (InterPredictionDelta.X >> 2), Y / 2 + (InterPredictionDelta.Y >> 2), Context.Stride, Context.Stride / 2);
                else CompVals = GetCompvals8x8(UVPredictionMode, Context.UVDec, X / 2, Y / 2, Context.Stride, Context.Stride / 2);
                for (int i = 0; i < 64; i++)
                {
                    Block2[i] = VData8x8[i] - CompVals[i];
                }
                int[] dct = MobiEncoder.DCT64(Block2);
                UVDCT8x8[1] = new int[64];
                for (int i = 0; i < 64; i++)
                {
                    UVDCT8x8[1][MobiConst.ZigZagTable8x8[i]] = (int)Math.Round(dct[i] / Context.QTable8x8[i]);
                }
                NrBits += MobiEncoder.CalculateNrBitsDCT(UVDCT8x8[1], 0);
                int lastnonzero = 0;
                for (int i = 0; i < 64; i++)
                {
                    if (UVDCT8x8[1][i] != 0) lastnonzero = i;
                }
                if (lastnonzero == 0 && UVDCT8x8[1][0] == 0) UVUseComplex8x8[1] = false;
                byte[] decresult;
                if (UVUseComplex8x8[1])
                {
                    int[] realdct = new int[64];
                    for (int i = 0; i < 64; i++)
                    {

                        realdct[i] = (int)Math.Round(dct[i] / Context.QTable8x8[i]);
                        realdct[i] = realdct[i] * (int)Context.QTable8x8[i];
                    }
                    decresult = MobiEncoder.IDCT64(realdct, CompVals);
                }
                else decresult = CompVals;
                FrameUtil.SetBlockPixels8x8(Context.UVDec, X / 2, Y / 2, Context.Stride, Context.Stride / 2, decresult);
            }
            else if (UVUseComplex8x8[1] && UVUse4x4[1])
            {
                NrBits += BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                for (int y2 = 0; y2 < 2; y2++)
                {
                    for (int x2 = 0; x2 < 2; x2++)
                    {
                        if (UVUseDCT4x4[1][x2 + y2 * 2])
                        {
                            int[] Block2 = new int[16];
                            byte[] CompVals = GetCompvals4x4(10 + UVPredictionMode, Context.UVDec, X / 2 + x2 * 4, Y / 2 + y2 * 4, Context.Stride, Context.Stride / 2);
                            for (int i = 0; i < 16; i++)
                            {
                                Block2[i] = VData4x4[x2 + y2 * 2][i] - CompVals[i];
                            }
                            int[] dct = MobiEncoder.DCT16(Block2);
                            UVDCT4x4[1][x2 + y2 * 2] = new int[16];
                            for (int i = 0; i < 16; i++)
                            {
                                UVDCT4x4[1][x2 + y2 * 2][MobiConst.ZigZagTable4x4[i]] = (int)Math.Round(dct[i] / Context.QTable4x4[i]);
                            }
                            NrBits += MobiEncoder.CalculateNrBitsDCT(UVDCT4x4[1][x2 + y2 * 2], 0);
                            int lastnonzero = 0;
                            for (int i = 0; i < 16; i++)
                            {
                                if (UVDCT4x4[1][x2 + y2 * 2][i] != 0) lastnonzero = i;
                            }
                            if (lastnonzero == 0 && UVDCT4x4[1][x2 + y2 * 2][0] == 0) UVUseDCT4x4[1][x2 + y2 * 2] = false;
                            byte[] decresult;
                            if (UVUseDCT4x4[1][x2 + y2 * 2])
                            {
                                int[] realdct = new int[16];
                                for (int i = 0; i < 16; i++)
                                {
                                    realdct[i] = (int)Math.Round(dct[i] / Context.QTable4x4[i]);
                                    realdct[i] = realdct[i] * (int)Context.QTable4x4[i];
                                }
                                decresult = MobiEncoder.IDCT16(realdct, CompVals);
                            }
                            else decresult = CompVals;
                            FrameUtil.SetBlockPixels4x4(Context.UVDec, X / 2 + x2 * 4, Y / 2 + y2 * 4, Context.Stride, Context.Stride / 2, decresult);
                        }
                    }
                }
            }
            return NrBits;
        }

        /*private static Color ConvertToVideoLevels(Color C)
        {
            return Color.FromArgb(
                (int)(C.R * (255f - 16f) / 255f + 16f),
                (int)(C.G * (255f - 16f) / 255f + 16f),
                (int)(C.B * (255f - 16f) / 255f + 16f));
        }*/

        /*private static byte GetYForColor(int c)//Color C)
        {
            int r = ((c >> 16) & 0xFF) * (255 - 16) / 255 + 16;
            int g = ((c >> 8) & 0xFF) * (255 - 16) / 255 + 16;
            int b = (c & 0xFF) * (255 - 16) / 255 + 16;
            //return r * 0.299f + g * 0.587f + b * 0.114f;
            return (byte)((r * 299 + g * 587 + b * 114) / 1000);
        }*/

        /*private static byte GetUForColor(int rin, int gin, int bin)
        {
            int r = rin * (255 - 16) / 255 + 16;
            int g = gin * (255 - 16) / 255 + 16;
            int b = bin * (255 - 16) / 255 + 16;
            //return r * -0.169f + g * -0.331f + b * 0.5f + 128f;
            return (byte)((r * -169 + g * -331 + b * 500) / 1000 + 128);
        }
        private static byte GetUForColors(int A, int B, int C, int D)
        {
            return GetUForColor(
                (((A >> 16) & 0xFF) + ((B >> 16) & 0xFF) + ((C >> 16) & 0xFF) + ((D >> 16) & 0xFF) + 2) / 4,
                (((A >> 8) & 0xFF) + ((B >> 8) & 0xFF) + ((C >> 8) & 0xFF) + ((D >> 8) & 0xFF) + 2) / 4,
                (((A >> 0) & 0xFF) + ((B >> 0) & 0xFF) + ((C >> 0) & 0xFF) + ((D >> 0) & 0xFF) + 2) / 4);
            /*Color result = Color.FromArgb(
                (A.R + B.R + C.R + D.R + 2) / 4,
                (A.G + B.G + C.G + D.G + 2) / 4,
                (A.B + B.B + C.B + D.B + 2) / 4);
            return GetUForColor(result);/
        }

        private static byte GetVForColor(int rin, int gin, int bin)
        {
            int r = rin * (255 - 16) / 255 + 16;
            int g = gin * (255 - 16) / 255 + 16;
            int b = bin * (255 - 16) / 255 + 16;
            //return r * 0.5f + g * -0.419f + b * -0.081f + 128f;
            return (byte)((r * 500 + g * -419 + b * -81) / 1000 + 128);
        }

        private static byte GetVForColors(int A, int B, int C, int D)
        {
            return GetVForColor(
                (((A >> 16) & 0xFF) + ((B >> 16) & 0xFF) + ((C >> 16) & 0xFF) + ((D >> 16) & 0xFF) + 2) / 4,
                (((A >> 8) & 0xFF) + ((B >> 8) & 0xFF) + ((C >> 8) & 0xFF) + ((D >> 8) & 0xFF) + 2) / 4,
                (((A >> 0) & 0xFF) + ((B >> 0) & 0xFF) + ((C >> 0) & 0xFF) + ((D >> 0) & 0xFF) + 2) / 4);
            /*Color result = Color.FromArgb(
               (A.R + B.R + C.R + D.R + 2) / 4,
               (A.G + B.G + C.G + D.G + 2) / 4,
               (A.B + B.B + C.B + D.B + 2) / 4);
            return GetVForColor(result);/
        }*/

        public static byte[] EncodeDecode8x8Block(MobiEncoder Context, byte[] Block, byte[] RefDecData, int X, int Y, int Stride, int Offset, int PMode, ref int NrBits)
        {
            return EncodeDecode8x8Block(Context, Block, RefDecData, GetCompvals8x8(PMode, RefDecData, X, Y, Stride, Offset), X, Y, Stride, Offset, ref NrBits);
        }

        //private static int BitConst8x8 = 0;

        public static byte[] EncodeDecode8x8Block(MobiEncoder Context, byte[] Block, byte[] RefDecData, byte[] CompVals, int X, int Y, int Stride, int Offset, ref int NrBits)
        {
            int[] Block2 = new int[64];
            for (int i = 0; i < 64; i++)
            {
                Block2[i] = Block[i] - CompVals[i];
            }
            int[] DCTResult = MobiEncoder.DCT64(Block2);
            int[] EncodeDct = new int[64];
            int[] RealDCT = new int[64];
            for (int i = 0; i < 64; i++)
            {
                int val = (int)Math.Round(DCTResult[i] / Context.QTable8x8[i]);
                EncodeDct[MobiConst.ZigZagTable8x8[i]] = val;
                RealDCT[i] = val * (int)Context.QTable8x8[i];
            }
            NrBits += MobiEncoder.CalculateNrBitsDCT(EncodeDct, 0);
            byte[] result = MobiEncoder.IDCT64(RealDCT, CompVals);
            FrameUtil.SetBlockPixels8x8(RefDecData, X, Y, Stride, Offset, result);
            return result;
        }

        public static byte[] EncodeDecode4x4Block(MobiEncoder Context, byte[] Block, byte[] RefDecData, int X, int Y, int Stride, int Offset, int PMode, ref int NrBits)
        {
            return EncodeDecode4x4Block(Context, Block, RefDecData, GetCompvals4x4(PMode, RefDecData, X, Y, Stride, Offset), X, Y, Stride, Offset, ref NrBits);
        }

        public static byte[] EncodeDecode4x4Block(MobiEncoder Context, byte[] Block, byte[] RefDecData, byte[] CompVals, int X, int Y, int Stride, int Offset, ref int NrBits)
        {
            //byte[] CompVals = GetCompvals4x4(PMode, RefDecData, X, Y, Stride, Offset);
            int[] Block2 = new int[16];
            for (int i = 0; i < 16; i++)
            {
                Block2[i] = Block[i] - CompVals[i];
            }
            int[] DCTResult = MobiEncoder.DCT16(Block2);
            int[] EncodeDct = new int[16];
            int[] RealDCT = new int[16];
            for (int i = 0; i < 16; i++)
            {
                int val = (int)Math.Round(DCTResult[i] / Context.QTable4x4[i]);
                EncodeDct[MobiConst.ZigZagTable4x4[i]] = val;
                RealDCT[i] = val * (int)Context.QTable4x4[i];
            }
            NrBits += MobiEncoder.CalculateNrBitsDCT(EncodeDct, 0);
            byte[] result = MobiEncoder.IDCT16(RealDCT, CompVals);
            FrameUtil.SetBlockPixels4x4(RefDecData, X, Y, Stride, Offset, result);
            return result;
        }



        public static unsafe byte[] GetCompvals8x8(int BlockType, byte[] Data, int X, int Y, int Stride, int Offset)
        {
            switch (BlockType)
            {
                case 0:
                    {
                        byte[] ThatBlock = FrameUtil.GetBlockPixels8x8(Data, X, Y - 8, Stride, Offset);
                        byte[] CompVals = new byte[64];
                        fixed (byte* pCompVals = &CompVals[0], pThatBlock = &ThatBlock[7 * 8])
                        {
                            ulong* pLCompVals = (ulong*)pCompVals;
                            ulong val = *((ulong*)pThatBlock);
                            pLCompVals[0] = val;
                            pLCompVals[1] = val;
                            pLCompVals[2] = val;
                            pLCompVals[3] = val;
                            pLCompVals[4] = val;
                            pLCompVals[5] = val;
                            pLCompVals[6] = val;
                            pLCompVals[7] = val;
                        }
                        return CompVals;
                    }
                case 1:
                    {
                        byte[] ThatBlock = FrameUtil.GetBlockPixels8x8(Data, X - 8, Y, Stride, Offset);
                        byte[] CompVals = new byte[64];
                        for (int y = 0; y < 8; y++)
                        {
                            byte val = ThatBlock[y * 8 + 7];
                            for (int x = 0; x < 8; x++)
                            {
                                CompVals[y * 8 + x] = val;
                            }
                        }
                        return CompVals;
                    }
                case 3:
                    {
                        byte[] CompVals = new byte[64];
                        int r8 = 0;
                        if (X > 0) r8 += 8;
                        if (Y > 0) r8 += 4;
                        switch (r8 / 4)
                        {
                            case 0://001170E4
                                {
                                    for (int i = 0; i < 64; i++) CompVals[i] = 0x80;
                                    break;
                                }
                            case 1:
                                {
                                    byte[] ThatBlock = FrameUtil.GetBlockPixels8x8(Data, X, Y - 8, Stride, Offset);
                                    int sum = 0;
                                    for (int x3 = 0; x3 < 8; x3++)
                                    {
                                        sum += (int)ThatBlock[7 * 8 + x3];
                                    }
                                    sum = (sum + 4) / 8;
                                    for (int i = 0; i < 64; i++) CompVals[i] = (byte)sum;
                                    break;
                                }
                            case 2:
                                {
                                    byte[] ThatBlock = FrameUtil.GetBlockPixels8x8(Data, X - 8, Y, Stride, Offset);
                                    int sum = 0;
                                    for (int y3 = 0; y3 < 8; y3++)
                                    {
                                        sum += (int)ThatBlock[y3 * 8 + 7];
                                    }
                                    sum = (sum + 4) / 8;
                                    for (int i = 0; i < 64; i++) CompVals[i] = (byte)sum;
                                    break;
                                }
                            case 3://00116EC0
                                {
                                    byte[] ThatBlock1 = FrameUtil.GetBlockPixels8x8(Data, X, Y - 8, Stride, Offset);
                                    byte[] ThatBlock2 = FrameUtil.GetBlockPixels8x8(Data, X - 8, Y, Stride, Offset);
                                    int sum = 0;
                                    for (int x3 = 0; x3 < 8; x3++)
                                    {
                                        sum += (int)ThatBlock1[7 * 8 + x3];
                                    }
                                    for (int y3 = 0; y3 < 8; y3++)
                                    {
                                        sum += (int)ThatBlock2[y3 * 8 + 7];
                                    }
                                    sum = (sum + 8) / 16;
                                    for (int i = 0; i < 64; i++) CompVals[i] = (byte)sum;
                                    break;
                                }
                            default:
                                break;
                        }
                        return CompVals;
                    }
                case 4:
                    {
                        uint r11_i = (uint)(Y * Stride + X + Offset);
                        uint r3_i = Data[r11_i - 1];
                        r11_i -= 1;
                        uint r12_i = Data[r11_i + Stride];
                        r11_i += (uint)Stride;
                        uint r9_i = Data[r11_i + Stride];
                        r11_i += (uint)Stride;
                        uint r6_i = Data[r11_i + Stride];
                        r11_i += (uint)Stride;
                        uint r8_i = ((r3_i + r12_i) + 1) / 2;
                        uint lr_i = ((r9_i + r3_i + r12_i * 2) + 2) / 4;
                        r3_i = ((r12_i + r9_i) + 1) / 2;
                        r12_i = ((r6_i + r12_i + r9_i * 2) + 2) / 4;
                        r8_i |= (lr_i << 8) | (r3_i << 16) | (r12_i << 24);
                        lr_i = r11_i - (uint)Stride * 3;
                        IOUtil.WriteU32LE(Data, (int)lr_i + 1, r8_i);
                        lr_i = ((r9_i + r6_i) + 1) / 2;
                        r8_i = Data[r11_i + Stride];
                        r11_i += (uint)Stride;
                        r3_i |= (r12_i << 8) | (lr_i << 16);
                        uint r4_i = ((r8_i + r9_i + r6_i * 2) + 2) / 4;
                        uint r5_i = ((r6_i + r8_i) + 1) / 2;
                        r9_i = Data[r11_i + Stride];
                        r11_i += (uint)Stride;
                        r3_i |= r4_i << 24;
                        uint r7_i = r11_i - (4 * (uint)Stride);
                        IOUtil.WriteU32LE(Data, (int)r7_i + 1, r3_i);
                        r6_i = ((r9_i + r6_i + r8_i * 2) + 2) / 4;
                        r7_i = lr_i | (r4_i << 8) | (r5_i << 16) | (r6_i << 24);
                        r12_i = r11_i - (5 * (uint)Stride);
                        IOUtil.WriteU32LE(Data, (int)r12_i + 5, r7_i);
                        r12_i = ((r8_i + r9_i) + 1) / 2;
                        r3_i = Data[r11_i + Stride];
                        r11_i += (uint)Stride;
                        lr_i = r5_i | (r6_i << 8) | (r12_i << 16);
                        r8_i = ((r3_i + r8_i + r9_i * 2) + 2) / 4;
                        lr_i |= r8_i << 24;
                        r5_i = r11_i - (uint)Stride * 4;
                        IOUtil.WriteU32LE(Data, (int)r5_i + 1, r7_i);
                        IOUtil.WriteU32LE(Data, (int)r5_i /*- 0xFB*/ - (Stride - 5), lr_i);
                        r5_i = ((r9_i + r3_i) + 1) / 2;
                        r4_i = Data[r11_i + Stride];
                        r8_i = r12_i | (r8_i << 8) | (r5_i << 16);
                        r9_i = ((r4_i + r9_i + r3_i * 2) + 2) / 4;
                        r12_i = ((r3_i + r4_i) + 1) / 2;
                        r8_i |= r9_i << 24;
                        r3_i = ((r4_i + r3_i + r4_i * 2) + 2) / 4;
                        r9_i = r5_i | (r9_i << 8) | (r12_i << 16) | (r3_i << 24);
                        r7_i = r11_i - (uint)Stride * 2;
                        IOUtil.WriteU32LE(Data, (int)r7_i /*- 0x1FB*/ - (Stride * 2 - 5), r8_i);
                        IOUtil.WriteU32LE(Data, (int)r7_i /*- 0xFB*/ - (Stride - 5), r9_i);
                        IOUtil.WriteU32LE(Data, (int)r7_i /*- 0xFF*/ - (Stride - 1), lr_i);
                        IOUtil.WriteU32LE(Data, (int)r7_i + 1, r8_i);
                        r8_i = r12_i | (r3_i << 8) | (r4_i << 16) | (r4_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r11_i /*- 0xFF*/ - (Stride - 1), r9_i);
                        r9_i = r4_i | (r4_i << 8) | (r4_i << 16) | (r4_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r11_i /*- 0x1FB*/ - (Stride * 2 - 5), r8_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i /*- 0xFB*/ - (Stride - 5), r9_i);
                        r11_i++;
                        IOUtil.WriteU32LE(Data, (int)r11_i, r8_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + 4, r9_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride, r9_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride + 4, r9_i);
                        r11_i -= (uint)Stride * 6;
                        return FrameUtil.GetBlockPixels8x8(Data, X, Y, Stride, Offset);
                    }
                case 5:
                    {
                        uint v1; // r3@1
                        uint v2; // r12@1
                        uint v4; // r1@1
                        uint v5; // r2@1
                        uint v6; // lr@1
                        uint v7; // r5@1
                        uint v8; // r4@1
                        uint v9; // r9@1
                        uint v10; // r7@1
                        uint v11; // r6@1
                        uint v12; // r7@1
                        uint v13; // r11@1
                        uint v14; // r8@1
                        uint v15; // r9@1
                        uint v16; // r12@1
                        uint v17; // r2@1
                        uint v18; // r3@1
                        uint v19; // r4@1
                        uint v20; // lr@1
                        uint v21; // r1@1
                        uint v22; // r5@1
                        uint v23; // r2@1
                        uint v24; // r6@1
                        uint v25; // r3@1
                        uint v26; // r12@1
                        uint v27; // r6@1
                        uint v28; // r1@1
                        uint v29; // r4@1
                        uint v30; // lr@1
                        uint v31; // r5@1
                        uint v32; // r12@1
                        uint v33; // r2@1
                        uint v34; // r3@1
                        uint v35; // r6@1
                        uint v36; // lr@1
                        uint v37; // r1@1
                        uint v38; // r4@1
                        uint v39; // r5@1
                        uint v40; // r1@1
                        uint v41; // r2@1

                        v1 = IOUtil.ReadU32LE(Data, (Y * Stride + X + Offset) - Stride);
                        v2 = IOUtil.ReadU32LE(Data, (Y * Stride + X + Offset) - Stride + 4);
                        v4 = Data[(Y * Stride + X + Offset) - 1];
                        v5 = Data[(Y * Stride + X + Offset) - Stride - 1];
                        v6 = (v4 + v5 + 1) >> 1;
                        v7 = ((byte)v1 + v4 + 2 * v5 + 2) >> 2;
                        v8 = v1 << 16 >> 24;
                        v9 = v1 << 8 >> 24;
                        v10 = (byte)v1 + v9 + 2 * v8;
                        v11 = (uint)(v5 + v8 + 2 * (byte)v1 + 2);
                        v1 >>= 24;
                        v11 >>= 2;
                        v12 = (v10 + 2) >> 2;
                        v13 = v6 | (v7 << 8) | (v11 << 16) | (v12 << 24);
                        v14 = (uint)((int)(v8 + v1 + 2 * v9 + 2) >> 2);
                        v15 = (uint)((int)(v9 + (byte)v2 + 2 * v1 + 2) >> 2);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset), v13);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + 4, (uint)(v14 | (v15 << 8) | ((v1
                                                                             + ((uint)(v2 << 16) >> 24)
                                                                             + 2 * (byte)v2
                                                                             + 2) >> 2 << 16) | (((byte)v2
                                                                                                            + ((uint)(v2 << 8) >> 24)
                                                                                                            + 2
                                                                                                            * ((uint)(v2 << 16) >> 24)
                                                                                                            + 2) >> 2 << 24)));
                        v16 = Data[(Y * Stride + X + Offset) + Stride - 1];
                        v17 = (v5 + v16 + 2 * v4 + 2) >> 2;
                        v18 = (v16 + v4 + 1) >> 1;
                        v19 = v18 | (v17 << 8) | (v6 << 16) | (v7 << 24);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride, v19);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride + 4, v11 | (v12 << 8) | (v14 << 16) | (v15 << 24));
                        v20 = Data[(Y * Stride + X + Offset) + Stride * 2 - 1];
                        v21 = (v4 + v20 + 2 * v16 + 2) >> 2;
                        v22 = (v20 + v16 + 1) >> 1;
                        v23 = v22 | (v21 << 8) | (v18 << 16) | (v17 << 24);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 2, v23);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 2 + 4, v13);
                        v25 = Data[(Y * Stride + X + Offset) + Stride * 3 - 1];
                        v26 = (v16 + v25 + 2 * v20 + 2) >> 2;
                        v27 = (v25 + v20 + 1) >> 1;
                        v28 = v27 | (v26 << 8) | (v22 << 16) | (v21 << 24);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 3, v28);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 3 + 4, v19);
                        v29 = Data[(Y * Stride + X + Offset) + Stride * 4 - 1];
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 4 + 4, v23);
                        v30 = (v20 + v29 + 2 * v25 + 2) >> 2;
                        v31 = (v29 + v25 + 1) >> 1;
                        v32 = v31 | (v30 << 8) | (v27 << 16) | (v26 << 24);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 4, v32);
                        v33 = Data[(Y * Stride + X + Offset) + Stride * 5 - 1];
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 5 + 4, v28);
                        v34 = (v25 + v33 + 2 * v29 + 2) >> 2;
                        v35 = (v33 + v29 + 1) >> 1;
                        v36 = v35 | (v34 << 8) | (v31 << 16) | (v30 << 24);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 5, v36);
                        v37 = Data[(Y * Stride + X + Offset) + Stride * 6 - 1];
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 6 + 4, v32);
                        v38 = (v29 + v37 + 2 * v33 + 2) >> 2;
                        v39 = (v37 + v33 + 1) >> 1;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 6, v39 | (v38 << 8) | (v35 << 16) | (v34 << 24));
                        v40 = ((Data[(Y * Stride + X + Offset) + Stride * 7 - 1] + v37 + 1) >> 1) | ((v33 + Data[(Y * Stride + X + Offset) + Stride * 7 - 1] + 2 * v37 + 2) >> 2 << 8) | (v39 << 16) | (v38 << 24);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 7, v40);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 7 + 4, v36);
                        return FrameUtil.GetBlockPixels8x8(Data, X, Y, Stride, Offset);
                    }
                case 6:
                    {
                        uint v0; // r11@0
                        uint v1; // r2@1
                        uint v2; // r7@1
                        uint v3; // r9@1
                        uint v4; // ST30_4@1
                        uint v5; // r10@1
                        uint v6; // ST2C_4@1
                        uint v7; // ST28_4@1
                        uint v8; // r1@1
                        uint v9; // lr@1
                        uint v10; // r4@1
                        uint v11; // r0@1
                        uint v12; // r3@1
                        uint v13; // r5@1
                        uint v14; // r12@1
                        uint v15; // r6@1
                        uint v16; // ST24_4@1
                        uint v17; // r11@1
                        uint v18; // ST1C_4@1
                        uint v19; // r7@1
                        uint v20; // ST18_4@1
                        uint v21; // r7@1
                        uint v22; // r8@1
                        uint v23; // r9@1
                        uint v24; // r10@1
                        uint v25; // ST14_4@1
                        uint v26; // ST10_4@1
                        uint v27; // r1@1
                        uint v28; // ST0C_4@1
                        uint v29; // ST08_4@1
                        uint v30; // r3@1
                        uint v31; // r12@1
                        uint v32; // r6@1
                        uint v33; // ST04_4@1
                        uint v34; // r11@1
                        uint v35; // ST00_4@1
                        uint v36; // r2@1
                        uint v37; // r2@1

                        v1 = IOUtil.ReadU32LE(Data, (Y * Stride + X + Offset) - Stride);
                        v2 = IOUtil.ReadU32LE(Data, (Y * Stride + X + Offset) - Stride + 4);
                        v3 = (byte)v1;
                        v4 = Data[(Y * Stride + X + Offset) - Stride - 1];
                        v5 = v1 << 16 >> 24;
                        v6 = ((uint)Data[(Y * Stride + X + Offset) - Stride - 1] + (byte)v1 + 1) >> 1;
                        v7 = (uint)(((byte)v1 + v5 + 1) >> 1);
                        v8 = v1 << 8 >> 24;
                        v9 = (uint)((v5 + v8 + 1) >> 1);
                        v1 >>= 24;
                        v10 = (uint)((v8 + v1 + 1) >> 1);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset), v6 | (v7 << 8) | (v9 << 16) | (v10 << 24));
                        v12 = (byte)v2;
                        v13 = (uint)((v1 + (byte)v2 + 1) >> 1);
                        v14 = v2 << 16 >> 24;
                        v15 = v2 << 8 >> 24;
                        v16 = (uint)(((byte)v2 + v14 + 1) >> 1);
                        v17 = (uint)((v14 + v15 + 1) >> 1);
                        v18 = v2 >> 24;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + 4, (uint)(v13 | (v16 << 8) | (v17 << 16) | (((v15 + (v2 >> 24) + 1) / 2) << 24)));
                        v19 = Data[(Y * Stride + X + Offset) - 1];
                        v20 = v19;
                        v21 = (v19 + v3 + 2 * v4 + 2) >> 2;
                        v22 = (uint)((v4 + v5 + 2 * v3 + 2) >> 2);
                        v23 = (uint)((v3 + v8 + 2 * v5 + 2) >> 2);
                        v24 = (uint)((v5 + v1 + 2 * v8 + 2) >> 2);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride, v21 | (v22 << 8) | (v23 << 16) | (v24 << 24));
                        v25 = (uint)((v1 + v14 + 2 * v12 + 2) >> 2);
                        v26 = (uint)((v12 + v15 + 2 * v14 + 2) >> 2);
                        v27 = (uint)((v8 + v12 + 2 * v1 + 2) >> 2);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride + 4, (uint)(v27 | (v25 << 8) | (v26 << 16) | ((v14 + v18 + 2 * v15 + 2) >> 2 << 24)));
                        v28 = Data[(Y * Stride + X + Offset) + Stride - 1];
                        v29 = (v28 + v4 + 2 * v20 + 2) >> 2;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 2, v29 | (v6 << 8) | (v7 << 16) | (v9 << 24));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 2 + 4, v10 | (v13 << 8) | (v16 << 16) | (v17 << 24));
                        v30 = Data[(Y * Stride + X + Offset) + Stride * 2 - 1];
                        v31 = (v30 + v20 + 2 * v28 + 2) >> 2;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 3, v31 | (v21 << 8) | (v22 << 16) | (v23 << 24));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 3 + 4, v24 | (v27 << 8) | (v25 << 16) | (v26 << 24));
                        v32 = Data[(Y * Stride + X + Offset) + Stride * 3 - 1];
                        v33 = (v32 + v28 + 2 * v30 + 2) >> 2;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 4, v33 | (v29 << 8) | (v6 << 16) | (v7 << 24));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 4 + 4, v9 | (v10 << 8) | (v13 << 16) | (v16 << 24));
                        v34 = Data[(Y * Stride + X + Offset) + Stride * 4 - 1];
                        v35 = (v34 + v30 + 2 * v32 + 2) >> 2;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 5, v35 | (v31 << 8) | (v21 << 16) | (v22 << 24));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 5 + 4, v23 | (v24 << 8) | (v27 << 16) | (v25 << 24));
                        v36 = Data[(Y * Stride + X + Offset) + Stride * 5 - 1];
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 6, ((v36 + v32 + 2 * v34 + 2) >> 2) | (v33 << 8) | (v29 << 16) | (v6 << 24));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 6 + 4, v7 | (v9 << 8) | (v10 << 16) | (v13 << 24));
                        v37 = ((Data[(Y * Stride + X + Offset) + Stride * 6 - 1] + v34 + 2 * v36 + 2) >> 2) | (v35 << 8) | (v31 << 16) | (v21 << 24);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 7, v37);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 7 + 4, v22 | (v23 << 8) | (v24 << 16) | (v27 << 24));
                        return FrameUtil.GetBlockPixels8x8(Data, X, Y, Stride, Offset);
                    }
                case 7://1178BC
                    {
                        uint r11_i = (uint)(Y * Stride + X + Offset);
                        uint r6_i = IOUtil.ReadU32LE(Data, (int)r11_i - Stride);
                        uint r5_i = Data[r11_i - 1];
                        uint r4_i = Data[r11_i - Stride - 1];
                        uint r3_i = r6_i & 0xFF;
                        uint r12_i = (r6_i << 16) >> 24;
                        uint r2_i = ((r4_i + r12_i + r3_i * 2) + 2) / 4;
                        uint r1_i = ((r5_i + r3_i + r4_i * 2) + 2) / 4;
                        uint lr_i = (r6_i << 8) >> 24;
                        r3_i = ((r3_i + lr_i + r12_i * 2) + 2) / 4;
                        r6_i >>= 24;
                        r12_i = ((r12_i + r6_i + lr_i * 2) + 2) / 4;
                        uint r8_i = r1_i | (r2_i << 8) | (r3_i << 16) | (r12_i << 24);
                        uint var_20 = r8_i;
                        IOUtil.WriteU32LE(Data, (int)r11_i, r8_i);
                        uint r7_i = IOUtil.ReadU32LE(Data, (int)r11_i - Stride + 4);
                        uint r0_i = r3_i | (r12_i << 8);
                        r8_i = r7_i & 0xFF;
                        uint r9_i = (r7_i << 16) >> 24;
                        lr_i = ((lr_i + r8_i + r6_i * 2) + 2) / 4;
                        uint r10_i = (r7_i << 8) >> 24;
                        r6_i = ((r6_i + r9_i + r8_i * 2) + 2) / 4;
                        r8_i = ((r8_i + r10_i + r9_i * 2) + 2) / 4;
                        r7_i = ((r9_i + (r7_i >> 24) + r10_i * 2) + 2) / 4;
                        r7_i = lr_i | (r6_i << 8) | (r8_i << 16) | (r7_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r11_i + 4, r7_i);
                        r7_i = Data[r11_i + Stride - 1];
                        r10_i = r12_i | (lr_i << 8) | (r6_i << 16);
                        r4_i = ((r7_i + r4_i + r5_i * 2) + 2) / 4;
                        r9_i = r4_i | (r1_i << 8) | (r2_i << 16) | (r3_i << 24);
                        r8_i = r10_i | (r8_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride + 4, r8_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride, r9_i);
                        r11_i += (uint)Stride;
                        r8_i = Data[r11_i + Stride - 1];
                        r6_i = r0_i | (lr_i << 16) | (r6_i << 24);
                        r5_i = ((r8_i + r5_i + r7_i * 2) + 2) / 4;
                        r10_i = r5_i | (r4_i << 8) | (r1_i << 16) | (r2_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride + 4, r6_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride, r10_i);
                        r11_i += (uint)Stride;
                        r6_i = Data[r11_i + Stride - 1];
                        r2_i |= (r3_i << 8) | (r12_i << 16) | (lr_i << 24);
                        r7_i = ((r6_i + r7_i + r8_i * 2) + 2) / 4;
                        r1_i = r7_i | (r5_i << 8) | (r4_i << 16) | (r1_i << 24);
                        r11_i += (uint)Stride;
                        IOUtil.WriteU32LE(Data, (int)r11_i + 0, r1_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + 4, r2_i);
                        r2_i = Data[r11_i + Stride - 1];
                        r3_i = ((r2_i + r8_i + r6_i * 2) + 2) / 4;
                        r12_i = r3_i | (r7_i << 8) | (r5_i << 16) | (r4_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride, r12_i);
                        r11_i += (uint)Stride;
                        r8_i = var_20;
                        IOUtil.WriteU32LE(Data, (int)r11_i + 4, r8_i);
                        r12_i = Data[r11_i + Stride - 1];
                        lr_i = ((r12_i + r6_i + r2_i * 2) + 2) / 4;
                        r4_i = lr_i | (r3_i << 8) | (r7_i << 16) | (r5_i << 24);
                        r11_i += (uint)Stride;
                        IOUtil.WriteU32LE(Data, (int)r11_i + 0, r4_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + 4, r9_i);
                        r4_i = Data[r11_i + Stride - 1];
                        r11_i += (uint)Stride;
                        r2_i = ((r4_i + r2_i + r12_i * 2) + 2) / 4;
                        r5_i = r2_i | (lr_i << 8) | (r3_i << 16) | (r7_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r11_i + 0, r5_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + 4, r10_i);
                        r5_i = Data[r11_i + Stride - 1];
                        r12_i = ((r5_i + r12_i + r4_i * 2) + 2) / 4;
                        r2_i = r12_i | (r2_i << 8) | (lr_i << 16) | (r3_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride, r2_i);
                        IOUtil.WriteU32LE(Data, (int)r11_i + Stride + 4, r1_i);
                        r11_i -= (uint)Stride * 6;
                        return FrameUtil.GetBlockPixels8x8(Data, X, Y, Stride, Offset);
                    }
                case 8://117AF4
                    {
                        uint r0_i = (uint)(Y * Stride + X + Offset);
                        uint r2_i = IOUtil.ReadU32LE(Data, (int)r0_i - Stride);
                        uint r11_i = (r2_i << 16) >> 24;
                        uint r10_i = r2_i & 0xFF;
                        uint r12_i = ((r10_i + r11_i) + 1) / 2;
                        uint r1_i = (r2_i << 8) >> 24;
                        uint var_s20 = r10_i;
                        r10_i = ((r11_i + r1_i) + 1) / 2;
                        r2_i >>= 24;
                        uint var_s1C = r11_i;
                        r11_i = ((r1_i + r2_i) + 1) / 2;
                        uint r9_i = r0_i - (uint)Stride;
                        uint var_s14 = r11_i;
                        uint var_s18 = r10_i;
                        uint r4_i = IOUtil.ReadU32LE(Data, (int)r9_i + 4);
                        uint r3_i = r4_i & 0xFF;
                        uint r5_i = ((r2_i + r3_i) + 1) / 2;
                        r12_i |= (r10_i << 8) | (r11_i << 16) | (r5_i << 24);
                        uint var_s10 = r5_i;
                        IOUtil.WriteU32LE(Data, (int)r0_i, r12_i);
                        r12_i = (r4_i << 16) >> 24;
                        uint r8_i = ((r3_i + r12_i) + 1) / 2;
                        uint lr_i = (r4_i << 8) >> 24;
                        uint r6_i = ((r12_i + lr_i) + 1) / 2;
                        r4_i >>= 24;
                        uint var_sC = r8_i;
                        r8_i = IOUtil.ReadU32LE(Data, (int)r9_i + 8);
                        uint r7_i = ((lr_i + r4_i) + 1) / 2;
                        r5_i = r8_i & 0xFF;
                        r10_i = var_sC;
                        r9_i = ((r4_i + r5_i) + 1) / 2;
                        r10_i |= (r6_i << 8) | (r7_i << 16) | (r9_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + 4, r10_i);
                        r10_i = var_s20;
                        r11_i = var_s1C;
                        r10_i = ((r10_i + r1_i + r11_i * 2) + 2) / 4;
                        uint var_s8 = r10_i;
                        r10_i = ((r11_i + r2_i + r1_i * 2) + 2) / 4;
                        r1_i = ((r1_i + r3_i + r2_i * 2) + 2) / 4;
                        uint var_0 = r1_i;
                        uint var_s4 = r10_i;
                        r1_i = ((r2_i + r12_i + r3_i * 2) + 2) / 4;
                        r2_i = var_s8 | (var_s4 << 8) | (var_0 << 16) | (r1_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + Stride, r2_i);
                        r0_i += (uint)Stride;
                        r2_i = ((r3_i + lr_i + r12_i * 2) + 2) / 4;
                        r3_i = ((r12_i + r4_i + lr_i * 2) + 2) / 4;
                        lr_i = ((lr_i + r5_i + r4_i * 2) + 2) / 4;
                        r12_i = (r8_i << 16) >> 24;
                        r4_i = ((r4_i + r12_i + r5_i * 2) + 2) / 4;
                        r10_i = r2_i | (r3_i << 8) | (lr_i << 16) | (r4_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + 4, r10_i);
                        r10_i = var_s18 | (var_s14 << 8) | (var_s10 << 16) | (var_sC << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + Stride, r10_i);
                        r0_i += (uint)Stride;
                        r11_i = ((r5_i + r12_i) + 1) / 2;
                        r10_i = r6_i | (r7_i << 8) | (r9_i << 16) | (r11_i << 24);
                        uint var_4 = r11_i;
                        IOUtil.WriteU32LE(Data, (int)r0_i + 4, r10_i);
                        r10_i = var_s4 | (var_0 << 8) | (r1_i << 16) | (r2_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + Stride, r10_i);
                        r0_i += (uint)Stride;
                        r10_i = (r8_i << 8) >> 24;
                        r11_i = ((r5_i + r10_i + r12_i * 2) + 2) / 4;
                        r5_i = r3_i | (lr_i << 8) | (r4_i << 16) | (r11_i << 24);
                        uint var_8 = r11_i;
                        IOUtil.WriteU32LE(Data, (int)r0_i + 4, r5_i);
                        r5_i = var_s14 | (var_s10 << 8) | (var_sC << 16) | (r6_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + Stride, r5_i);
                        r0_i += (uint)Stride;
                        r11_i = ((r12_i + r10_i) + 1) / 2;
                        uint var_C = r11_i;
                        r11_i = var_4;
                        r5_i = r7_i | (r9_i << 8) | (r11_i << 16) | (var_C << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + 4, r5_i);
                        r11_i = var_0;
                        r5_i = r11_i | (r1_i << 8) | (r2_i << 16) | (r3_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + Stride, r5_i);
                        r0_i += (uint)Stride;
                        r5_i = r8_i >> 24;
                        r12_i = ((r12_i + r5_i + r10_i * 2) + 2) / 4;
                        r11_i = var_8;
                        r8_i = lr_i | (r4_i << 8) | (r11_i << 16) | (r12_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + 4, r8_i);
                        r8_i = var_sC;
                        r11_i = var_s10;
                        r1_i |= r2_i << 8;
                        r6_i = r11_i | (r8_i << 8) | (r6_i << 16) | (r7_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + Stride, r6_i);
                        r0_i += (uint)Stride;
                        r6_i = ((r10_i + r5_i) + 1) / 2;
                        r1_i |= (r3_i << 16) | (lr_i << 24);
                        r6_i = r9_i | (var_4 << 8) | (var_C << 16) | (r6_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + Stride, r1_i);
                        IOUtil.WriteU32LE(Data, (int)r0_i + 4, r6_i);
                        r11_i = r0_i - (uint)Stride * 7;
                        r1_i = ((Data[r11_i + 0xC] + r10_i + r5_i * 2) + 2) / 4;
                        r11_i = var_8;
                        r1_i = r4_i | (r11_i << 8) | (r12_i << 16) | (r1_i << 24);
                        IOUtil.WriteU32LE(Data, (int)r0_i + Stride + 4, r1_i);
                        r11_i = r0_i - (uint)Stride * 6;
                        return FrameUtil.GetBlockPixels8x8(Data, X, Y, Stride, Offset);
                    }
            }
            return null;
        }

        private static unsafe byte[] GetCompvals4x4(int BlockType, byte[] Data, int X, int Y, int Stride, int Offset)
        {
            switch (BlockType)
            {
                case 10://117E34
                    {
                        byte[] ThatBlock = FrameUtil.GetBlockPixels4x4(Data, X, Y - 4, Stride, Offset);
                        byte[] CompVals = new byte[16];
                        fixed (byte* pCompVals = &CompVals[0], pThatBlock = &ThatBlock[3 * 4])
                        {
                            uint* pLCompVals = (uint*)pCompVals;
                            uint val = *((uint*)pThatBlock);
                            pLCompVals[0] = val;
                            pLCompVals[1] = val;
                            pLCompVals[2] = val;
                            pLCompVals[3] = val;
                        }
                        return CompVals;
                    }
                case 11://117E50
                    {
                        byte[] ThatBlock = FrameUtil.GetBlockPixels4x4(Data, X - 4, Y, Stride, Offset);
                        byte[] CompVals = new byte[16];
                        for (int y = 0; y < 4; y++)
                        {
                            byte val = ThatBlock[y * 4 + 3];
                            for (int x = 0; x < 4; x++)
                            {
                                CompVals[y * 4 + x] = val;
                            }
                        }
                        return CompVals;
                    }
                /*case 12://117E98
                    {
                        sub_117E98(ref nrBitsRemaining, ref r3, Dst, Offset);
                        break;
                    }*/
                case 13://1180FC
                    {
                        byte[] CompVals = new byte[16];
                        int r8 = 0;
                        if (X > 0) r8 += 8;
                        if (Y > 0) r8 += 4;
                        switch (r8 / 4)
                        {
                            case 0://001170E4
                                {
                                    for (int i = 0; i < 16; i++) CompVals[i] = 0x80;
                                    break;
                                }
                            case 1:
                                {
                                    byte[] ThatBlock = FrameUtil.GetBlockPixels4x4(Data, X, Y - 4, Stride, Offset);
                                    int sum = 0;
                                    for (int x3 = 0; x3 < 4; x3++)
                                    {
                                        sum += (int)ThatBlock[3 * 4 + x3];
                                    }
                                    sum = (sum + 2) / 4;
                                    for (int i = 0; i < 16; i++) CompVals[i] = (byte)sum;
                                    break;
                                }
                            case 2:
                                {
                                    byte[] ThatBlock = FrameUtil.GetBlockPixels4x4(Data, X - 4, Y, Stride, Offset);
                                    int sum = 0;
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        sum += (int)ThatBlock[y3 * 4 + 3];
                                    }
                                    sum = (sum + 2) / 4;
                                    for (int i = 0; i < 16; i++) CompVals[i] = (byte)sum;
                                    break;
                                }
                            case 3://00116EC0
                                {
                                    byte[] ThatBlock1 = FrameUtil.GetBlockPixels4x4(Data, X, Y - 4, Stride, Offset);
                                    byte[] ThatBlock2 = FrameUtil.GetBlockPixels4x4(Data, X - 4, Y, Stride, Offset);
                                    int sum = 0;
                                    for (int x3 = 0; x3 < 4; x3++)
                                    {
                                        sum += (int)ThatBlock1[3 * 4 + x3];
                                    }
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        sum += (int)ThatBlock2[y3 * 4 + 3];
                                    }
                                    sum = (sum + 4) / 8;
                                    for (int i = 0; i < 16; i++) CompVals[i] = (byte)sum;
                                    break;
                                }
                            default:
                                break;
                        }
                        return CompVals;
                    }
                case 14://debug confirmed okay
                    {
                        //uint v0; // r11@0
                        uint v1; // r5@1
                        uint v2; // r6@1
                        uint v3; // r7@1
                        //uint v4; // r11@1
                        uint v5; // t1@1
                        uint v6; // r4@1
                        uint v7; // r5@1
                        uint v8; // t1@1
                        uint v9; // r6@1
                        uint v10; // r4@1
                        uint v11; // r5@1
                        uint v12; // r6@1
                        uint v13; // r7@1
                        uint v14; // r8@1

                        v1 = Data[(Y * Stride + X + Offset) - 1];
                        v2 = Data[(Y * Stride + X + Offset) + Stride - 1];
                        v5 = Data[(Y * Stride + X + Offset) + Stride * 2 - 1];
                        //v4 = v0 + 511;
                        v3 = v5;
                        v6 = ((v1 + v2 + 1) >> 1) | ((v1 + 2 * v2 + v5 + 2) >> 2 << 8);
                        v7 = (v2 + v5 + 1) >> 1;
                        v8 = Data[(Y * Stride + X + Offset) + Stride * 3 - 1];
                        //v4 += 256;
                        v9 = (v2 + 2 * v3 + v8 + 2) >> 2;
                        v10 = v6 | (v7 << 16) | (v9 << 24);
                        v11 = v7 | (v9 << 8);
                        v12 = (v3 + v8 + 1) >> 1;
                        v13 = (v3 + 2 * v8 + v8 + 2) >> 2;
                        v14 = v8 | (v8 << 8);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 3, v14 | (v14 << 16));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 2, v12 | (v13 << 8) | (v14 << 16));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 1, v11 | (v12 << 16) | (v13 << 24));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 0, v10);
                        return FrameUtil.GetBlockPixels4x4(Data, X, Y, Stride, Offset);
                    }
                case 15:
                    {
                        uint v0; // r11@0
                        uint v1; // r8@1
                        uint v2; // r7@1
                        uint v3; // r9@1
                        uint v4; // lr@1
                        uint v5; // r4@1
                        uint v6; // r9@1
                        uint v7; // r8@1
                        uint v8; // r12@1
                        //uint v9; // r11@1
                        uint v10; // lr@1
                        uint v11; // r7@1
                        uint v12; // r4@1

                        v1 = Data[(Y * Stride + X + Offset) - Stride - 1];
                        v2 = Data[(Y * Stride + X + Offset) - 1];
                        v3 = IOUtil.ReadU32LE(Data, (Y * Stride + X + Offset) - Stride);
                        v4 = (v1 + v2 + 1) >> 1;
                        v5 = (v2 + 2 * v1 + (byte)v3 + 2) >> 2;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset), (uint)(
                            v4 | (v5 << 8) | ((v1 + 2 * (byte)v3 + ((uint)(v3 << 16) >> 24) + 2) >> 2 << 16) | (((byte)v3 + 2 * ((uint)(v3 << 16) >> 24) + ((uint)(v3 << 8) >> 24) + 2) >> 2 << 24)));
                        v6 = Data[(Y * Stride + X + Offset) + Stride - 1];
                        v7 = (v1 + 2 * v2 + v6 + 2) >> 2;
                        v8 = (v2 + v6 + 1) >> 1;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride, v8 | (v7 << 8) | (v4 << 16) | (v5 << 24));
                        //v9 = v0 + 256;
                        v10 = Data[(Y * Stride + X + Offset) + Stride * 2 - 1];
                        v11 = (v2 + 2 * v6 + v10 + 2) >> 2;
                        v12 = (v6 + v10 + 1) >> 1;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 2, v12 | (v11 << 8) | (v8 << 16) | (v7 << 24));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 3,
                            ((v10 + Data[(Y * Stride + X + Offset) + Stride * 3 - 1] + 1) >> 1) | ((v6 + 2 * v10 + Data[(Y * Stride + X + Offset) + Stride * 3 - 1] + 2) >> 2 << 8) | (v12 << 16) | (v11 << 24));
                        return FrameUtil.GetBlockPixels4x4(Data, X, Y, Stride, Offset);
                    }
                case 16://1182C4
                    {
                        /*int r11_i = (Y * Stride + X + Offset);
                        uint lr_i = IOUtil.ReadU32LE(Data, r11_i - Stride);
                        uint r3_i = Data[r11_i - Stride - 1];
                        uint r1_i = lr_i & 0xFF;*/
                        uint v0; // r11@0
                        uint v1; // lr@1
                        uint v2; // r3@1
                        uint v3; // r1@1
                        uint v4; // r7@1
                        uint v5; // r4@1
                        uint v6; // r2@1
                        uint v7; // r5@1
                        uint v8; // r12@1
                        uint v9; // r6@1
                        uint v10; // lr@1
                        uint v11; // r9@1
                        uint v12; // r8@1
                        uint v13; // r1@1
                        uint v14; // r11@1
                        uint v15; // r2@1
                        v1 = IOUtil.ReadU32LE(Data, (Y * Stride + X + Offset) - Stride);
                        v2 = Data[(Y * Stride + X + Offset) - Stride - 1];
                        v3 = v1 & 0xFF;
                        v4 = v1 >> 24;
                        v5 = (v2 + (byte)v1 + 1) >> 1;
                        v6 = v1 << 16 >> 24;
                        v7 = ((byte)v1 + v6 + 1) >> 1;
                        v8 = v1 << 8 >> 24;
                        v9 = (v6 + v8 + 1) >> 1;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset), v5 | (v7 << 8) | (v9 << 16) | ((v8 + (v1 >> 24) + 1) >> 1 << 24));
                        v10 = Data[(Y * Stride + X + Offset) - 1];
                        v11 = (v2 + 2 * v3 + v6 + 2) >> 2;
                        v12 = (v10 + 2 * v2 + v3 + 2) >> 2;
                        v13 = (v3 + 2 * v6 + v8 + 2) >> 2;
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride, v12 | (v11 << 8) | (v13 << 16) | ((v6 + 2 * v8 + v4 + 2) >> 2 << 24));
                        // *(v0 + 256) = v12 | (v11 << 8) | (v13 << 16) | ((v6 + 2 * v8 + v4 + 2) >> 2 << 24);
                        //v14 = v0 + 256;
                        v15 = Data[(Y * Stride + X + Offset) + Stride - 1];//*(v14 - 1);
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 2, ((v2 + 2 * v10 + v15 + 2) >> 2) | (v5 << 8) | (v7 << 16) | (v9 << 24));
                        IOUtil.WriteU32LE(Data, (Y * Stride + X + Offset) + Stride * 3, ((v10 + 2 * v15 + Data[(Y * Stride + X + Offset) + Stride * 2 - 1] + 2) >> 2) | (v12 << 8) | (v11 << 16) | (v13 << 24));
                        //*(v14 + 256) = ((v2 + 2 * v10 + v15 + 2) >> 2) | (v5 << 8) | (v7 << 16) | (v9 << 24);
                        // *(v14 + 512) = ((v10 + 2 * v15 + *(v14 + 255) + 2) >> 2) | (v12 << 8) | (v11 << 16) | (v13 << 24);
                        return FrameUtil.GetBlockPixels4x4(Data, X, Y, Stride, Offset);
                    }
                case 17://1183CC
                    {
                        int r11_i = (Y * Stride + X + Offset);
                        uint r7_i = IOUtil.ReadU32LE(Data, r11_i - Stride);
                        uint r12_i = Data[r11_i - Stride - 1];
                        uint lr_i = r7_i & 0xFF;
                        uint r9_i = Data[r11_i - 1];
                        uint r4_i = (r7_i << 16) >> 24;
                        uint r5_i = ((r12_i + lr_i * 2 + r4_i) + 2) / 4;
                        uint r8_i = ((r9_i + r12_i * 2 + lr_i) + 2) / 4;
                        uint r6_i = (r7_i << 8) >> 24;
                        lr_i = ((lr_i + r4_i * 2 + r6_i) + 2) / 4;
                        r7_i = ((r4_i + r6_i * 2 + (r7_i >> 24)) + 2) / 4;
                        r7_i = r8_i | (r5_i << 8) | (lr_i << 16) | (r7_i << 24);
                        IOUtil.WriteU32LE(Data, r11_i, r7_i);
                        r7_i = Data[r11_i + Stride - 1];
                        r12_i = ((r7_i + r9_i * 2 + r12_i) + 2) / 4;
                        lr_i = r12_i | (r8_i << 8) | (r5_i << 16) | (lr_i << 24);
                        IOUtil.WriteU32LE(Data, r11_i + Stride, lr_i);
                        r11_i += Stride;
                        lr_i = Data[r11_i + Stride - 1];
                        r9_i = ((lr_i + r7_i * 2 + r9_i) + 2) / 4;
                        r4_i = r9_i | (r12_i << 8) | (r8_i << 16) | (r5_i << 24);
                        IOUtil.WriteU32LE(Data, r11_i + Stride, r4_i);
                        r11_i += Stride;
                        r4_i = Data[r11_i + Stride - 1];
                        r7_i = ((r4_i + lr_i * 2 + r7_i) + 2) / 4;
                        r7_i = r7_i | (r9_i << 8) | (r12_i << 16) | (r8_i << 24);
                        IOUtil.WriteU32LE(Data, r11_i + Stride, r7_i);
                        r11_i -= Stride * 2;
                        return FrameUtil.GetBlockPixels4x4(Data, X, Y, Stride, Offset);
                    }
                case 18://1184B4
                    {
                        int r11_i = (Y * Stride + X + Offset);
                        uint r9_i = IOUtil.ReadU32LE(Data, r11_i - Stride);
                        uint r2_i = (uint)r11_i - (uint)Stride;
                        uint r6_i = (r9_i << 16) >> 24;
                        uint r7_i = r9_i & 0xFF;
                        uint r8_i = ((r7_i + r6_i) + 1) / 2;
                        uint r12_i = (r9_i << 8) >> 24;
                        uint lr_i = ((r6_i + r12_i) + 1) / 2;
                        r9_i >>= 24;
                        uint r4_i = ((r12_i + r9_i) + 1) / 2;
                        uint r3_i = IOUtil.ReadU32LE(Data, (int)r2_i + 4);
                        r7_i = r7_i + r6_i * 2 + r12_i;
                        r2_i = r3_i & 0xFF;
                        uint r5_i = ((r9_i + r2_i) + 1) / 2;
                        r8_i |= (lr_i << 8) | (r4_i << 16) | (r5_i << 24);
                        IOUtil.WriteU32LE(Data, r11_i, r8_i);
                        r8_i = (r7_i + 2) / 4;
                        r7_i = ((r12_i + r9_i * 2 + r2_i) + 2) / 4;
                        r6_i = ((r6_i + r12_i * 2 + r9_i) + 2) / 4;
                        r12_i = (r3_i << 16) >> 24;
                        r9_i = ((r9_i + r2_i * 2 + r12_i) + 2) / 4;
                        r8_i |= (r6_i << 8) | (r7_i << 16) | (r9_i << 24);
                        IOUtil.WriteU32LE(Data, r11_i + Stride, r8_i);
                        r11_i += Stride;
                        r8_i = ((r2_i + r12_i) + 1) / 2;
                        r2_i = ((r2_i + r12_i * 2 + ((r3_i << 8) >> 24)) + 2) / 4;
                        lr_i |= (r4_i << 8) | (r5_i << 16) | (r8_i << 24);
                        r9_i = r6_i | (r7_i << 8) | (r9_i << 16) | (r2_i << 24);
                        IOUtil.WriteU32LE(Data, r11_i + Stride, lr_i);
                        r11_i += Stride;
                        IOUtil.WriteU32LE(Data, r11_i + Stride, r9_i);
                        r11_i -= Stride * 2;
                        return FrameUtil.GetBlockPixels4x4(Data, X, Y, Stride, Offset);
                    }
                case 19: break;
                default:
                    break;
            }
            return null;
        }

        public static byte[] PredictIntraPlane16x16(byte[] Data, int Offset, int Stride, int Param)
        {
            int r6 = Param;
            byte[] vals = new byte[16];
            Array.Copy(Data, Offset - Stride, vals, 0, 16);
            int r4 = Data[Offset + Stride * 15 - 1];
            int r10 = vals[15];
            int r5 = ((r4 + r10) + 1) >> 1;
            r5 += r6 * 2;
            r6 = r5 - r4;
            r6++;
            r4 <<= 3;
            int[] sp_min0x80 = new int[32];
            for (int i = 0; i < 16; i++)
            {
                r4 += r6 >> 1;
                sp_min0x80[i * 2] = vals[i] * 64;
                sp_min0x80[i * 2 + 1] = (r4 - vals[i] * 8) + 1;
            }
            int r9 = r5 - r10;
            r9++;
            r10 <<= 3;
            uint lr = 16;
            while (true)
            {
                r10 += r9 >> 1;
                int r8 = Data[Offset - 1];
                int r7 = r10 - (r8 << 3);
                r7++;
                r8 <<= 6;
                int r0_i = sp_min0x80[0];
                int r1_i = sp_min0x80[1];
                int r2_i = sp_min0x80[2];
                int r3_i2 = sp_min0x80[3];
                r4 = sp_min0x80[4];
                r5 = sp_min0x80[5];
                r6 = sp_min0x80[6];
                int r12 = sp_min0x80[7];
                r0_i += r1_i >> 1;
                r2_i += r3_i2 >> 1;
                r4 += r5 >> 1;
                r6 += r12 >> 1;
                sp_min0x80[0] = r0_i;
                sp_min0x80[2] = r2_i;
                sp_min0x80[4] = r4;
                sp_min0x80[6] = r6;
                r8 += r7 >> 1;
                r5 = ((r0_i + r8) + 64) >> 7;
                r8 += r7 >> 1;
                r12 = ((r2_i + r8) + 64) >> 7;
                r5 |= (r12 << 8);
                r8 += r7 >> 1;
                r12 = ((r4 + r8) + 64) >> 7;
                r5 |= (r12 << 16);
                r8 += r7 >> 1;
                r12 = ((r6 + r8) + 64) >> 7;
                r5 |= (r12 << 24);
                IOUtil.WriteU32LE(Data, Offset, (uint)r5);
                Offset += 4;
                r0_i = sp_min0x80[8];
                r1_i = sp_min0x80[9];
                r2_i = sp_min0x80[10];
                r3_i2 = sp_min0x80[11];
                r4 = sp_min0x80[12];
                r5 = sp_min0x80[13];
                r6 = sp_min0x80[14];
                r12 = sp_min0x80[15];
                r0_i += r1_i >> 1;
                r2_i += r3_i2 >> 1;
                r4 += r5 >> 1;
                r6 += r12 >> 1;
                sp_min0x80[8] = r0_i;
                sp_min0x80[10] = r2_i;
                sp_min0x80[12] = r4;
                sp_min0x80[14] = r6;
                r8 += r7 >> 1;
                r5 = ((r0_i + r8) + 64) >> 7;
                r8 += r7 >> 1;
                r12 = ((r2_i + r8) + 64) >> 7;
                r5 |= (r12 << 8);
                r8 += r7 >> 1;
                r12 = ((r4 + r8) + 64) >> 7;
                r5 |= (r12 << 16);
                r8 += r7 >> 1;
                r12 = ((r6 + r8) + 64) >> 7;
                r5 |= (r12 << 24);
                IOUtil.WriteU32LE(Data, Offset, (uint)r5);
                Offset += 4;
                r0_i = sp_min0x80[16];
                r1_i = sp_min0x80[17];
                r2_i = sp_min0x80[18];
                r3_i2 = sp_min0x80[19];
                r4 = sp_min0x80[20];
                r5 = sp_min0x80[21];
                r6 = sp_min0x80[22];
                r12 = sp_min0x80[23];
                r0_i += r1_i >> 1;
                r2_i += r3_i2 >> 1;
                r4 += r5 >> 1;
                r6 += r12 >> 1;
                sp_min0x80[16] = r0_i;
                sp_min0x80[18] = r2_i;
                sp_min0x80[20] = r4;
                sp_min0x80[22] = r6;
                r8 += r7 >> 1;
                r5 = ((r0_i + r8) + 64) >> 7;
                r8 += r7 >> 1;
                r12 = ((r2_i + r8) + 64) >> 7;
                r5 |= (r12 << 8);
                r8 += r7 >> 1;
                r12 = ((r4 + r8) + 64) >> 7;
                r5 |= (r12 << 16);
                r8 += r7 >> 1;
                r12 = ((r6 + r8) + 64) >> 7;
                r5 |= (r12 << 24);
                IOUtil.WriteU32LE(Data, Offset, (uint)r5);
                Offset += 4;
                r0_i = sp_min0x80[24];
                r1_i = sp_min0x80[25];
                r2_i = sp_min0x80[26];
                r3_i2 = sp_min0x80[27];
                r4 = sp_min0x80[28];
                r5 = sp_min0x80[29];
                r6 = sp_min0x80[30];
                r12 = sp_min0x80[31];
                r0_i += r1_i >> 1;
                r2_i += r3_i2 >> 1;
                r4 += r5 >> 1;
                r6 += r12 >> 1;
                sp_min0x80[24] = r0_i;
                sp_min0x80[26] = r2_i;
                sp_min0x80[28] = r4;
                sp_min0x80[30] = r6;
                r8 += r7 >> 1;
                r5 = ((r0_i + r8) + 64) >> 7;
                r8 += r7 >> 1;
                r12 = ((r2_i + r8) + 64) >> 7;
                r5 |= (r12 << 8);
                r8 += r7 >> 1;
                r12 = ((r4 + r8) + 64) >> 7;
                r5 |= (r12 << 16);
                r8 += r7 >> 1;
                r12 = ((r6 + r8) + 64) >> 7;
                r5 |= (r12 << 24);
                IOUtil.WriteU32LE(Data, Offset, (uint)r5);
                Offset += Stride - 12;
                lr--;
                if (lr <= 0) break;
            }
            Offset -= Stride * 16;
            return FrameUtil.GetBlockPixels16x16(Data, 0, 0, Stride, Offset);
        }

        public static byte[] PredictIntraPlane8x8(byte[] Data, int Offset, int Stride, int Param)
        {
            int r6 = Param;
            byte[] vals = new byte[8];
            Array.Copy(Data, Offset - Stride, vals, 0, 8);
            int r4 = Data[Offset + Stride * 7 - 1];
            int r10 = vals[7];
            int r5 = ((r4 + r10) + 1) >> 1;
            r5 += r6 * 2;
            r6 = r5 - r4;
            r4 *= 8;
            int[] sp_min0x40 = new int[16];
            for (int i = 0; i < 8; i++)
            {
                r4 += r6;
                sp_min0x40[i * 2] = vals[i] * 64;
                sp_min0x40[i * 2 + 1] = r4 - vals[i] * 8;
            }
            int r9 = r5 - r10;
            r10 <<= 3;
            uint lr = 8;
            while (true)
            {
                r10 += r9;
                int r8 = Data[Offset - 1];
                int r7 = r10 - r8 * 8;
                r8 *= 64;
                int r0_i = sp_min0x40[0];
                int r1_i = sp_min0x40[1];
                int r2 = sp_min0x40[2];
                int r3_i = sp_min0x40[3];
                r4 = sp_min0x40[4];
                r5 = sp_min0x40[5];
                r6 = sp_min0x40[6];
                int r12 = sp_min0x40[7];
                r0_i += r1_i;
                r2 += r3_i;
                r4 += r5;
                r6 += r12;
                sp_min0x40[0] = r0_i;
                sp_min0x40[2] = r2;
                sp_min0x40[4] = r4;
                sp_min0x40[6] = r6;
                r8 += r7;
                uint r5_i = (uint)(((r0_i + r8) + 64) >> 7);
                r8 += r7;
                r5_i |= ((uint)(((r2 + r8) + 64) >> 7) << 8);
                r8 += r7;
                r5_i |= ((uint)(((r4 + r8) + 64) >> 7) << 16);
                r8 += r7;
                r5_i |= ((uint)(((r6 + r8) + 64) >> 7) << 24);
                IOUtil.WriteU32LE(Data, Offset, r5_i);
                Offset += 4;
                r0_i = sp_min0x40[8];
                r1_i = sp_min0x40[9];
                r2 = sp_min0x40[10];
                r3_i = sp_min0x40[11];
                r4 = sp_min0x40[12];
                r5 = sp_min0x40[13];
                r6 = sp_min0x40[14];
                r12 = sp_min0x40[15];
                r0_i += r1_i;
                r2 += r3_i;
                r4 += r5;
                r6 += r12;
                sp_min0x40[8] = r0_i;
                sp_min0x40[10] = r2;
                sp_min0x40[12] = r4;
                sp_min0x40[14] = r6;
                r8 += r7;
                r5_i = (uint)(((r0_i + r8) + 64) >> 7);
                r8 += r7;
                r5_i |= ((uint)(((r2 + r8) + 64) >> 7) << 8);
                r8 += r7;
                r5_i |= ((uint)(((r4 + r8) + 64) >> 7) << 16);
                r8 += r7;
                r5_i |= ((uint)(((r6 + r8) + 64) >> 7) << 24);
                IOUtil.WriteU32LE(Data, Offset, r5_i);
                Offset += Stride - 4;
                lr--;
                if (lr <= 0) break;
            }
            Offset -= Stride * 8;
            return FrameUtil.GetBlockPixels8x8(Data, 0, 0, Stride, Offset);
        }

        private static byte[] PredictIntraPlane4x4(byte[] Data, int Offset, int Stride, int Param)
        {
            int r6 = Param;
            uint r0 = IOUtil.ReadU32LE(Data, Offset - Stride);
            int r4 = Data[Offset + Stride * 3 - 1];
            int r10 = (int)(r0 >> 24);
            int r5 = ((r4 + r10) + 1) >> 1;
            r5 += r6 * 2;
            r6 = r5 - r4;
            r4 <<= 2;
            r4 += r6;
            int r7 = (int)(r0 & 0xFF);
            int r8 = r4 - (r7 << 2);
            r7 <<= 4;
            r4 += r6;
            int r9 = (int)((r0 >> 8) & 0xFF);
            int r12 = r4 - (r9 << 2);
            r9 <<= 4;
            int[] sp_min0x20 = new int[8];
            sp_min0x20[0] = r7;
            sp_min0x20[1] = r8;
            sp_min0x20[2] = r9;
            sp_min0x20[3] = r12;
            r4 += r6;
            r7 = (int)((r0 >> 16) & 0xFF);
            r8 = r4 - (r7 << 2);
            r7 <<= 4;
            r4 += r6;
            r9 = (int)((r0 >> 24) & 0xFF);
            r12 = r4 - (r9 << 2);
            r9 <<= 4;
            sp_min0x20[4] = r7;
            sp_min0x20[5] = r8;
            sp_min0x20[6] = r9;
            sp_min0x20[7] = r12;
            r9 = r5 - r10;
            r10 <<= 2;
            uint lr = 4;
            while (true)
            {
                r10 += r9;
                r8 = Data[Offset - 1];
                r7 = r10 - (r8 << 2);
                r8 <<= 4;
                int r0_i = sp_min0x20[0];
                int r1 = sp_min0x20[1];
                int r2 = sp_min0x20[2];
                int r3_i = sp_min0x20[3];
                r4 = sp_min0x20[4];
                r5 = sp_min0x20[5];
                r6 = sp_min0x20[6];
                r12 = sp_min0x20[7];
                r0_i += r1;
                r2 += r3_i;
                r4 += r5;
                r6 += r12;
                sp_min0x20[0] = r0_i;
                sp_min0x20[2] = r2;
                sp_min0x20[4] = r4;
                sp_min0x20[6] = r6;
                r8 += r7;
                uint r5_i = (uint)(((r0_i + r8) + 16) >> 5);
                r8 += r7;
                r5_i |= (uint)((((r2 + r8) + 16) >> 5) << 8);
                r8 += r7;
                r5_i |= (uint)((((r4 + r8) + 16) >> 5) << 16);
                r8 += r7;
                r5_i |= (uint)((((r6 + r8) + 16) >> 5) << 24);
                IOUtil.WriteU32LE(Data, Offset, r5_i);
                Offset += Stride;
                lr--;
                if (lr <= 0) break;
            }
            Offset -= Stride * 4;
            return FrameUtil.GetBlockPixels4x4(Data, 0, 0, Stride, Offset);
        }
    }
}
