using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMobiclip.Utils;

namespace LibMobiclip.Codec.FastAudio
{
    public class FastAudioDecoder
    {
        //<1><1940a3>: Abbrev Number: 64 (DW_TAG_structure_type)
        //   <1940a4>   DW_AT_sibling     : <0x1940fe>	
        //   <1940a6>   DW_AT_name        : FastAudioUnpackData	
        //   <1940ba>   DW_AT_byte_size   : 464	
        //<2><1940bc>: Abbrev Number: 38 (DW_TAG_member)
        //   <1940bd>   DW_AT_name        : Src	
        //   <1940c1>   DW_AT_type        : DW_FORM_ref2 <0x19409b>	
        //   <1940c4>   DW_AT_data_member_location: 2 byte block: 23 0 	(DW_OP_plus_uconst: 0)
        //<2><1940c7>: Abbrev Number: 38 (DW_TAG_member)
        //   <1940c8>   DW_AT_name        : Dst	
        //   <1940cc>   DW_AT_type        : DW_FORM_ref2 <0x19409f>	
        //   <1940cf>   DW_AT_data_member_location: 2 byte block: 23 4 	(DW_OP_plus_uconst: 4)
        //<2><1940d2>: Abbrev Number: 3 (DW_TAG_array_type)
        //   <1940d3>   DW_AT_sibling     : <0x1940db>	
        //   <1940d5>   DW_AT_type        : DW_FORM_ref2 <0x194087>	
        //<3><1940d8>: Abbrev Number: 1 (DW_TAG_subrange_type)
        //   <1940d9>   DW_AT_upper_bound : 112	
        //<2><1940db>: Abbrev Number: 38 (DW_TAG_member)
        //   <1940dc>   DW_AT_name        : Internal	
        //   <1940e5>   DW_AT_type        : DW_FORM_ref2 <0x1940d2>	
        //   <1940e8>   DW_AT_data_member_location: 2 byte block: 23 8 	(DW_OP_plus_uconst: 8)
        //<2><1940eb>: Abbrev Number: 38 (DW_TAG_member)
        //   <1940ec>   DW_AT_name        : Increment	
        //   <1940f6>   DW_AT_type        : DW_FORM_ref2 <0x194087>	
        //   <1940f9>   DW_AT_data_member_location: 3 byte block: 23 cc 3 	(DW_OP_plus_uconst: 460)
        public byte[] Data;
        public int Offset = 0;
        private uint[] Internal = new uint[113];
        private uint Increment;

        public short[] Decode()
        {
            sub_C48();
            short[] result = new short[256];
            int dstoffset = 0;
            int intoffset = 16;
            sub_11F4(result, ref dstoffset, ref intoffset, 0, (int)Internal[12] * 8, (int)Internal[8]);
            sub_11F4(result, ref dstoffset, ref intoffset, 0, (int)Internal[13] * 8, (int)Internal[9]);
            sub_11F4(result, ref dstoffset, ref intoffset, 0, (int)Internal[14] * 8, (int)Internal[10]);
            sub_11F4(result, ref dstoffset, ref intoffset, 0, (int)Internal[15] * 8, (int)Internal[11]);
            int r11 = 0x6E14;
            int r9 = (int)Internal[109];
            for (int i = 0; i < 256; i++)
            {
                int r5 = result[i];
                for (int j = 0; j < 8; j++)
                {
                    int r6 = (int)Internal[7 - j];
                    int r7 = (int)Internal[107 - j];
                    r5 -= (r6 * r7 + 0x4000) >> 15;
                    Internal[108 - j] = (uint)(r7 + ((r6 * r5 + 0x4000) >> 15));
                }
                Internal[100] = (uint)r5;
                r9 = r5 + ((r9 * r11 + 0x4000) >> 15);
                int r8 = r9 * 2;
                if (r8 > 32767) r8 = 32767;
                if (r8 < -32768) r8 = -32768;
                result[i] = (short)r8;
            }
            Internal[109] = (uint)r9;
            return result;
        }

        private int[] dword_9C8 =
        {
            -0x7F99,-0x7ECC,-0x7E00,-0x7D33,-0x7C66,-0x7B99,-0x7ACC,
            -0x7A00,-0x77FF,-0x74CD,-0x7199,-0x6E65,-0x6B33,-0x67FF,
            -0x64CD,-0x6199,-0x5E65,-0x5B33,-0x57FF,-0x5334,-0x4CCC,
            -0x4668,-0x4000,-0x3998,-0x3334,-0x2CCC,-0x2668,-0x2000,
            -0x1998,-0x1334,-0xCCC,-0x668, 0,0x668,0xCCC,0x1334,
            0x1998,0x2000,0x2668,0x2CCC,0x3334,0x3998,0x4000,0x4668,
            0x4CCC,0x5334,0x57FF,0x5B33,0x5E67,0x6199,0x64CD,0x67FF,
            0x6B33,0x6E65,0x7199,0x74CD,0x77FF,0x7A00,0x7ACC,0x7B99,
            0x7C66,0x7D33,0x7E00,0x7ECC
        };

        private int[] dword_AC8 =
        {    
            -0x6B33,-0x67FF,-0x64CD,-0x6199,-0x5E65,-0x5B33,-0x57FF,
            -0x5334,-0x4CCC,-0x4668,-0x4000,-0x3998,-0x3334,-0x2CCC,
            -0x2668,-0x2000,-0x1998,-0x1334,-0xCCC,-0x668, 0,0x668,
            0xCCC,0x1334,0x1998,0x2000,0x2668,0x2CCC,0x3334,0x3998,
            0x4000,0x4668
        };

        private int[] dword_B48 =
        {
            -0x4668,-0x4000,-0x3998,-0x3334,-0x2CCC,-0x2668,-0x2000,
            -0x1998,-0x1334,-0xCCC,-0x668, 0,0x668,0xCCC,0x1334,
            0x1998,0x2000,0x2668,0x2CCC,0x3334,0x3998,0x4000,0x4668,
            0x4CCC,0x5334,0x57FF,0x5B33,0x5E67,0x6199,0x64CD,0x67FF,
            0x6B33
        };

        private int[] dword_BC8 =
        {
            -0x4CD0,-0x436C,-0x3A0C,-0x30A8,-0x2744,-0x1DE0,-0x1480,
            -0xB1C,-0x1B8,0x7A8,0x110C,0x1A70,0x23D4,0x2D34,0x3698,
            0x3FFC
        };

        private int[] dword_C08 =
        {
            -0x3334,-0x23D8,-0x147C,-0x520,0xA3C,0x1998,0x28F4,
            0x384C
        };

        private int[] dword_C28 =
        {
            -0x199C,-0xB1C,0x368,0x11E8,0x2068,0x2EEC,0x3D6C,0x4BEC
        };

        private int[] dword_11B4 =
        {
            -0x2668,-0x1DDC,-0x1554,-0xCCC,-0x444,0x444,0xCCC,
            0x1554,0x1DDC,0x2668,0x2EF0,0x3778,0x4000,0x4888,0x5110,
            0x57FF
        };

        private void sub_C48()
        {
            uint r3 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            uint r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r3 |= r4 << 16;
            Internal[0] = (uint)dword_9C8[r3 >> 26];
            Internal[1] = (uint)dword_9C8[(r3 >> 20) & 0x3F];
            Internal[2] = (uint)dword_AC8[(r3 >> 15) & 0x1F];
            Internal[3] = (uint)dword_B48[(r3 >> 10) & 0x1F];
            Internal[4] = (uint)dword_BC8[(r3 >> 6) & 0xF];
            Internal[6] = (uint)dword_C08[(r3 >> 3) & 0x7];
            Internal[7] = (uint)dword_C28[r3 & 0x7];
            r3 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r3 |= r4 << 16;
            Internal[15] = r3 >> 26;
            Internal[14] = (r3 >> 20) & 0x3F;
            Internal[13] = (r3 >> 14) & 0x3F;
            Internal[12] = (r3 >> 8) & 0x3F;
            Internal[11] = (r3 >> 6) & 3;
            Internal[10] = (r3 >> 4) & 3;
            Internal[9] = (r3 >> 2) & 3;
            Internal[8] = r3 & 3;
            r3 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r3 |= r4 << 16;
            Internal[16] = r3 >> 29;
            Internal[17] = (r3 >> 26) & 7;
            Internal[18] = (r3 >> 23) & 7;
            Internal[19] = (r3 >> 20) & 7;
            Internal[20] = (r3 >> 17) & 7;
            Internal[21] = (r3 >> 14) & 7;
            Internal[22] = (r3 >> 11) & 7;
            Internal[23] = (r3 >> 8) & 7;
            Internal[24] = (r3 >> 5) & 7;
            Internal[25] = (r3 >> 2) & 7;
            uint r5 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r5 |= r4 << 16;
            Internal[26] = r5 >> 29;
            Internal[27] = (r5 >> 26) & 7;
            Internal[28] = (r5 >> 23) & 7;
            Internal[29] = (r5 >> 20) & 7;
            Internal[30] = (r5 >> 17) & 7;
            Internal[31] = (r5 >> 14) & 7;
            Internal[32] = (r5 >> 11) & 7;
            Internal[33] = (r5 >> 8) & 7;
            Internal[34] = (r5 >> 5) & 7;
            Internal[35] = (r5 >> 2) & 7;
            Internal[36] = ((r5 >> 1) & 1) | ((r3 & 3) << 1);
            uint v19 = r5;
            r3 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r3 |= r4 << 16;
            Internal[37] = r3 >> 29;
            Internal[38] = (r3 >> 26) & 7;
            Internal[39] = (r3 >> 23) & 7;
            Internal[40] = (r3 >> 20) & 7;
            Internal[41] = (r3 >> 17) & 7;
            Internal[42] = (r3 >> 14) & 7;
            Internal[43] = (r3 >> 11) & 7;
            Internal[44] = (r3 >> 8) & 7;
            Internal[45] = (r3 >> 5) & 7;
            Internal[46] = (r3 >> 2) & 7;
            r5 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r5 |= r4 << 16;
            Internal[47] = r5 >> 29;
            Internal[48] = (r5 >> 26) & 7;
            Internal[49] = (r5 >> 23) & 7;
            Internal[50] = (r5 >> 20) & 7;
            Internal[51] = (r5 >> 17) & 7;
            Internal[52] = (r5 >> 14) & 7;
            Internal[53] = (r5 >> 11) & 7;
            Internal[54] = (r5 >> 8) & 7;
            Internal[55] = (r5 >> 5) & 7;
            Internal[56] = (r5 >> 2) & 7;
            Internal[57] = ((r5 >> 1) & 1) | ((r3 & 3) << 1);
            uint v27 = r5;
            r3 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r3 |= r4 << 16;
            Internal[58] = r3 >> 29;
            Internal[59] = (r3 >> 26) & 7;
            Internal[60] = (r3 >> 23) & 7;
            Internal[61] = (r3 >> 20) & 7;
            Internal[62] = (r3 >> 17) & 7;
            Internal[63] = (r3 >> 14) & 7;
            Internal[64] = (r3 >> 11) & 7;
            Internal[65] = (r3 >> 8) & 7;
            Internal[66] = (r3 >> 5) & 7;
            Internal[67] = (r3 >> 2) & 7;
            r5 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r5 |= r4 << 16;
            Internal[68] = r5 >> 29;
            Internal[69] = (r5 >> 26) & 7;
            Internal[70] = (r5 >> 23) & 7;
            Internal[71] = (r5 >> 20) & 7;
            Internal[72] = (r5 >> 17) & 7;
            Internal[73] = (r5 >> 14) & 7;
            Internal[74] = (r5 >> 11) & 7;
            Internal[75] = (r5 >> 8) & 7;
            Internal[76] = (r5 >> 5) & 7;
            Internal[77] = (r5 >> 2) & 7;
            Internal[78] = ((r5 >> 1) & 1) | ((r3 & 3) << 1);
            uint v35 = r5;
            r3 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r3 |= r4 << 16;
            Internal[79] = r3 >> 29;
            Internal[80] = (r3 >> 26) & 7;
            Internal[81] = (r3 >> 23) & 7;
            Internal[82] = (r3 >> 20) & 7;
            Internal[83] = (r3 >> 17) & 7;
            Internal[84] = (r3 >> 14) & 7;
            Internal[85] = (r3 >> 11) & 7;
            Internal[86] = (r3 >> 8) & 7;
            Internal[87] = (r3 >> 5) & 7;
            Internal[88] = (r3 >> 2) & 7;
            r5 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r4 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            r5 |= r4 << 16;
            Internal[89] = r5 >> 29;
            Internal[90] = (r5 >> 26) & 7;
            Internal[91] = (r5 >> 23) & 7;
            Internal[92] = (r5 >> 20) & 7;
            Internal[93] = (r5 >> 17) & 7;
            Internal[94] = (r5 >> 14) & 7;
            Internal[95] = (r5 >> 11) & 7;
            Internal[96] = (r5 >> 8) & 7;
            Internal[97] = (r5 >> 5) & 7;
            Internal[98] = (r5 >> 2) & 7;
            Internal[99] = ((r5 >> 1) & 1) | ((r3 & 3) << 1);
            Internal[5] = (uint)dword_11B4[(r5 & 1) | ((v35 & 1) << 1) | ((v27 & 1) << 2) | ((v19 & 1) << 3)];
        }

        private void sub_11F4(short[] Dst, ref int DstOffset, ref int InternalOffset, short r6, int TableOffset, int r8)
        {
            if (r8 != 0)
            {
                for (int i = 0; i < r8; i++)
                {
                    Dst[DstOffset++] = r6;
                }
            }
            for (int i = 0; i < 20; i++)
            {
                int r9 = dword_1570[TableOffset + (int)Internal[InternalOffset++]];
                Dst[DstOffset++] = (short)r9;
                Dst[DstOffset++] = r6;
                Dst[DstOffset++] = r6;
            }
            int r9_2 = dword_1570[TableOffset + (int)Internal[InternalOffset++]];
            Dst[DstOffset++] = (short)r9_2;
            int r12 = 3 - r8;
            if (r12 == 0) return;
            for (int i = 0; i < r12; i++)
            {
                Dst[DstOffset++] = r6;
            }
        }

        private int[] dword_1570 = 
        {
            -0x1C,-0x14,-0xC,-4, 4,0xC,0x14,0x1C,-0x38,-0x28,-0x18,
            -8, 8,0x18,0x28,0x38,-0x54,-0x3C,-0x24,-0xC,0xC,0x24,
            0x3C,0x54,-0x70,-0x50,-0x30,-0x10,0x10,0x30,0x50,0x70,
            -0x8C,-0x64,-0x3C,-0x14,0x14,0x3C,0x64,0x8C,-0xA8,
            -0x78,-0x48,-0x18,0x18,0x48,0x78,0xA8,-0xC4,-0x8C,
            -0x54,-0x1C,0x1C,0x54,0x8C,0xC4,-0xE0,-0xA0,-0x60,
            -0x20,0x20,0x60,0xA0,0xE0,-0xFC,-0xB4,-0x6C,-0x24,
            0x24,0x6C,0xB4,0xFC,-0x118,-0xC8,-0x78,-0x28,0x28,
            0x78,0xC8,0x118,-0x134,-0xDC,-0x84,-0x2C,0x2C,0x84,
            0xDC,0x134,-0x150,-0xF0,-0x90,-0x30,0x30,0x90,0xF0,
            0x150,-0x16C,-0x104,-0x9C,-0x34,0x34,0x9C,0x104,0x16C,
            -0x188,-0x118,-0xA8,-0x38,0x38,0xA8,0x118,0x188,-0x1A4,
            -0x12C,-0xB4,-0x3C,0x3C,0xB4,0x12C,0x1A4,-0x1C0,-0x140,
            -0xC0,-0x40,0x40,0xC0,0x140,0x1C0,-0x1F8,-0x168,-0xD8,
            -0x48,0x48,0xD8,0x168,0x1F8,-0x230,-0x190,-0xF0,-0x50,
            0x50,0xF0,0x190,0x230,-0x268,-0x1B8,-0x108,-0x58,0x58,
            0x108,0x1B8,0x268,-0x2A0,-0x1E0,-0x120,-0x60,0x60,
            0x120,0x1E0,0x2A0,-0x2D8,-0x208,-0x138,-0x68,0x68,
            0x138,0x208,0x2D8,-0x310,-0x230,-0x150,-0x70,0x70,
            0x150,0x230,0x310,-0x348,-0x258,-0x168,-0x78,0x78,
            0x168,0x258,0x348,-0x380,-0x280,-0x180,-0x80,0x80,
            0x180,0x280,0x380,-0x3F0,-0x2D0,-0x1B0,-0x90,0x90,
            0x1B0,0x2D0,0x3F0,-0x460,-0x320,-0x1E0,-0xA0,0xA0,
            0x1E0,0x320,0x460,-0x4D0,-0x370,-0x210,-0xB0,0xB0,
            0x210,0x370,0x4D0,-0x540,-0x3C0,-0x240,-0xC0,0xC0,
            0x240,0x3C0,0x540,-0x5B0,-0x410,-0x270,-0xD0,0xD0,
            0x270,0x410,0x5B0,-0x620,-0x460,-0x2A0,-0xE0,0xE0,
            0x2A0,0x460,0x620,-0x690,-0x4B0,-0x2D0,-0xF0,0xF0,
            0x2D0,0x4B0,0x690,-0x700,-0x500,-0x300,-0x100,0x100,
            0x300,0x500,0x700,-0x7E0,-0x5A0,-0x360,-0x120,0x120,
            0x360,0x5A0,0x7E0,-0x8C0,-0x640,-0x3C0,-0x140,0x140,
            0x3C0,0x640,0x8C0,-0x9A0,-0x6E0,-0x420,-0x160,0x160,
            0x420,0x6E0,0x9A0,-0xA80,-0x780,-0x480,-0x180,0x180,
            0x480,0x780,0xA80,-0xB60,-0x820,-0x4E0,-0x1A0,0x1A0,
            0x4E0,0x820,0xB60,-0xC40,-0x8C0,-0x540,-0x1C0,0x1C0,
            0x540,0x8C0,0xC40,-0xD20,-0x960,-0x5A0,-0x1E0,0x1E0,
            0x5A0,0x960,0xD20,-0xE00,-0xA00,-0x600,-0x200,0x200,
            0x600,0xA00,0xE00,-0xFC0,-0xB40,-0x6C0,-0x240,0x240,
            0x6C0,0xB40,0xFC0,-0x1180,-0xC80,-0x780,-0x280,0x280,
            0x780,0xC80,0x1180,-0x1340,-0xDC0,-0x840,-0x2C0,0x2C0,
            0x840,0xDC0,0x1340,-0x1500,-0xF00,-0x900,-0x300,0x300,
            0x900,0xF00,0x1500,-0x16C0,-0x1040,-0x9C0,-0x340,0x340,
            0x9C0,0x1040,0x16C0,-0x1880,-0x1180,-0xA80,-0x380,
            0x380,0xA80,0x1180,0x1880,-0x1A40,-0x12C0,-0xB40,-0x3C0,
            0x3C0,0xB40,0x12C0,0x1A40,-0x1C00,-0x1400,-0xC00,-0x400,
            0x400,0xC00,0x1400,0x1C00,-0x1F7F,-0x167F,-0xD80,-0x480,
            0x480,0xD80,0x1680,0x1F80,-0x22FF,-0x18FF,-0xF00,-0x500,
            0x500,0xF00,0x1900,0x2300,-0x267F,-0x1B7F,-0x1080,
            -0x580,0x580,0x1080,0x1B80,0x2680,-0x29FF,-0x1DFF,
            -0x1200,-0x600,0x600,0x1200,0x1E00,0x2A00,-0x2D7F,
            -0x207F,-0x1380,-0x680,0x680,0x1380,0x2080,0x2D80,
            -0x30FF,-0x22FF,-0x1500,-0x700,0x700,0x1500,0x2300,
            0x3100,-0x347F,-0x257F,-0x1680,-0x780,0x780,0x1680,
            0x2580,0x3480,-0x37FF,-0x27FF,-0x1800,-0x800,0x800,
            0x1800,0x2800,0x3800,-0x3EFF,-0x2CFF,-0x1B00,-0x900,
            0x900,0x1B00,0x2CFF,0x3EFF,-0x45FF,-0x31FF,-0x1E00,
            -0xA00,0xA00,0x1E00,0x31FF,0x45FF,-0x4CFF,-0x36FF,
            -0x2100,-0xB00,0xB00,0x2100,0x36FF,0x4CFF,-0x53FF,
            -0x3BFF,-0x2400,-0xC00,0xC00,0x2400,0x3BFF,0x53FF,
            -0x5AFF,-0x40FF,-0x2700,-0xD00,0xD00,0x2700,0x40FF,
            0x5AFF,-0x61FF,-0x45FF,-0x2A00,-0xE00,0xE00,0x2A00,
            0x45FF,0x61FF,-0x68FF,-0x4AFF,-0x2D00,-0xF00,0xF00,
            0x2D00,0x4AFF,0x68FF,-0x6FFF,-0x4FFF,-0x3000,-0x1000,
            0x1000,0x3000,0x4FFF,0x6FFF
        };
    }
}
