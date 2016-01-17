using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMobiclip.Utils;

namespace LibMobiclip.Codec.Sx
{
    public class SxDecoder
    {
        //<1><db2>: Abbrev Number: 135 (DW_TAG_class_type)
        //   <db4>   DW_AT_byte_size   : 0x14f8	
        //   <db8>   DW_AT_name        : __SxUnpack	
        //<2><dc3>: Abbrev Number: 136 (DW_TAG_member)
        //   <dc5>   DW_AT_data_member_location: 5 byte block: c 0 0 0 0 	(DW_OP_const4u: 0)
        //   <dcc>   DW_AT_type        : <0xe29>	
        //   <dd0>   DW_AT_name        : Src	
        //<2><dd4>: Abbrev Number: 136 (DW_TAG_member)
        //   <dd6>   DW_AT_data_member_location: 5 byte block: c 4 0 0 0 	(DW_OP_const4u: 4)
        //   <ddd>   DW_AT_type        : <0xe2f>	
        //   <de1>   DW_AT_name        : Dst	
        //<2><de5>: Abbrev Number: 136 (DW_TAG_member)
        //   <de7>   DW_AT_data_member_location: 5 byte block: c 8 0 0 0 	(DW_OP_const4u: 8)
        //   <dee>   DW_AT_type        : <0xe9d>	
        //   <df2>   DW_AT_name        : Internal	
        //<2><dfb>: Abbrev Number: 136 (DW_TAG_member)
        //   <dfd>   DW_AT_data_member_location: 5 byte block: c c0 8 0 0 	(DW_OP_const4u: 2240)
        //   <e04>   DW_AT_type        : <0xeae>	
        //   <e08>   DW_AT_name        : Codebook	
        //<2><e11>: Abbrev Number: 136 (DW_TAG_member)
        //   <e13>   DW_AT_data_member_location: 5 byte block: c f4 14 0 0 	(DW_OP_const4u: 5364)
        //   <e1a>   DW_AT_type        : <0x2eb>	
        //   <e1e>   DW_AT_name        : Increment	
        public byte[] Data;
        public int Offset = 0;
        private byte[] Internal = new byte[0x8B8];//Offset 0x8
        public byte[] Codebook = new byte[0xC34];//Offset 0x8C0
        private uint Increment;

        private byte[] table_83E = { 0x14, 0xE, 0xC, 0xA, 0, 0 };

        public short[] Decode()
        {
            ushort val = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            int result;
            if ((val >> 9) == 0x7F) result = sub_8BC(ref val);
            else result = sub_8FC(ref val);
            IOUtil.WriteU32LE(Internal, 0x64, IOUtil.ReadU32LE(Internal, 0x64) ^ 1);
            short[] dst = new short[128];
            for (int i = 0; i < 128; i++)
            {
                int r7 = (int)IOUtil.ReadU32LE(Internal, result);
                result += 4;
                if (r7 > 32767) r7 = 32767;
                if (r7 < -32768) r7 = -32768;
                dst[i] = (short)r7;
            }
            int r0 = table_83E[Internal[0x6B]];
            return dst;
        }

        private void sub_0(int InternalOffset)
        {
            InternalOffset += 0x200;
            for (int i = 0; i < 32; i++)
            {
                IOUtil.WriteU32LE(Internal, InternalOffset + 0, 0);
                IOUtil.WriteU32LE(Internal, InternalOffset + 4, 0);
                IOUtil.WriteU32LE(Internal, InternalOffset + 8, 0);
                IOUtil.WriteU32LE(Internal, InternalOffset + 0xC, 0);
                InternalOffset += 16;
            }
        }

        private void sub_28(int InternalOffset, int InternalOffset2, int r6)
        {
            InternalOffset += 0x200;
            int r12 = 0x7F - r6;
            InternalOffset2 += r12 * 4;
            int r2 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            int r1 = 2;
            r2 >>= 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)r2);
            InternalOffset += 4;
            r2 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            int r7 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            int r8 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            int r9 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            int r10 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            int r11 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r2 * r1) >> 4));
            r1++;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r7 * r1) >> 4));
            r1++;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r8 * r1) >> 4));
            r1++;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r9 * r1) >> 4));
            r1++;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r10 * r1) >> 4));
            r1++;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r11 * r1) >> 4));
            //r1++;
            InternalOffset += 4;
            r12 = 0x72;
            while (true)
            {
                r6 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
                InternalOffset2 += 4;
                r7 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
                InternalOffset2 += 4;
                r8 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
                InternalOffset2 += 4;
                r9 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
                InternalOffset2 += 4;
                r10 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
                InternalOffset2 += 4;
                r11 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
                InternalOffset2 += 4;
                IOUtil.WriteU32LE(Internal, InternalOffset, (uint)(r6 >> 1));
                InternalOffset += 4;
                IOUtil.WriteU32LE(Internal, InternalOffset, (uint)(r7 >> 1));
                InternalOffset += 4;
                IOUtil.WriteU32LE(Internal, InternalOffset, (uint)(r8 >> 1));
                InternalOffset += 4;
                IOUtil.WriteU32LE(Internal, InternalOffset, (uint)(r9 >> 1));
                InternalOffset += 4;
                IOUtil.WriteU32LE(Internal, InternalOffset, (uint)(r10 >> 1));
                InternalOffset += 4;
                IOUtil.WriteU32LE(Internal, InternalOffset, (uint)(r11 >> 1));
                InternalOffset += 4;
                r12 -= 6;
                if (r12 == 0) break;
            }
            r2 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            r7 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            r8 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            r9 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            r10 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            r11 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r2 * r1) >> 4));
            r1--;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r7 * r1) >> 4));
            r1--;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r8 * r1) >> 4));
            r1--;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r9 * r1) >> 4));
            r1--;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r10 * r1) >> 4));
            r1--;
            InternalOffset += 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)((r11 * r1) >> 4));
            InternalOffset += 4;
            r2 = (int)IOUtil.ReadU32LE(Internal, InternalOffset2);
            InternalOffset2 += 4;
            r2 >>= 4;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)r2);
            InternalOffset += 4;
        }

        private void sub_170(int InternalOffset, ref ushort val, int r6, byte r7, byte r8, byte r9)
        {
            InternalOffset += 0x200;
            InternalOffset += r7 * 4;
            int r7_2 = -r6;
            r6 *= 2;
            r7_2 -= r6;
            for (int i = 0; i < r8; i++)
            {
                val = IOUtil.ReadU16LE(Data, Offset);
                Offset += 2;
                int r11 = 0xE;
                while (true)
                {
                    int r10 = (int)IOUtil.ReadU32LE(Internal, InternalOffset);
                    int r2 = (val >> r11) & 3;
                    r2 = r6 * r2 + r7_2;
                    r10 += r2;
                    IOUtil.WriteU32LE(Internal, InternalOffset, (uint)r10);
                    InternalOffset += r9;
                    r11 -= 2;
                    if (r11 < 0) break;
                }
            }
        }

        private void sub_1B8(int InternalOffset, ref ushort val, int r6, int r7)
        {
            InternalOffset += 0x200;
            InternalOffset += r7 * 4;
            r7 = -r6;
            r6 <<= 1;
            r7 -= r6 << 1;
            r7 -= r6;
            int r9 = 0;
            int r8 = 8;
            for (int i = 0; i < r8; i++)
            {
                val = IOUtil.ReadU16LE(Data, Offset);
                Offset += 2;
                int r11 = 0xD;
                while (true)
                {
                    int r10 = (int)IOUtil.ReadU32LE(Internal, InternalOffset);
                    int r2 = (val >> r11) & 7;
                    r2 = r6 * r2 + r7;
                    r10 += r2;
                    IOUtil.WriteU32LE(Internal, InternalOffset, (uint)r10);
                    InternalOffset += 0xC;
                    r11 -= 3;
                    if (r11 < 0) break;
                }
                val &= 1;
                r9 = (r9 << 1) | val;
            }
            int r10_2 = (int)IOUtil.ReadU32LE(Internal, InternalOffset);
            int r2_2 = (r9 >> 5) & 7;
            r2_2 = r6 * r2_2 + r7;
            r10_2 += r2_2;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)r10_2);
            InternalOffset += 0xC;
            r10_2 = (int)IOUtil.ReadU32LE(Internal, InternalOffset);
            r2_2 = (r9 >> 2) & 7;
            r2_2 = r6 * r2_2 + r7;
            r10_2 += r2_2;
            IOUtil.WriteU32LE(Internal, InternalOffset, (uint)r10_2);
            InternalOffset += 0xC;
        }

        private int[] sub_244(byte[] CodeBookData, int Offset)
        {
            int[] codebookthings = new int[8];
            for (int i = 0; i < 8; i++)
            {
                codebookthings[i] = (int)IOUtil.ReadU32LE(CodeBookData, Offset + i * 4);
            }
            sub_3B4(Internal[0x68] * 16, codebookthings);
            sub_3B4(Internal[0x69] * 16 + 0x400, codebookthings);
            sub_3B4(Internal[0x6A] * 16 + 0x800, codebookthings);
            for (int i = 0; i < 8; i++)
            {
                IOUtil.WriteU32LE(Internal, i * 4, (uint)codebookthings[i]);
            }
            codebookthings[0] += (codebookthings[0] * codebookthings[1]) >> 15;
            int tmp = codebookthings[0] * codebookthings[2];
            codebookthings[0] += (codebookthings[1] * codebookthings[2]) >> 15;
            codebookthings[1] += (tmp) >> 15;
            tmp = codebookthings[0] * codebookthings[3];
            codebookthings[0] += (codebookthings[2] * codebookthings[3]) >> 15;
            codebookthings[2] += (tmp) >> 15;
            codebookthings[1] += (codebookthings[1] * codebookthings[3]) >> 15;
            tmp = codebookthings[0] * codebookthings[4];
            codebookthings[0] += (codebookthings[3] * codebookthings[4]) >> 15;
            codebookthings[3] += (tmp) >> 15;
            tmp = codebookthings[1] * codebookthings[4];
            codebookthings[1] += (codebookthings[2] * codebookthings[4]) >> 15;
            codebookthings[2] += (tmp) >> 15;
            tmp = codebookthings[0] * codebookthings[5];
            codebookthings[0] += (codebookthings[4] * codebookthings[5]) >> 15;
            codebookthings[4] += (tmp) >> 15;
            tmp = codebookthings[1] * codebookthings[5];
            codebookthings[1] += (codebookthings[3] * codebookthings[5]) >> 15;
            codebookthings[3] += (tmp) >> 15;
            codebookthings[2] += (codebookthings[2] * codebookthings[5]) >> 15;
            tmp = codebookthings[0] * codebookthings[6];
            codebookthings[0] += (codebookthings[5] * codebookthings[6]) >> 15;
            codebookthings[5] += (tmp) >> 15;
            tmp = codebookthings[1] * codebookthings[6];
            codebookthings[1] += (codebookthings[4] * codebookthings[6]) >> 15;
            codebookthings[4] += (tmp) >> 15;
            tmp = codebookthings[2] * codebookthings[6];
            codebookthings[2] += (codebookthings[3] * codebookthings[6]) >> 15;
            codebookthings[3] += (tmp) >> 15;
            tmp = codebookthings[0] * codebookthings[7];
            codebookthings[0] += (codebookthings[6] * codebookthings[7]) >> 15;
            codebookthings[6] += (tmp) >> 15;
            tmp = codebookthings[1] * codebookthings[7];
            codebookthings[1] += (codebookthings[5] * codebookthings[7]) >> 15;
            codebookthings[5] += (tmp) >> 15;
            tmp = codebookthings[2] * codebookthings[7];
            codebookthings[2] += (codebookthings[4] * codebookthings[7]) >> 15;
            codebookthings[4] += (tmp) >> 15;
            codebookthings[3] += (codebookthings[3] * codebookthings[7]) >> 15;
            codebookthings[0] = -(codebookthings[0] >> 1);
            codebookthings[1] = -(codebookthings[1] >> 1);
            codebookthings[2] = -(codebookthings[2] >> 1);
            codebookthings[3] = -(codebookthings[3] >> 1);
            codebookthings[4] = -(codebookthings[4] >> 1);
            codebookthings[5] = -(codebookthings[5] >> 1);
            codebookthings[6] = -(codebookthings[6] >> 1);
            codebookthings[7] = -(codebookthings[7] >> 1);
            return codebookthings;
        }

        private void sub_3B4(int CodebookOffset, int[] codebookthings)
        {
            for (int i = 0; i < 8; i++)
            {
                codebookthings[i] += IOUtil.ReadS16LE(Codebook, CodebookOffset);
                CodebookOffset += 2;
            }
        }
        private void sub_3F8(ref int InternalOffset, int r2, int[] codebookthings)
        {
            int intoffset2 = 0x70;
            int r1 = (int)IOUtil.ReadU32LE(Internal, 0x6C);
            while (true)
            {
                int r2_tmp = r2;
                for (int i = 0; i < 8; i++)
                {
                    int r4 = (int)IOUtil.ReadU32LE(Internal, InternalOffset);
                    InternalOffset += 4;
                    r4 <<= 14;
                    int idx = (7 + i) & 7;
                    for (int i2 = 0; i2 < 8; i2++)
                    {
                        r4 += (int)IOUtil.ReadU32LE(Internal, intoffset2 + idx * 4) * codebookthings[i2];
                        idx--;
                        if (idx < 0) idx = 7;
                    }
                    r4 >>= 14;
                    IOUtil.WriteU32LE(Internal, intoffset2 + i * 4, (uint)r4);
                    IOUtil.WriteU32LE(Internal, r1, (uint)r4);
                    r1 += 4;
                }
                r2 = r2_tmp;
                r2 -= 8;
                if (r2 == 0) break;
            }
            IOUtil.WriteU32LE(Internal, 0x6C, (uint)r1);
        }

        private void sub_6C0(int r2, int[] codebookthings)
        {
            for (int i = 0; i < 8; i++)
            {
                codebookthings[i] += (int)IOUtil.ReadU32LE(Internal, r2 + i * 4);
                codebookthings[i] >>= 1;
            }
        }

        private void sub_728(int InternalOffset, int InternalOffset2, int[] codebookthings)
        {
            int r2 = (int)IOUtil.ReadU32LE(Internal, 0x64);
            int r0 = InternalOffset + 0x200;
            IOUtil.WriteU32LE(Internal, 0x6C, (uint)InternalOffset2);
            if (r2 == 1)
            {
                InternalOffset2 = 0x20;
                InternalOffset = InternalOffset2 + 0x20;
            }
            else
            {
                InternalOffset = 0x20;
                InternalOffset2 = InternalOffset + 0x20;
            }
            for (int i = 0; i < 8; i++)
            {
                IOUtil.WriteU32LE(Internal, InternalOffset + i * 4, (uint)codebookthings[i]);
            }
            sub_6C0(InternalOffset2, codebookthings);
            int[] codebookthings2 = new int[8];
            Array.Copy(codebookthings, codebookthings2, 8);
            sub_6C0(InternalOffset2, codebookthings);
            sub_3F8(ref r0, 0x20, codebookthings);
            Array.Copy(codebookthings2, codebookthings, 8);
            sub_3F8(ref r0, 0x20, codebookthings);
            sub_6C0(InternalOffset, codebookthings);
            sub_3F8(ref r0, 0x20, codebookthings);
            for (int i = 0; i < 8; i++)
            {
                codebookthings[i] = (int)IOUtil.ReadU32LE(Internal, InternalOffset + i * 4);
            }
            sub_3F8(ref r0, 0x20, codebookthings);
        }

        private void sub_798()
        {
            IOUtil.WriteU32LE(Internal, 0x60, IOUtil.ReadU32LE(Codebook, 0xC30));
            IOUtil.WriteU32LE(Internal, 0x64, 1);
            int internaloffset = 0x70;
            for (int i = 0; i < 8; i++)
            {
                IOUtil.WriteU32LE(Internal, internaloffset, 0);
                internaloffset += 4;
            }
        }

        private byte[] table_836 = { 0, 0, 5, 0xC, 4, 0x10, 3, 0x14 };

        private void sub_844(int InternalOffset, ref ushort val)
        {
            int r6 = (val >> 6) & 7;
            Internal[0x68] = (byte)(val & 0x3F);
            val = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            int codebookoffset = 0xC00;
            int r7 = (val >> 14) & 3;
            int r8 = IOUtil.ReadS16LE(Codebook, codebookoffset + r6 * 2);
            r6 = (int)IOUtil.ReadU32LE(Internal, 0x60);
            uint r11 = (uint)((val >> 12) & 3);//something bitrate related
            r6 = r8 * r6;
            int r10 = (val >> 6) & 0x3F;
            r6 >>= 13;
            IOUtil.WriteU32LE(Internal, 0x60, (uint)r6);
            Internal[0x69] = (byte)r10;
            Internal[0x6A] = (byte)(val & 0x3F);
            Internal[0x6B] = (byte)r11;
            if (r11 == 0) sub_1B8(InternalOffset, ref val, r6, (byte)r7);
            else
            {
                sub_170(InternalOffset, ref val, r6, (byte)r7, table_836[r11 * 2], table_836[r11 * 2 + 1]);
            }
        }

        private int sub_8BC(ref ushort val)
        {
            sub_798();
            sub_0(0x4B8);
            sub_844(0x4B8, ref val);
            int[] codebookthings = sub_244(Codebook, 0xC10);
            for (int i = 0; i < 8; i++)
            {
                IOUtil.WriteU32LE(Internal, 0x40 + i * 4, (uint)codebookthings[i]);
            }
            IOUtil.WriteU32LE(Internal, 0x6C, 0xB8);
            int internaloffset = 0x6B8;
            sub_3F8(ref internaloffset, 0x80, codebookthings);
            return 0xB8;//0x4B8;
        }

        private int sub_8FC(ref ushort val)
        {
            int r7 = 0x400;
            int r2 = (int)IOUtil.ReadU32LE(Internal, 0x64);
            int r3 = r2 * r7 + 0xB8;
            r2 ^= 1;
            int r4 = r2 * r7 + 0xB8;
            r7 = r4 + 0x200;
            for (int i = 0; i < 32; i++)
            {
                IOUtil.WriteU32LE(Internal, r3 + i * 16 + 0, IOUtil.ReadU32LE(Internal, r7 + i * 16 + 0));
                IOUtil.WriteU32LE(Internal, r3 + i * 16 + 4, IOUtil.ReadU32LE(Internal, r7 + i * 16 + 4));
                IOUtil.WriteU32LE(Internal, r3 + i * 16 + 8, IOUtil.ReadU32LE(Internal, r7 + i * 16 + 8));
                IOUtil.WriteU32LE(Internal, r3 + i * 16 + 12, IOUtil.ReadU32LE(Internal, r7 + i * 16 + 12));
            }
            if ((val >> 9) == 0x7E) sub_0(r3);
            else sub_28(r3, r4, val >> 9);
            sub_844(r3, ref val);
            int[] codebookthings = sub_244(Internal, 0);
            sub_728(r3, r4, codebookthings);
            return r4;
        }
    }
}
