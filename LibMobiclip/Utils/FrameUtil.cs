using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMobiclip.Utils
{
    public class FrameUtil
    {
        public static unsafe byte[] GetBlockPixels16x16(byte[] Data, int X, int Y, int Stride, int Offset)
        {
            byte[] values = new byte[256];
            fixed (byte* pVals = &values[0])
            {
                ulong* pLVals = (ulong*)pVals;
                for (int y3 = 0; y3 < 16; y3++)
                {
                    fixed (byte* pData = &Data[(Y + y3) * Stride + X + Offset])
                    {
                        *pLVals++ = *((ulong*)pData);
                        *pLVals++ = *((ulong*)(pData + 8));
                    }
                }
            }
            return values;
        }

        public static unsafe byte[] GetBlockPixels8x8(byte[] Data, int X, int Y, int Stride, int Offset)
        {
            byte[] values = new byte[64];
            fixed (byte* pVals = &values[0], pData = &Data[Y * Stride + X + Offset])
            {
                ulong* pLVals = (ulong*)pVals;
                *pLVals++ = *((ulong*)pData);
                *pLVals++ = *((ulong*)(pData + Stride));
                *pLVals++ = *((ulong*)(pData + Stride * 2));
                *pLVals++ = *((ulong*)(pData + Stride * 3));
                *pLVals++ = *((ulong*)(pData + Stride * 4));
                *pLVals++ = *((ulong*)(pData + Stride * 5));
                *pLVals++ = *((ulong*)(pData + Stride * 6));
                *pLVals++ = *((ulong*)(pData + Stride * 7));
                /*ulong* pLVals = (ulong*)pVals;
                for (int y3 = 0; y3 < 8; y3++)
                {
                    fixed (byte* pData = &Data[(Y + y3) * Stride + X + Offset])
                    {
                        *pLVals++ = *((ulong*)pData);
                    }
                }*/
            }
            return values;
        }

        public static unsafe byte[] GetBlockPixels4x4(byte[] Data, int X, int Y, int Stride, int Offset)
        {
            byte[] values = new byte[16];
            fixed (byte* pVals = &values[0], pData = &Data[Y * Stride + X + Offset])
            {
                uint* pLVals = (uint*)pVals;
                *pLVals++ = *((uint*)pData);
                *pLVals++ = *((uint*)(pData + Stride));
                *pLVals++ = *((uint*)(pData + Stride * 2));
                *pLVals++ = *((uint*)(pData + Stride * 3));
            }
            return values;
        }

        public static unsafe void SetBlockPixels4x4(byte[] Data, int X, int Y, int Stride, int Offset, byte[] Values)
        {
            fixed (byte* pVals = &Values[0], pData = &Data[Y * Stride + X + Offset])
            {
                uint* pLVals = (uint*)pVals;
                *((uint*)pData) = *pLVals++;
                *((uint*)(pData + Stride)) = *pLVals++;
                *((uint*)(pData + Stride * 2)) = *pLVals++;
                *((uint*)(pData + Stride * 3)) = *pLVals++;
            }
        }

        public static unsafe void SetBlockPixels8x8(byte[] Data, int X, int Y, int Stride, int Offset, byte[] Values)
        {
            fixed (byte* pVals = &Values[0], pData = &Data[Y * Stride + X + Offset])
            {
                ulong* pLVals = (ulong*)pVals;
                *((ulong*)pData) = *pLVals++;
                *((ulong*)(pData + Stride)) = *pLVals++;
                *((ulong*)(pData + Stride * 2)) = *pLVals++;
                *((ulong*)(pData + Stride * 3)) = *pLVals++;
                *((ulong*)(pData + Stride * 4)) = *pLVals++;
                *((ulong*)(pData + Stride * 5)) = *pLVals++;
                *((ulong*)(pData + Stride * 6)) = *pLVals++;
                *((ulong*)(pData + Stride * 7)) = *pLVals++;
            }
        }

        public static unsafe byte[] GetPBlock(byte[] Src, int Dx, int Dy, uint Width, uint Height, int Offset, int Stride)
        {
            byte[] result = new byte[Width * Height];
            fixed (byte* pfResult = &result[0], pfSrc = &Src[0])
            {
                byte* pResult = pfResult;
                byte* pSrc = pfSrc + Offset + (Dx >> 1) + (Dy >> 1) * Stride;
                for (int i = 0; i < Height; i++)
                {
                    //int pos = Offset + (((Dy >> 1) + i) * Stride) + (Dx >> 1);
                    switch ((Dx & 1) | ((Dy & 1) << 1))
                    {
                        case 0:
                            for (int j = 0; j < Width; j++) /*result[i * Width + j]*/*pResult++ = *pSrc++;//Src[pos + j];
                            break;
                        case 1:
                            {
                                for (int j = 0; j < Width; j++)
                                {
                                    /*result[i * Width + j]*/
                                    *pResult++ = (byte)((/*Src[pos + j]*/*pSrc++ >> 1) + (/*Src[pos + j + 1]*/*pSrc >> 1));
                                }
                                break;
                            }
                        case 2:
                            {
                                for (int j = 0; j < Width; j++)
                                {
                                    /*result[i * Width + j]*/
                                    *pResult++ = (byte)((/*Src[pos + j]*/*pSrc++ >> 1) + (/*Src[pos + j + Stride]*/pSrc[Stride - 1] >> 1));
                                }
                                break;
                            }
                        case 3:
                            {
                                for (int j = 0; j < Width; j++)
                                {
                                    /*result[i * Width + j]*/
                                    *pResult++ = (byte)((((/*Src[pos + j]*/*pSrc++ >> 1) + (/*Src[pos + j + 1]*/*pSrc >> 1)) >> 1) + (((/*Src[pos + j + Stride]*/pSrc[Stride - 1] >> 1) + (/*Src[pos + j + 1 + Stride]*/pSrc[Stride] >> 1)) >> 1));
                                }
                                break;
                            }
                    }
                    pSrc += Stride - Width;
                }
            }
            return result;
        }
    }
}
