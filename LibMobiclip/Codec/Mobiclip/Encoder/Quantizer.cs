using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMobiclip.Codec.Mobiclip.Encoder
{
    public class Quantizer
    {
        private static float[,] adaptive_quant_table =
        {
            { 1f/2f, 3f/7f, 2f/5f, 1f/3f },
            { 3f/7f, 3f/7f, 1f/3f, 1f/4f },
            { 2f/5f, 1f/3f, 1f/5f, 1f/5f },
            { 1f/3f, 1f/4f, 1f/5f, 1f/5f }
        };

        public static bool Quantize64(MobiEncoder Context, int[] DCT, int[] ZigQuantizedResult, int[] ReconstructedResult)
        {
            int[] nonzigzagquantized = new int[64];
            for (int i = 0; i < 64; i++)
            {
                int f = 87381;//(Context.QTable8x8[i] << 7);// + (1 << 16);//* 96 /*<< 6*/);// (int)(adaptive_quant_table[(i / 8) / 2, (i % 8) / 2] * (((1 << 17) + (1 << 18)) / 2));//(int)(1f / ((MobiConst.ZigZagTable8x8[i] >> 3) + 1f) * (1 << 17));// ((7 * (1 << 16) + (1 << 17)) / 8)/*(1 << 18)*/);
                if (DCT[i] < 0)
                    nonzigzagquantized[i] = -((-DCT[i] * Context.QTable8x8[i] + /*49152*//*87381*/f) >> 18);
                else
                    nonzigzagquantized[i] = (DCT[i] * Context.QTable8x8[i] + /*49152*//*87381*/f) >> 18;
            }

            int nrzeros = 0;
            for (int i = 0; i < 64; i++)
            {
                if (nonzigzagquantized[i] == 0)
                    nrzeros++;
                else
                    nrzeros = 0;
            }

            for (int i = 0; i < 64; i++)
            {
                ZigQuantizedResult[MobiConst.ZigZagTable8x8[i]] = nonzigzagquantized[i];
                ReconstructedResult[i] = nonzigzagquantized[i] * Context.DeQTable8x8[i];
            }

            return nrzeros == 64;
        }

        public static bool Quantize16(MobiEncoder Context, int[] DCT, int[] ZigQuantizedResult, int[] ReconstructedResult)
        {
            int[] nonzigzagquantized = new int[16];
            for (int i = 0; i < 16; i++)
            {
                int f = 10922;// (Context.QTable4x4[i] << 4);// + (1 << 13);//* 12);//<< 4);
                if (DCT[i] < 0)
                    nonzigzagquantized[i] = -((-DCT[i] * Context.QTable4x4[i] + f) >> 15);
                else
                    nonzigzagquantized[i] = (DCT[i] * Context.QTable4x4[i] + f) >> 15;
            }

            int nrzeros = 0;
            for (int i = 0; i < 16; i++)
            {
                if (nonzigzagquantized[i] == 0)
                    nrzeros++;
                else
                    nrzeros = 0;
            }

            for (int i = 0; i < 16; i++)
            {
                ZigQuantizedResult[MobiConst.ZigZagTable4x4[i]] = nonzigzagquantized[i];
                ReconstructedResult[i] = nonzigzagquantized[i] * (int)Context.DeQTable4x4[i];
            }

            return nrzeros == 16;
        }
    }
}
