using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobiclipDecoder.Mobi
{
    public class MobiOld
    {
        //If I need them later on

        //114EA4
        private void CopyBlock1(int Idx, byte[] Src, int SrcOffset, byte[] Dst, int DstOffset, uint r2)
        {
            switch (Idx)
            {
                case 0:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset];
                            SrcOffset += Stride;
                            Dst[DstOffset] = (byte)r3;
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 1:
                    {
                        SrcOffset++;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset - 1];
                            uint r9 = Src[SrcOffset];
                            SrcOffset += Stride;
                            r3 >>= 1;
                            r3 += r9 >> 1;
                            Dst[DstOffset] = (byte)r3;
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 2:
                    {
                        uint r3 = Src[SrcOffset];
                        SrcOffset += Stride;
                        r3 >>= 1;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset];
                            SrcOffset += Stride;
                            r7 >>= 1;
                            r3 += r7;
                            Dst[DstOffset] = (byte)r3;
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset];
                            SrcOffset += Stride;
                            r3 >>= 1;
                            r7 += r3;
                            Dst[DstOffset] = (byte)r7;
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 3:
                    {
                        uint r3 = Src[SrcOffset++];
                        uint r9 = Src[SrcOffset];
                        SrcOffset += Stride;
                        r3 >>= 1;
                        r3 += r9 >> 1;
                        r3 >>= 1;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset - 1];
                            r9 = Src[SrcOffset];
                            SrcOffset += Stride;
                            r7 >>= 1;
                            r7 += r9 >> 1;
                            r7 >>= 1;
                            r3 += r7;
                            Dst[DstOffset] = (byte)r3;
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset - 1];
                            r9 = Src[SrcOffset];
                            SrcOffset += Stride;
                            r3 >>= 1;
                            r3 += r9 >> 1;
                            r3 >>= 1;
                            r7 += r3;
                            Dst[DstOffset] = (byte)r7;
                            DstOffset += Stride;
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        //Switch_114C6C
        private void CopyBlock2(int Idx, byte[] Src, int SrcOffset, byte[] Dst, int DstOffset, uint r2)
        {
            switch (Idx)
            {
                case 0:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 1:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                            uint r9 = Src[SrcOffset + 2];
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r3 >> 8;
                            r3 += r9 << 8;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 2:
                    {
                        SrcOffset++;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset - 1];
                            uint r9 = Src[SrcOffset];
                            SrcOffset += Stride;
                            r3 += r9 << 8;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 3:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset];
                            uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 1);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            uint r10 = r9 << 24;
                            r10 = r3 + (r10 >> 16);
                            r3 = r10 + r9;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 4:
                    {
                        uint r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU16LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 5:
                    {
                        uint r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                        uint r9 = Src[SrcOffset + 2];
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        r3 += r3 >> 8;
                        r3 += r9 << 8;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU16LE(Src, SrcOffset);
                            r9 = Src[SrcOffset + 2];
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r7 += r7 >> 8;
                            r7 += r9 << 8;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                            r9 = Src[SrcOffset + 2];
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r3 >> 8;
                            r3 += r9 << 8;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 6:
                    {
                        SrcOffset++;
                        uint r3 = Src[SrcOffset - 1];
                        uint r9 = Src[SrcOffset];
                        SrcOffset += Stride;
                        r3 += r9 << 8;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset - 1];
                            r9 = Src[SrcOffset];
                            SrcOffset += Stride;
                            r7 += r9 << 8;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset - 1];
                            r9 = Src[SrcOffset];
                            SrcOffset += Stride;
                            r3 += r9 << 8;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 7:
                    {
                        uint r3 = Src[SrcOffset];
                        uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 1);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        uint r10 = r9 << 24;
                        r10 = r3 + (r10 >> 16);
                        r3 = r10 + r9;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset];
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 1);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r9 << 24;
                            r10 = r7 + (r10 >> 16);
                            r7 = r10 + r9;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset];
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 1);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r9 << 24;
                            r10 = r3 + (r10 >> 16);
                            r3 = r10 + r9;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU16LE(Dst, DstOffset, (ushort)r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        //Switch_114A04
        private void CopyBlock4(int Idx, byte[] Src, int SrcOffset, byte[] Dst, int DstOffset, uint r2)
        {
            switch (Idx)
            {
                case 0:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 1:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = Src[SrcOffset + 4];
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r3 >> 8;
                            r3 += r9 << 24;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 2:
                    {
                        SrcOffset--;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = Src[SrcOffset + 4];
                            SrcOffset += Stride;
                            r3 >>= 8;
                            r3 += r9 << 24;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 3:
                    {
                        SrcOffset--;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            uint r10 = r3 >> 16;
                            r10 += r9 << 16;
                            r10 += r3 >> 8;
                            r3 = r10 + (r9 << 24);
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 4:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 2);
                            SrcOffset += Stride;
                            r3 += r9 << 16;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 5:
                    {
                        SrcOffset += 2;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            uint r10 = r3 + (r9 << 16);
                            r10 += r3 >> 8;
                            r3 = r10 + (r9 << 8);
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 6:
                    {
                        SrcOffset++;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset - 1];
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r3 += r9 << 8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 7:
                    {
                        SrcOffset++;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset - 1];
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r9 << 8;
                            r3 += r9;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 8:
                    {
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 9:
                    {
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r9 = Src[SrcOffset + 4];
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        r3 += r3 >> 8;
                        r3 += r9 << 24;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = Src[SrcOffset + 4];
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r7 += r7 >> 8;
                            r7 += r9 << 24;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = Src[SrcOffset + 4];
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r3 >> 8;
                            r3 += r9 << 24;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 10:
                    {
                        SrcOffset--;
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r9 = Src[SrcOffset + 4];
                        SrcOffset += Stride;
                        r3 >>= 8;
                        r3 += r9 << 24;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = Src[SrcOffset + 4];
                            SrcOffset += Stride;
                            r7 >>= 8;
                            r7 += r9 << 24;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = Src[SrcOffset + 4];
                            SrcOffset += Stride;
                            r3 >>= 8;
                            r3 += r9 << 24;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 11:
                    {
                        SrcOffset--;
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 4);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        uint r10 = r3 >> 16;
                        r10 += r9 << 16;
                        r10 += r3 >> 8;
                        r3 = r10 + (r9 << 24);
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r7 >> 16;
                            r10 += r9 << 16;
                            r10 += r7 >> 8;
                            r7 = r10 + (r9 << 24);
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r3 >> 16;
                            r10 += r9 << 16;
                            r10 += r3 >> 8;
                            r3 = r10 + (r9 << 24);
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 12:
                    {
                        uint r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                        uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 2);
                        SrcOffset += Stride;
                        r3 += r9 << 16;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU16LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 2);
                            SrcOffset += Stride;
                            r7 += r9 << 16;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 2);
                            SrcOffset += Stride;
                            r3 += r9 << 16;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 13:
                    {
                        SrcOffset += 2;
                        uint r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                        uint r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        uint r10 = r3 + (r9 << 16);
                        r10 += r3 >> 8;
                        r3 = r10 + (r9 << 8);
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r7 + (r9 << 16);
                            r10 += r7 >> 8;
                            r7 = r10 + (r9 << 8);
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r3 + (r9 << 16);
                            r10 += r3 >> 8;
                            r3 = r10 + (r9 << 8);
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 14:
                    {
                        SrcOffset++;
                        uint r3 = Src[SrcOffset - 1];
                        uint r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                        SrcOffset += Stride;
                        r3 += r9 << 8;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset - 1];
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r7 += r9 << 8;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset - 1];
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r3 += r9 << 8;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 15:
                    {
                        SrcOffset++;
                        uint r3 = Src[SrcOffset - 1];
                        uint r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        r3 += r9 << 8;
                        r3 += r9;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset - 1];
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r7 += r9 << 8;
                            r7 += r9;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset - 1];
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r9 << 8;
                            r3 += r9;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            DstOffset += Stride;
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        //Switch_114750
        private void CopyBlock8(int Idx, byte[] Src, int SrcOffset, byte[] Dst, int DstOffset, uint r2)
        {
            switch (Idx)
            {
                case 0:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 1:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r9 = Src[SrcOffset + 8];
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r3 >> 8;
                            r3 += r4 << 24;
                            r4 += r4 >> 8;
                            r4 += r9 << 24;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 2:
                    {
                        SrcOffset--;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r9 = Src[SrcOffset + 8];
                            SrcOffset += Stride;
                            r3 >>= 8;
                            r3 += r4 << 24;
                            r4 >>= 8;
                            r4 += r9 << 24;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 3:
                    {
                        SrcOffset--;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 8);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            uint r10 = r3 >> 16;
                            r10 += r4 << 16;
                            r10 += r3 >> 8;
                            r3 = r10 + (r4 << 24);
                            r10 = r4 >> 16;
                            r10 += r9 << 16;
                            r10 += r4 >> 8;
                            r4 = r10 + (r9 << 24);
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 4:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 2);
                            uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 6);
                            SrcOffset += Stride;
                            r3 += r4 << 16;
                            r4 >>= 16;
                            r4 += r9 << 16;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 5:
                    {
                        SrcOffset += 2;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            uint r10 = r3 + (r4 << 16);
                            r10 += r3 >> 8;
                            r3 = r10 + (r4 << 8);
                            r4 >>= 16;
                            r10 = r4 + (r9 << 16);
                            r10 += r4 >> 8;
                            r4 = r10 + (r9 << 8);
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 6:
                    {
                        SrcOffset++;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset - 1];
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 += r4 << 8;
                            r4 >>= 24;
                            r4 += r9 << 8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 7:
                    {
                        SrcOffset++;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset - 1];
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r4 << 8;
                            r3 += r4;
                            r4 = r9 + (r4 >> 24);
                            r4 += r9 << 8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 8:
                    {
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 9:
                    {
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r9 = Src[SrcOffset + 8];
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        r3 += r3 >> 8;
                        r3 += r4 << 24;
                        r4 += r4 >> 8;
                        r4 += r9 << 24;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r9 = Src[SrcOffset + 8];
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r7 += r7 >> 8;
                            r7 += r8 << 24;
                            r8 += r8 >> 8;
                            r8 += r9 << 24;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r9 = Src[SrcOffset + 8];
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r3 >> 8;
                            r3 += r4 << 24;
                            r4 += r4 >> 8;
                            r4 += r9 << 24;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 10:
                    {
                        SrcOffset--;
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r9 = Src[SrcOffset + 8];
                        SrcOffset += Stride;
                        r3 >>= 8;
                        r3 += r4 << 24;
                        r4 >>= 8;
                        r4 += r9 << 24;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r9 = Src[SrcOffset + 8];
                            SrcOffset += Stride;
                            r7 >>= 8;
                            r7 += r8 << 24;
                            r8 >>= 8;
                            r8 += r9 << 24;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r9 = Src[SrcOffset + 8];
                            SrcOffset += Stride;
                            r3 >>= 8;
                            r3 += r4 << 24;
                            r4 >>= 8;
                            r4 += r9 << 24;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 11:
                    {
                        SrcOffset--;
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 8);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        uint r10 = r3 >> 16;
                        r10 += r4 << 16;
                        r10 += r3 >> 8;
                        r3 = r10 + (r4 << 24);
                        r10 = r4 >> 16;
                        r10 += r9 << 16;
                        r10 += r4 >> 8;
                        r4 = r10 + (r9 << 24);
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 8);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r7 >> 16;
                            r10 += r8 << 16;
                            r10 += r7 >> 8;
                            r7 = r10 + (r8 << 24);
                            r10 = r8 >> 16;
                            r10 += r9 << 16;
                            r10 += r8 >> 8;
                            r8 = r10 + (r9 << 24);
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 8);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r3 >> 16;
                            r10 += r4 << 16;
                            r10 += r3 >> 8;
                            r3 = r10 + (r4 << 24);
                            r10 = r4 >> 16;
                            r10 += r9 << 16;
                            r10 += r4 >> 8;
                            r4 = r10 + (r9 << 24);
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 12:
                    {
                        uint r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 2);
                        uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 6);
                        SrcOffset += Stride;
                        r3 += r4 << 16;
                        r4 >>= 16;
                        r4 += r9 << 16;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU16LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 2);
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 6);
                            SrcOffset += Stride;
                            r7 += r8 << 16;
                            r8 >>= 16;
                            r8 += r9 << 16;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU16LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 2);
                            r9 = IOUtil.ReadU16LE(Src, SrcOffset + 6);
                            SrcOffset += Stride;
                            r3 += r4 << 16;
                            r4 >>= 16;
                            r4 += r9 << 16;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 13:
                    {
                        SrcOffset += 2;
                        uint r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        uint r10 = r3 + (r4 << 16);
                        r10 += r3 >> 8;
                        r3 = r10 + (r4 << 8);
                        r4 >>= 16;
                        r10 = r4 + (r9 << 16);
                        r10 += r4 >> 8;
                        r4 = r10 + (r9 << 8);
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r7 + (r8 << 16);
                            r10 += r7 >> 8;
                            r7 = r10 + (r8 << 8);
                            r8 >>= 16;
                            r10 = r8 + (r9 << 16);
                            r10 += r8 >> 8;
                            r8 = r10 + (r9 << 8);
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = r3 + (r4 << 16);
                            r10 += r3 >> 8;
                            r3 = r10 + (r4 << 8);
                            r4 >>= 16;
                            r10 = r4 + (r9 << 16);
                            r10 += r4 >> 8;
                            r4 = r10 + (r9 << 8);
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 14:
                    {
                        SrcOffset++;
                        uint r3 = Src[SrcOffset - 1];
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        SrcOffset += Stride;
                        r3 += r4 << 8;
                        r4 >>= 24;
                        r4 += r9 << 8;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset - 1];
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r7 += r8 << 8;
                            r8 >>= 24;
                            r8 += r9 << 8;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset - 1];
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 += r4 << 8;
                            r4 >>= 24;
                            r4 += r9 << 8;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 15:
                    {
                        SrcOffset++;
                        uint r3 = Src[SrcOffset - 1];
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r9 = (r9 >> 1) & 0x7F7F7F7F;
                        r3 += r4 << 8;
                        r3 += r4;
                        r4 = r9 + (r4 >> 24);
                        r4 += r9 << 8;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset - 1];
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r7 += r8 << 8;
                            r7 += r8;
                            r8 = r9 + (r8 >> 24);
                            r8 += r9 << 8;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset - 1];
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r4 << 8;
                            r3 += r4;
                            r4 = r9 + (r4 >> 24);
                            r4 += r9 << 8;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        //Switch_114710
        private void CopyBlock16(int Idx, byte[] Src, int SrcOffset, byte[] Dst, int DstOffset, uint r2)
        {
            switch (Idx)
            {
                case 0:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 1:
                    {
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            uint r9 = Src[SrcOffset + 0x10];
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r3 >> 8;
                            r3 += r4 << 24;
                            r4 += r4 >> 8;
                            r4 += r7 << 24;
                            r7 += r7 >> 8;
                            r7 += r8 << 24;
                            r8 += r8 >> 8;
                            r8 += r9 << 24;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 12, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 2:
                    {
                        SrcOffset--;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            uint r9 = Src[SrcOffset + 0x10];
                            SrcOffset += Stride;
                            r3 >>= 8;
                            r3 += r4 << 24;
                            r4 >>= 8;
                            r4 += r7 << 24;
                            r7 >>= 8;
                            r7 += r8 << 24;
                            r8 >>= 8;
                            r8 += r9 << 24;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 12, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 3:
                    {
                        SrcOffset--;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 0x10);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            uint r10 = r3 >> 16;
                            r10 += r4 << 16;
                            r10 += r3 >> 8;
                            r3 = r10 + (r4 << 24);
                            r10 = r4 >> 16;
                            r10 += r7 << 16;
                            r10 += r4 >> 8;
                            r4 = r10 + (r7 << 24);
                            r10 = r7 >> 16;
                            r10 += r8 << 16;
                            r10 += r7 >> 8;
                            r7 = r10 + (r8 << 24);
                            r10 = r8 >> 16;
                            r10 += r9 << 16;
                            r10 += r8 >> 8;
                            r8 = r10 + (r9 << 24);
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 12, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 4:
                    {
                        SrcOffset += 2;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r9 = IOUtil.ReadU16LE(Src, SrcOffset + 12);
                            SrcOffset += Stride;
                            r3 += r4 << 16;
                            r4 >>= 16;
                            r4 += r7 << 16;
                            r7 >>= 16;
                            r7 += r8 << 16;
                            r8 >>= 16;
                            r8 += r9 << 16;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 12, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 5:
                    {
                        SrcOffset += 2;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 12);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            uint r10 = r3 + (r4 << 16);
                            r10 += r3 >> 8;
                            r3 = r10 + (r4 << 8);
                            r4 >>= 16;
                            r10 = r4 + (r7 << 16);
                            r10 += r4 >> 8;
                            r4 = r10 + (r7 << 8);
                            r7 >>= 16;
                            r10 = r7 + (r8 << 16);
                            r10 += r7 >> 8;
                            r7 = r10 + (r8 << 8);
                            r8 >>= 16;
                            r10 = r8 + (r9 << 16);
                            r10 += r8 >> 8;
                            r8 = r10 + (r9 << 8);
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 12, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 6:
                    {
                        SrcOffset++;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset - 1];
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 12);
                            SrcOffset += Stride;
                            r3 += r4 << 8;
                            r4 >>= 24;
                            r4 += r7 << 8;
                            r7 >>= 24;
                            r7 += r8 << 8;
                            r8 >>= 24;
                            r8 += r9 << 8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 12, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 7:
                    {
                        SrcOffset++;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r3 = Src[SrcOffset - 1];
                            uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 12);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r3 += r4 << 8;
                            r3 += r4;
                            r4 = r7 + (r4 >> 24);
                            r4 += r7 << 8;
                            r7 = r8 + (r7 >> 24);
                            r7 += r8 << 8;
                            r8 = r9 + (r8 >> 24);
                            r8 += r9 << 8;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 12, r8);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 8:
                    {
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r5 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                        uint r6 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r10 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            r5 += r9;
                            r6 += r10;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r5);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r6);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r5 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            r6 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            r9 += r5;
                            r10 += r6;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r9);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r10);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 9:
                    {
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r5 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                        uint r6 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                        uint lr = Src[SrcOffset + 0x10];
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        lr = (lr >> 1) & 0x7F7F7F7F;
                        r3 += r3 >> 8;
                        r3 += r4 << 24;
                        r4 += r4 >> 8;
                        r4 += r5 << 24;
                        r5 += r5 >> 8;
                        r5 += r6 << 24;
                        r6 += r6 >> 8;
                        r6 += lr << 24;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r10 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            lr = Src[SrcOffset + 0x10];
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            lr = (lr >> 1) & 0x7F7F7F7F;
                            r7 += r7 >> 8;
                            r7 += r8 << 24;
                            r8 += r8 >> 8;
                            r8 += r9 << 24;
                            r9 += r9 >> 8;
                            r9 += r10 << 24;
                            r10 += r10 >> 8;
                            r10 += lr << 24;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            r5 += r9;
                            r6 += r10;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r5);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r6);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r5 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            r6 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            lr = Src[SrcOffset + 0x10];
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            lr = (lr >> 1) & 0x7F7F7F7F;
                            r3 += r3 >> 8;
                            r3 += r4 << 24;
                            r4 += r4 >> 8;
                            r4 += r5 << 24;
                            r5 += r5 >> 8;
                            r5 += r6 << 24;
                            r6 += r6 >> 8;
                            r6 += lr << 24;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            r9 += r5;
                            r10 += r6;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r9);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r10);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 10:
                    {
                        SrcOffset--;
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r5 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                        uint r6 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                        uint lr = Src[SrcOffset + 0x10];
                        SrcOffset += Stride;
                        r3 >>= 8;
                        r3 += r4 << 24;
                        r4 >>= 8;
                        r4 += r5 << 24;
                        r5 >>= 8;
                        r5 += r6 << 24;
                        r6 >>= 8;
                        r6 += lr << 24;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r10 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            lr = Src[SrcOffset + 0x10];
                            SrcOffset += Stride;
                            r7 >>= 8;
                            r7 += r8 << 24;
                            r8 >>= 8;
                            r8 += r9 << 24;
                            r9 >>= 8;
                            r9 += r10 << 24;
                            r10 >>= 8;
                            r10 += lr << 24;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            r5 += r9;
                            r6 += r10;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r5);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r6);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r5 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            r6 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            lr = Src[SrcOffset + 0x10];
                            SrcOffset += Stride;
                            r3 >>= 8;
                            r3 += r4 << 24;
                            r4 >>= 8;
                            r4 += r5 << 24;
                            r5 >>= 8;
                            r5 += r6 << 24;
                            r6 >>= 8;
                            r6 += lr << 24;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            r9 += r5;
                            r10 += r6;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r9);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r10);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 11:
                    {
                        SrcOffset--;
                        uint r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r5 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                        uint r6 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                        uint lr = IOUtil.ReadU16LE(Src, SrcOffset + 0x10);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        lr = (lr >> 1) & 0x7F7F7F7F;
                        uint r11 = r3 >> 16;
                        r11 += r4 << 16;
                        r11 += r3 >> 8;
                        r3 = r11 + (r4 << 24);
                        r11 = r4 >> 16;
                        r11 += r5 << 16;
                        r11 += r4 >> 8;
                        r4 = r11 + (r5 << 24);
                        r11 = r5 >> 16;
                        r11 += r6 << 16;
                        r11 += r5 >> 8;
                        r5 = r11 + (r6 << 24);
                        r11 = r6 >> 16;
                        r11 += lr << 16;
                        r11 += r6 >> 8;
                        r6 = r11 + (lr << 24);
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            uint r10 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            lr = IOUtil.ReadU16LE(Src, SrcOffset + 0x10);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            lr = (lr >> 1) & 0x7F7F7F7F;
                            r11 = r7 >> 16;
                            r11 += r8 << 16;
                            r11 += r7 >> 8;
                            r7 = r11 + (r8 << 24);
                            r11 = r8 >> 16;
                            r11 += r9 << 16;
                            r11 += r8 >> 8;
                            r8 = r11 + (r9 << 24);
                            r11 = r9 >> 16;
                            r11 += r10 << 16;
                            r11 += r9 >> 8;
                            r9 = r11 + (r10 << 24);
                            r11 = r10 >> 16;
                            r11 += lr << 16;
                            r11 += r10 >> 8;
                            r10 = r11 + (lr << 24);
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            r5 += r9;
                            r6 += r10;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r5);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r6);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r5 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            r6 = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            lr = IOUtil.ReadU16LE(Src, SrcOffset + 0x10);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            lr = (lr >> 1) & 0x7F7F7F7F;
                            r11 = r3 >> 16;
                            r11 += r4 << 16;
                            r11 += r3 >> 8;
                            r3 = r11 + (r4 << 24);
                            r11 = r4 >> 16;
                            r11 += r5 << 16;
                            r11 += r4 >> 8;
                            r4 = r11 + (r5 << 24);
                            r11 = r5 >> 16;
                            r11 += r6 << 16;
                            r11 += r5 >> 8;
                            r5 = r11 + (r6 << 24);
                            r11 = r6 >> 16;
                            r11 += lr << 16;
                            r11 += r6 >> 8;
                            r6 = r11 + (lr << 24);
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            r9 += r5;
                            r10 += r6;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r9);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r10);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 12:
                    {
                        SrcOffset += 2;
                        uint r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r5 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r6 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                        uint lr = IOUtil.ReadU16LE(Src, SrcOffset + 0xC);
                        SrcOffset += Stride;
                        r3 += r4 << 16;
                        r4 >>= 16;
                        r4 += r5 << 16;
                        r5 >>= 16;
                        r5 += r6 << 16;
                        r6 >>= 16;
                        r6 += lr << 16;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r10 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            lr = IOUtil.ReadU16LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r7 += r8 << 16;
                            r8 >>= 16;
                            r8 += r9 << 16;
                            r9 >>= 16;
                            r9 += r10 << 16;
                            r10 >>= 16;
                            r10 += lr << 16;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            r5 += r9;
                            r6 += r10;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r5);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r6);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r5 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r6 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            lr = IOUtil.ReadU16LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r3 += r4 << 16;
                            r4 >>= 16;
                            r4 += r5 << 16;
                            r5 >>= 16;
                            r5 += r6 << 16;
                            r6 >>= 16;
                            r6 += lr << 16;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            r9 += r5;
                            r10 += r6;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r9);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r10);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 13:
                    {
                        SrcOffset += 2;
                        uint r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r5 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r6 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                        uint lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        lr = (lr >> 1) & 0x7F7F7F7F;
                        uint r11 = r3 + (r4 << 16);
                        r11 += r3 >> 8;
                        r3 = r11 + (r4 << 8);
                        r4 >>= 16;
                        r11 = r4 + (r5 << 16);
                        r11 += r4 >> 8;
                        r4 = r11 + (r6 << 8);
                        r5 >>= 16;
                        r11 = r5 + (r6 << 16);
                        r11 += r5 >> 8;
                        r5 = r11 + (r6 << 8);
                        r6 >>= 16;
                        r11 = r6 + (lr << 16);
                        r11 += r6 >> 8;
                        r6 = r11 + (lr << 8);
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r10 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            lr = (lr >> 1) & 0x7F7F7F7F;
                            r11 = r7 + (r8 << 16);
                            r11 += r7 >> 8;
                            r7 = r11 + (r8 << 8);
                            r8 >>= 16;
                            r11 = r8 + (r9 << 16);
                            r11 += r8 >> 8;
                            r8 = r11 + (r9 << 8);
                            r9 >>= 16;
                            r11 = r9 + (r10 << 16);
                            r11 += r9 >> 8;
                            r9 = r11 + (r10 << 8);
                            r10 >>= 16;
                            r11 = r10 + (lr << 16);
                            r11 += r10 >> 8;
                            r10 = r11 + (lr << 8);
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            r5 += r9;
                            r6 += r10;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r5);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r6);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = IOUtil.ReadU16LE(Src, SrcOffset - 2);
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r5 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r6 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            lr = (lr >> 1) & 0x7F7F7F7F;
                            r11 = r3 + (r4 << 16);
                            r11 += r3 >> 8;
                            r3 = r11 + (r4 << 8);
                            r4 >>= 16;
                            r11 = r4 + (r5 << 16);
                            r11 += r4 >> 8;
                            r4 = r11 + (r5 << 8);
                            r5 >>= 16;
                            r11 = r5 + (r6 << 16);
                            r11 += r5 >> 8;
                            r5 = r11 + (r6 << 8);
                            r6 >>= 16;
                            r11 = r6 + (lr << 16);
                            r11 += r6 >> 8;
                            r6 = r11 + (lr << 8);
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            r9 += r5;
                            r10 += r6;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r9);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r10);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 14:
                    {
                        SrcOffset++;
                        uint r3 = Src[SrcOffset - 1];
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r5 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r6 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                        uint lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                        SrcOffset += Stride;
                        r3 += r4 << 8;
                        r4 >>= 24;
                        r4 += r5 << 8;
                        r5 >>= 24;
                        r5 += r6 << 8;
                        r6 >>= 24;
                        r6 += lr << 8;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset - 1];
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r10 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r7 += r8 << 8;
                            r8 >>= 24;
                            r8 += r9 << 8;
                            r9 >>= 24;
                            r9 += r10 << 8;
                            r10 >>= 24;
                            r10 += lr << 8;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            r5 += r9;
                            r6 += r10;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r5);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r6);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset - 1];
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r5 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r6 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r3 += r4 << 8;
                            r4 >>= 24;
                            r4 += r5 << 8;
                            r5 >>= 24;
                            r5 += r6 << 8;
                            r6 >>= 24;
                            r6 += lr << 8;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            r9 += r5;
                            r10 += r6;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r9);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r10);
                            DstOffset += Stride;
                        }
                        break;
                    }
                case 15:
                    {
                        SrcOffset++;
                        uint r3 = Src[SrcOffset - 1];
                        uint r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                        uint r5 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                        uint r6 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                        uint lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                        SrcOffset += Stride;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        lr = (lr >> 1) & 0x7F7F7F7F;
                        r3 += r4 << 8;
                        r3 += r4;
                        r4 = r5 + (r4 >> 24);
                        r4 += r5 << 8;
                        r5 = r6 + (r5 >> 24);
                        r5 += r6 << 8;
                        r6 = lr + (r6 >> 24);
                        r6 += lr << 8;
                        r3 = (r3 >> 1) & 0x7F7F7F7F;
                        r4 = (r4 >> 1) & 0x7F7F7F7F;
                        r5 = (r5 >> 1) & 0x7F7F7F7F;
                        r6 = (r6 >> 1) & 0x7F7F7F7F;
                        for (int i = 0; i < r2; i++)
                        {
                            uint r7 = Src[SrcOffset - 1];
                            uint r8 = IOUtil.ReadU32LE(Src, SrcOffset);
                            uint r9 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            uint r10 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            lr = (lr >> 1) & 0x7F7F7F7F;
                            r7 += r8 << 8;
                            r7 += r8;
                            r8 = r9 + (r8 >> 24);
                            r8 += r9 << 8;
                            r9 = r10 + (r9 >> 24);
                            r9 += r10 << 8;
                            r10 = lr + (r10 >> 24);
                            r10 += lr << 8;
                            r7 = (r7 >> 1) & 0x7F7F7F7F;
                            r8 = (r8 >> 1) & 0x7F7F7F7F;
                            r9 = (r9 >> 1) & 0x7F7F7F7F;
                            r10 = (r10 >> 1) & 0x7F7F7F7F;
                            r3 += r7;
                            r4 += r8;
                            r5 += r9;
                            r6 += r10;
                            IOUtil.WriteU32LE(Dst, DstOffset, r3);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r4);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r5);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r6);
                            DstOffset += Stride;
                            if (++i >= r2) break;
                            r3 = Src[SrcOffset - 1];
                            r4 = IOUtil.ReadU32LE(Src, SrcOffset);
                            r5 = IOUtil.ReadU32LE(Src, SrcOffset + 4);
                            r6 = IOUtil.ReadU32LE(Src, SrcOffset + 8);
                            lr = IOUtil.ReadU32LE(Src, SrcOffset + 0xC);
                            SrcOffset += Stride;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            lr = (lr >> 1) & 0x7F7F7F7F;
                            r3 += r4 << 8;
                            r3 += r4;
                            r4 = r5 + (r4 >> 24);
                            r4 += r5 << 8;
                            r5 = r6 + (r5 >> 24);
                            r5 += r6 << 8;
                            r6 = lr + (r6 >> 24);
                            r6 += lr << 8;
                            r3 = (r3 >> 1) & 0x7F7F7F7F;
                            r4 = (r4 >> 1) & 0x7F7F7F7F;
                            r5 = (r5 >> 1) & 0x7F7F7F7F;
                            r6 = (r6 >> 1) & 0x7F7F7F7F;
                            r7 += r3;
                            r8 += r4;
                            r9 += r5;
                            r10 += r6;
                            IOUtil.WriteU32LE(Dst, DstOffset, r7);
                            IOUtil.WriteU32LE(Dst, DstOffset + 4, r8);
                            IOUtil.WriteU32LE(Dst, DstOffset + 8, r9);
                            IOUtil.WriteU32LE(Dst, DstOffset + 0xC, r10);
                            DstOffset += Stride;
                        }
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
