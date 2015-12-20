using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using MobiclipDecoder.IO;

namespace MobiclipDecoder.Mobi
{
    public class MobiclipEncoder
    {
        private static byte[] DeZigZagTable8x8 =
        {
	        0x00, 0x01, 0x08, 0x10, 0x09, 0x02, 0x03, 0x0A, 0x11, 0x18, 0x20, 0x19,
	        0x12, 0x0B, 0x04, 0x05, 0x0C, 0x13, 0x1A, 0x21, 0x28, 0x30, 0x29, 0x22,
	        0x1B, 0x14, 0x0D, 0x06, 0x07, 0x0E, 0x15, 0x1C, 0x23, 0x2A, 0x31, 0x38,
	        0x39, 0x32, 0x2B, 0x24, 0x1D, 0x16, 0x0F, 0x17, 0x1E, 0x25, 0x2C, 0x33,
	        0x3A, 0x3B, 0x34, 0x2D, 0x26, 0x1F, 0x27, 0x2E, 0x35, 0x3C, 0x3D, 0x36,
	        0x2F, 0x37, 0x3E, 0x3F
        };

        private static byte[] ZigZagTable8x8 =
        {
                0, 1, 5, 6,14,15,27,28, 
               2, 4, 7,13,16,26,29,42, 
               3, 8,12,17,25,30,41,43, 
               9,11,18,24,31,40,44,53, 
              10,19,23,32,39,45,52,54, 
              20,22,33,38,46,51,55,60, 
              21,34,37,47,50,56,59,61, 
              35,36,48,49,57,58,62,63  
        };

        public static unsafe byte[] Encode(Bitmap Frame)
        {
            int Stride = 1024;
            if (Frame.Width <= 256) Stride = 256;
            else if (Frame.Width <= 512) Stride = 512;
            byte[] YDec = new byte[Stride * Frame.Height];
            byte[] UVDec = new byte[Stride * Frame.Height / 2];
            int Quantizer = 0xC;
            BitWriter b = new BitWriter();
            b.WriteBits(1, 1);//Interframe
            b.WriteBits(1, 1);//YUV format
            b.WriteBits(0, 1);//Table
            float[] QuantizationTable = GetQuantizationTable(ref Quantizer);
            b.WriteBits((uint)Quantizer, 6);//Quantizer
            BitmapData d = Frame.LockBits(new Rectangle(0, 0, Frame.Width, Frame.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            for (int y = 0; y < Frame.Height; y += 16)
            {
                for (int x = 0; x < Frame.Width; x += 16)
                {
                    b.WriteBits(0, 1);//Func
                    b.WriteVarIntUnsigned(2);//always use dct coding
                    //Y
                    {
                        int BestType = 3;
                        if (x > 0 || y > 0)
                        {
                            BlockResult[] results = new BlockResult[4];
                            results[0] = FindBestBlockType(YDec, x, y, Stride, 0);
                            results[1] = FindBestBlockType(YDec, x + 8, y, Stride, 0);
                            results[2] = FindBestBlockType(YDec, x, y + 8, Stride, 0);
                            results[3] = FindBestBlockType(YDec, x + 8, y + 8, Stride, 0);
                            float BestScore = float.MaxValue;
                            for (int i = 0; i < 4; i++)
                            {
                                if (results[i].BlockType == 0 && y == 0) continue;
                                if (results[i].BlockType == 1 && x == 0) continue;
                                if (results[i].BlockType > 3 && (y == 0 || x == 0)) continue;
                                if (results[i].Score < BestScore)
                                {
                                    BestScore = results[i].Score;
                                    BestType = results[i].BlockType;
                                }
                            }
                        }
                        b.WriteBits((uint)BestType, 3);//Block type
                        for (int y2 = 0; y2 < 16; y2 += 8)
                        {
                            for (int x2 = 0; x2 < 16; x2 += 8)
                            {
                                float[] Compvals = GetCompvals(BestType, YDec, x + x2, y + y2, Stride, 0);
                                b.WriteBits(1, 1);//Don't use 4x4 blocks
                                float[] values = GetBlockPixels(d, x + x2, y + y2, BlockComponent.Y);
                                for (int i = 0; i < 64; i++)
                                {
                                    values[i] -= Compvals[i];
                                }
                                float[] dct = GetDCT(values);
                                float[] result2 = new float[64];
                                for (int i = 0; i < 64; i++)
                                {
                                    result2[ZigZagTable8x8[i]] = (float)Math.Round(dct[i] * 8f / QuantizationTable[i]);
                                }
                                dct = result2;
                                int lastnonzero = 0;
                                for (int i = 0; i < 64; i++)
                                {
                                    if (dct[i] != 0) lastnonzero = i;
                                }
                                if (lastnonzero == 0 && dct[0] == 0)
                                {

                                }
                                int skip = 0;
                                for (int i = 0; i < 64; i++)
                                {
                                    if (dct[i] == 0 && lastnonzero != 0)
                                    {
                                        skip++;
                                        continue;
                                    }
                                    b.WriteBits(3, 7);
                                    b.WriteBits(1, 1);
                                    b.WriteBits(1, 1);
                                    if (i == lastnonzero) b.WriteBits(1, 1);
                                    else b.WriteBits(0, 1);
                                    b.WriteBits((uint)skip, 6);
                                    skip = 0;
                                    b.WriteBits((uint)(int)dct[i], 12);
                                    if (i == lastnonzero) break;
                                }

                                //Decompress Block for reference use
                                byte[] tmp = b.PeekStream();
                                byte[] tmp2 = new byte[tmp.Length + 2];
                                Array.Copy(tmp, tmp2, tmp.Length);
                                AsmData ddd = new AsmData((uint)Frame.Width, (uint)Frame.Height, AsmData.MobiclipVersion.Moflex3DS);
                                ddd.Data = tmp2;
                                ddd.Offset = 0;
                                ddd.MobiclipUnpack_0_0();
                                YDec = ddd.Y[0];
                                UVDec = ddd.UV[0];
                            }
                        }
                    }
                    //UV
                    {
                        int BestType = 3;
                        if (x > 0 || y > 0)
                        {
                            BlockResult[] results = new BlockResult[4];
                            results[0] = FindBestBlockType(UVDec, x / 2, y / 2, Stride, 0);
                            results[1] = FindBestBlockType(UVDec, x / 2, y / 2, Stride, Stride / 2);
                            float BestScore = float.MaxValue;
                            for (int i = 0; i < 2; i++)
                            {
                                if (results[i].BlockType == 0 && (y / 2) == 0) continue;
                                if (results[i].BlockType == 1 && (x / 2) == 0) continue;
                                if (results[i].BlockType > 3 && ((y / 2) == 0 || (x / 2) == 0)) continue;
                                if (results[i].Score < BestScore)
                                {
                                    BestScore = results[i].Score;
                                    BestType = results[i].BlockType;
                                }
                            }
                        }
                        b.WriteBits((uint)BestType, 3);//Use block type 3
                        float[] CompvalsU = GetCompvals(BestType, UVDec, x / 2, y / 2, Stride, 0);
                        float[] CompvalsV = GetCompvals(BestType, UVDec, x / 2, y / 2, Stride, Stride / 2);
                        //U
                        {
                            b.WriteBits(1, 1);//Don't use 4x4 blocks
                            float[] values = GetBlockPixels(d, x, y, BlockComponent.U);
                            for (int i = 0; i < 64; i++)
                            {
                                values[i] -= CompvalsU[i];
                            }
                            float[] dct = GetDCT(values);
                            float[] result2 = new float[64];
                            for (int i = 0; i < 64; i++)
                            {
                                result2[ZigZagTable8x8[i]] = (float)Math.Round(dct[i] * 8f / QuantizationTable[i]);
                            }
                            dct = result2;
                            int lastnonzero = 0;
                            for (int i = 0; i < 64; i++)
                            {
                                if (dct[i] != 0) lastnonzero = i;
                            }
                            if (lastnonzero == 0 && dct[0] == 0)
                            {

                            }
                            int skip = 0;
                            for (int i = 0; i < 64; i++)
                            {
                                if (dct[i] == 0 && lastnonzero != 0)
                                {
                                    skip++;
                                    continue;
                                }
                                b.WriteBits(3, 7);
                                b.WriteBits(1, 1);
                                b.WriteBits(1, 1);
                                if (i == lastnonzero) b.WriteBits(1, 1);
                                else b.WriteBits(0, 1);
                                b.WriteBits((uint)skip, 6);
                                skip = 0;
                                b.WriteBits((uint)(int)dct[i], 12);
                                if (i == lastnonzero) break;
                            }
                        }
                        //V
                        {
                            b.WriteBits(1, 1);//Don't use 4x4 blocks
                            float[] values = GetBlockPixels(d, x, y, BlockComponent.V);
                            for (int i = 0; i < 64; i++)
                            {
                                values[i] -= CompvalsV[i];
                            }
                            float[] dct = GetDCT(values);
                            float[] result2 = new float[64];
                            for (int i = 0; i < 64; i++)
                            {
                                result2[ZigZagTable8x8[i]] = (float)Math.Round(dct[i] * 8f / QuantizationTable[i]);
                            }
                            dct = result2;
                            int lastnonzero = 0;
                            for (int i = 0; i < 64; i++)
                            {
                                if (dct[i] != 0) lastnonzero = i;
                            }
                            if (lastnonzero == 0 && dct[0] == 0)
                            {

                            }
                            int skip = 0;
                            for (int i = 0; i < 64; i++)
                            {
                                if (dct[i] == 0 && lastnonzero != 0)
                                {
                                    skip++;
                                    continue;
                                }
                                b.WriteBits(3, 7);
                                b.WriteBits(1, 1);
                                b.WriteBits(1, 1);
                                if (i == lastnonzero) b.WriteBits(1, 1);
                                else b.WriteBits(0, 1);
                                b.WriteBits((uint)skip, 6);
                                skip = 0;
                                b.WriteBits((uint)(int)dct[i], 12);
                                if (i == lastnonzero) break;
                            }
                        }
                        //Decompress Block for reference use
                        byte[] tmp = b.PeekStream();
                        AsmData ddd = new AsmData((uint)Frame.Width, (uint)Frame.Height, AsmData.MobiclipVersion.Moflex3DS);
                        ddd.Data = tmp;
                        ddd.Offset = 0;
                        ddd.MobiclipUnpack_0_0();
                        YDec = ddd.Y[0];
                        UVDec = ddd.UV[0];
                    }
                }
            }
            Frame.UnlockBits(d);
            b.WriteBits(0, 16);
            b.Flush();
            byte[] result = b.ToArray();
            AsmData dd = new AsmData((uint)Frame.Width, (uint)Frame.Height, AsmData.MobiclipVersion.Moflex3DS);
            dd.Data = result;
            dd.Offset = 0;
            Bitmap q = dd.MobiclipUnpack_0_0();
            return result;
        }

        private enum BlockComponent
        {
            Y,
            U,
            V
        }

        private class BlockResult
        {
            public int BlockType;
            public float Score;
        }

        private static BlockResult FindBestBlockType(byte[] Data, int X, int Y, int Stride, int Offset)
        {
            if (X == 0 && Y == 0)
            {
                var b = new BlockResult() { Score = 0, BlockType = 3 };
                //b.Compvals = new float[64];
                //for (int i = 0; i < 64; i++) b.Compvals[i] = 0x80;
                return b;
            }
            float[] ThisBlock = GetBlockPixels(Data, X, Y, Stride, Offset);
            int BestType = -1;
            float BestScore = float.MaxValue;
            if (Y >= 8)//Block Type 0
            {
                float Score = GetScore(ThisBlock, GetCompvals(0, Data, X, Y, Stride, Offset));
                if (Score < BestScore)
                {
                    BestScore = Score;
                    BestType = 0;
                }
            }
            if (X >= 8)//Block Type 1
            {
                float Score = GetScore(ThisBlock, GetCompvals(1, Data, X, Y, Stride, Offset));
                if (Score < BestScore)
                {
                    BestScore = Score;
                    BestType = 1;
                }
            }
            //Block Type 3
            {
                float Score = GetScore(ThisBlock, GetCompvals(3, Data, X, Y, Stride, Offset));
                if (Score < BestScore)
                {
                    BestScore = Score;
                    BestType = 3;
                }
            }
            if (X >= 8 && Y >= 8)
            {
                float Score = GetScore(ThisBlock, GetCompvals(4, Data, X, Y, Stride, Offset));
                if (Score < BestScore)
                {
                    BestScore = Score;
                    BestType = 4;
                }
                Score = GetScore(ThisBlock, GetCompvals(5, Data, X, Y, Stride, Offset));
                if (Score < BestScore)
                {
                    BestScore = Score;
                    BestType = 5;
                }
                Score = GetScore(ThisBlock, GetCompvals(6, Data, X, Y, Stride, Offset));
                if (Score < BestScore)
                {
                    BestScore = Score;
                    BestType = 6;
                }
                Score = GetScore(ThisBlock, GetCompvals(7, Data, X, Y, Stride, Offset));
                if (Score < BestScore)
                {
                    BestScore = Score;
                    BestType = 7;
                }
                /*Score = GetScore(ThisBlock, GetCompvals(8, Data, X, Y, Stride, Offset));
                if (Score < BestScore)
                {
                    BestScore = Score;
                    BestType = 8;
                }*/
            }
            return new BlockResult() { BlockType = BestType, Score = BestScore };
        }

        private static float[] GetCompvals(int BlockType, byte[] Data, int X, int Y, int Stride, int Offset)
        {
            switch (BlockType)
            {
                case 0:
                    {
                        float[] ThatBlock = GetBlockPixels(Data, X, Y - 8, Stride, Offset);
                        float[] CompVals = new float[64];
                        for (int y = 0; y < 8; y++)
                        {
                            Array.Copy(ThatBlock, 7 * 8, CompVals, y * 8, 8);
                        }
                        return CompVals;
                    }
                case 1:
                    {
                        float[] ThatBlock = GetBlockPixels(Data, X - 8, Y, Stride, Offset);
                        float[] CompVals = new float[64];
                        for (int y = 0; y < 8; y++)
                        {
                            float val = ThatBlock[y * 8 + 7];
                            for (int x = 0; x < 8; x++)
                            {
                                CompVals[y * 8 + x] = val;
                            }
                        }
                        return CompVals;
                    }
                case 3:
                    {
                        float[] CompVals = new float[64];
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
                                    float[] ThatBlock = GetBlockPixels(Data, X, Y - 8, Stride, Offset);
                                    int sum = 0;
                                    for (int x3 = 0; x3 < 8; x3++)
                                    {
                                        sum += (int)ThatBlock[7 * 8 + x3];
                                    }
                                    sum = (sum + 4) / 8;
                                    for (int i = 0; i < 64; i++) CompVals[i] = sum;
                                    break;
                                }
                            case 2:
                                {
                                    float[] ThatBlock = GetBlockPixels(Data, X - 8, Y, Stride, Offset);
                                    int sum = 0;
                                    for (int y3 = 0; y3 < 8; y3++)
                                    {
                                        sum += (int)ThatBlock[y3 * 8 + 7];
                                    }
                                    sum = (sum + 4) / 8;
                                    for (int i = 0; i < 64; i++) CompVals[i] = sum;
                                    break;
                                }
                            case 3://00116EC0
                                {
                                    float[] ThatBlock1 = GetBlockPixels(Data, X, Y - 8, Stride, Offset);
                                    float[] ThatBlock2 = GetBlockPixels(Data, X - 8, Y, Stride, Offset);
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
                                    for (int i = 0; i < 64; i++) CompVals[i] = sum;
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
                        uint r3_i =  Data[r11_i - 1];
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
                        return GetBlockPixels(Data, X, Y, Stride, Offset);
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
                        return GetBlockPixels(Data, X, Y, Stride, Offset);
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
                        return GetBlockPixels(Data, X, Y, Stride, Offset);
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
                        return GetBlockPixels(Data, X, Y, Stride, Offset);
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
                        return GetBlockPixels(Data, X, Y, Stride, Offset);
                    }
            }
            return null;
        }

        private static unsafe float[] GetBlockPixels(BitmapData Bd, int X, int Y, BlockComponent Component)
        {
            float[] values = new float[64];
            switch (Component)
            {
                case BlockComponent.Y:
                    {
                        for (int y3 = 0; y3 < 8; y3++)
                        {
                            for (int x3 = 0; x3 < 8; x3++)
                            {
                                Color c = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3) * Bd.Stride + (X + x3) * 4))[0]);
                                values[y3 * 8 + x3] = GetYForColor(c);
                            }
                        }
                        break;
                    }
                case BlockComponent.U:
                    {
                        for (int y3 = 0; y3 < 16; y3 += 2)
                        {
                            for (int x3 = 0; x3 < 16; x3 += 2)
                            {
                                Color c = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3) * Bd.Stride + (X + x3) * 4))[0]);
                                Color c2 = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3) * Bd.Stride + (X + x3 + 1) * 4))[0]);
                                Color c3 = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3 + 1) * Bd.Stride + (X + x3) * 4))[0]);
                                Color c4 = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3 + 1) * Bd.Stride + (X + x3 + 1) * 4))[0]);
                                values[(y3 / 2) * 8 + (x3 / 2)] = GetUForColors(c, c2, c3, c4);
                            }
                        }
                        break;
                    }
                case BlockComponent.V:
                    {
                        for (int y3 = 0; y3 < 16; y3 += 2)
                        {
                            for (int x3 = 0; x3 < 16; x3 += 2)
                            {
                                Color c = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3) * Bd.Stride + (X + x3) * 4))[0]);
                                Color c2 = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3) * Bd.Stride + (X + x3 + 1) * 4))[0]);
                                Color c3 = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3 + 1) * Bd.Stride + (X + x3) * 4))[0]);
                                Color c4 = Color.FromArgb(((int*)(((byte*)Bd.Scan0) + (Y + y3 + 1) * Bd.Stride + (X + x3 + 1) * 4))[0]);
                                values[(y3 / 2) * 8 + (x3 / 2)] = GetVForColors(c, c2, c3, c4);
                            }
                        }
                        break;
                    }
            }
            return values;
        }

        private static unsafe float[] GetBlockPixels(byte[] Data, int X, int Y, int Stride, int Offset)
        {
            float[] values = new float[64];
            for (int y3 = 0; y3 < 8; y3++)
            {
                for (int x3 = 0; x3 < 8; x3++)
                {
                    values[y3 * 8 + x3] = Data[(Y + y3) * Stride + X + x3 + Offset];
                }
            }
            return values;
        }

        private static float GetScore(float[] Block, float[] Compvals)
        {
            float diff = 0;
            for (int i = 0; i < 64; i++)
            {
                diff += Math.Abs(Block[i] - Compvals[i]);
            }
            return diff;
        }

        private static float[] GetDCT(float[] Pixels)
        {
            float[] Result = new float[64];
            for (int u = 0; u < 8; u++)
            {
                for (int v = 0; v < 8; v++)
                {
                    float sum = 0;
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            sum += Pixels[y * 8 + x] * (float)Math.Cos(((2 * x + 1) * v * Math.PI) / 16f) * (float)Math.Cos(((2 * y + 1) * u * Math.PI) / 16f);
                        }
                    }
                    Result[v * 8 + u] = 0.25f * ((u == 0) ? (1f / (float)Math.Sqrt(2)) : 1f) * ((v == 0) ? (1f / (float)Math.Sqrt(2)) : 1f) * sum;
                }
            }
            return Result;
        }

        private static Color ConvertToVideoLevels(Color C)
        {
            return Color.FromArgb(
                (int)(C.R * (255f - 16f) / 255f + 16f),
                (int)(C.G * (255f - 16f) / 255f + 16f),
                (int)(C.B * (255f - 16f) / 255f + 16f));
        }

        private static float GetYForColor(Color C)
        {
            C = ConvertToVideoLevels(C);
            return C.R * 0.299f + C.G * 0.587f + C.B * 0.114f;
        }

        private static float GetUForColor(Color C)
        {
            C = ConvertToVideoLevels(C);
            return C.R * -0.169f + C.G * -0.331f + C.B * 0.5f + 128f;
        }
        private static float GetUForColors(Color A, Color B, Color C, Color D)
        {
            Color result = Color.FromArgb(
                (A.R + B.R + C.R + D.R + 2) / 4,
                (A.G + B.G + C.G + D.G + 2) / 4,
                (A.B + B.B + C.B + D.B + 2) / 4);
            return GetUForColor(result);
        }

        private static float GetVForColor(Color C)
        {
            C = ConvertToVideoLevels(C);
            return C.R * 0.5f + C.G * -0.419f + C.B * -0.081f + 128f;
        }

        private static float GetVForColors(Color A, Color B, Color C, Color D)
        {
            Color result = Color.FromArgb(
               (A.R + B.R + C.R + D.R + 2) / 4,
               (A.G + B.G + C.G + D.G + 2) / 4,
               (A.B + B.B + C.B + D.B + 2) / 4);
            return GetVForColor(result);
        }

        private static byte[] byte_118DD4 = 
        {
	        0x14, 0x13, 0x13, 0x19, 0x12, 0x19, 0x13, 0x18, 0x18, 0x13, 0x14, 0x12,
	        0x20, 0x12, 0x14, 0x13, 0x13, 0x18, 0x18, 0x13, 0x13, 0x19, 0x12, 0x19,
	        0x12, 0x19, 0x12, 0x19, 0x13, 0x18, 0x18, 0x13, 0x13, 0x18, 0x18, 0x13,
	        0x12, 0x20, 0x12, 0x14, 0x12, 0x20, 0x12, 0x18, 0x18, 0x13, 0x13, 0x18,
	        0x18, 0x12, 0x19, 0x12, 0x19, 0x12, 0x13, 0x18, 0x18, 0x13, 0x12, 0x20,
	        0x12, 0x18, 0x18, 0x12, 0x16, 0x15, 0x15, 0x1C, 0x13, 0x1C, 0x15, 0x1A,
	        0x1A, 0x15, 0x16, 0x13, 0x23, 0x13, 0x16, 0x15, 0x15, 0x1A, 0x1A, 0x15,
	        0x15, 0x1C, 0x13, 0x1C, 0x13, 0x1C, 0x13, 0x1C, 0x15, 0x1A, 0x1A, 0x15,
	        0x15, 0x1A, 0x1A, 0x15, 0x13, 0x23, 0x13, 0x16, 0x13, 0x23, 0x13, 0x1A,
	        0x1A, 0x15, 0x15, 0x1A, 0x1A, 0x13, 0x1C, 0x13, 0x1C, 0x13, 0x15, 0x1A,
	        0x1A, 0x15, 0x13, 0x23, 0x13, 0x1A, 0x1A, 0x13, 0x1A, 0x18, 0x18, 0x21,
	        0x17, 0x21, 0x18, 0x1F, 0x1F, 0x18, 0x1A, 0x17, 0x2A, 0x17, 0x1A, 0x18,
	        0x18, 0x1F, 0x1F, 0x18, 0x18, 0x21, 0x17, 0x21, 0x17, 0x21, 0x17, 0x21,
	        0x18, 0x1F, 0x1F, 0x18, 0x18, 0x1F, 0x1F, 0x18, 0x17, 0x2A, 0x17, 0x1A,
	        0x17, 0x2A, 0x17, 0x1F, 0x1F, 0x18, 0x18, 0x1F, 0x1F, 0x17, 0x21, 0x17,
	        0x21, 0x17, 0x18, 0x1F, 0x1F, 0x18, 0x17, 0x2A, 0x17, 0x1F, 0x1F, 0x17,
	        0x1C, 0x1A, 0x1A, 0x23, 0x19, 0x23, 0x1A, 0x21, 0x21, 0x1A, 0x1C, 0x19,
	        0x2D, 0x19, 0x1C, 0x1A, 0x1A, 0x21, 0x21, 0x1A, 0x1A, 0x23, 0x19, 0x23,
	        0x19, 0x23, 0x19, 0x23, 0x1A, 0x21, 0x21, 0x1A, 0x1A, 0x21, 0x21, 0x1A,
	        0x19, 0x2D, 0x19, 0x1C, 0x19, 0x2D, 0x19, 0x21, 0x21, 0x1A, 0x1A, 0x21,
	        0x21, 0x19, 0x23, 0x19, 0x23, 0x19, 0x1A, 0x21, 0x21, 0x1A, 0x19, 0x2D,
	        0x19, 0x21, 0x21, 0x19, 0x20, 0x1E, 0x1E, 0x28, 0x1C, 0x28, 0x1E, 0x26,
	        0x26, 0x1E, 0x20, 0x1C, 0x33, 0x1C, 0x20, 0x1E, 0x1E, 0x26, 0x26, 0x1E,
	        0x1E, 0x28, 0x1C, 0x28, 0x1C, 0x28, 0x1C, 0x28, 0x1E, 0x26, 0x26, 0x1E,
	        0x1E, 0x26, 0x26, 0x1E, 0x1C, 0x33, 0x1C, 0x20, 0x1C, 0x33, 0x1C, 0x26,
	        0x26, 0x1E, 0x1E, 0x26, 0x26, 0x1C, 0x28, 0x1C, 0x28, 0x1C, 0x1E, 0x26,
	        0x26, 0x1E, 0x1C, 0x33, 0x1C, 0x26, 0x26, 0x1C, 0x24, 0x22, 0x22, 0x2E,
	        0x20, 0x2E, 0x22, 0x2B, 0x2B, 0x22, 0x24, 0x20, 0x3A, 0x20, 0x24, 0x22,
	        0x22, 0x2B, 0x2B, 0x22, 0x22, 0x2E, 0x20, 0x2E, 0x20, 0x2E, 0x20, 0x2E,
	        0x22, 0x2B, 0x2B, 0x22, 0x22, 0x2B, 0x2B, 0x22, 0x20, 0x3A, 0x20, 0x24,
	        0x20, 0x3A, 0x20, 0x2B, 0x2B, 0x22, 0x22, 0x2B, 0x2B, 0x20, 0x2E, 0x20,
	        0x2E, 0x20, 0x22, 0x2B, 0x2B, 0x22, 0x20, 0x3A, 0x20, 0x2B, 0x2B, 0x20
        };

        private static byte[] byte_119004 =
        {
	        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
	        0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03,
	        0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05,
	        0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
	        0x08, 0x08, 0x08, 0x08, 0x08, 0x08
        };

        private static byte[] byte_11903A = 
        {
	        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
	        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
	        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
	        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
	        0x00, 0x01, 0x02, 0x03, 0x04, 0x05
        };

        private static float[] GetQuantizationTable(ref int Quantizer)
        {
            if (Quantizer < 0xC) Quantizer = 0xC;
            if (Quantizer > 0x34) Quantizer = 0x34;
            float[] Table = new float[64];
            int r6 = byte_119004[Quantizer] + 8;
            int r5 = byte_11903A[Quantizer];
            r6 -= 2;
            int r4 = r5 << 6;
            for (int i = 0; i < 64; i++)
            {
                Table[i] = (byte_118DD4[r4++] << r6) >> 8;
            }
            //Dezigzag
            float[] result2 = new float[64];
            for (int i = 0; i < 64; i++)
            {
                result2[DeZigZagTable8x8[i]] = Table[i];
            }
            return result2;
        }
    }
}
