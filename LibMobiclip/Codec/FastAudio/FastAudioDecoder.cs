using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMobiclip.Utils;

namespace LibMobiclip.Codec.FastAudio
{
    public class FastAudioDecoder
    {
        public byte[] Data;
        public int Offset = 0;

        //0 - 7
        private readonly int[] _multipliers = new int[8];
        //8 - 11
        private readonly uint[] _startPads = new uint[4];
        //12 - 15
        private readonly uint[] _tableIndices = new uint[4];
        //16 - 99
        private readonly uint[][] _tableSamples = new uint[4][]
            {new uint[21], new uint[21], new uint[21], new uint[21]};
        //100 - 108
        private readonly int[] _filterStuff = new int[9];
        //109
        private int _lastHalfSample = 0;

        public short[] Decode()
        {
            ReadFrame();
            short[] result = new short[256];
            for (int i = 0; i < 4; i++)
                DecodeSubFrame(result, i * 64, _tableSamples[i], _tableIndices[i], _startPads[i]);
            for (int i = 0; i < 256; i++)
            {
                int r5 = result[i];
                for (int j = 0; j < 8; j++)
                {
                    int multiplier = _multipliers[7 - j];
                    int r7 = _filterStuff[7 - j];
                    r5 -= (multiplier * r7 + 0x4000) >> 15;
                    _filterStuff[8 - j] = r7 + ((multiplier * r5 + 0x4000) >> 15);
                }
                _filterStuff[0] = r5;
                _lastHalfSample = r5 + ((_lastHalfSample * 28180 + 0x4000) >> 15);
                int sample = _lastHalfSample * 2;
                if (sample < -32768)
                    sample = -32768;
                if (sample > 32767)
                    sample = 32767;
                result[i] = (short)sample;
            }
            return result;
        }

        private void ReadFrame()
        {
            uint r3 = IOUtil.ReadU32LE(Data, Offset);
            Offset += 4;
            _multipliers[0] = Multiplier01Table[r3 >> 26];
            _multipliers[1] = Multiplier01Table[(r3 >> 20) & 0x3F];
            _multipliers[2] = Multiplier2Table[(r3 >> 15) & 0x1F];
            _multipliers[3] = Multiplier3Table[(r3 >> 10) & 0x1F];
            _multipliers[4] = Multiplier4Table[(r3 >> 6) & 0xF];
            _multipliers[6] = Multiplier6Table[(r3 >> 3) & 0x7];
            _multipliers[7] = Multiplier7Table[r3 & 0x7];
            r3 = IOUtil.ReadU32LE(Data, Offset);
            Offset += 4;
            _tableIndices[3] = r3 >> 26;
            _tableIndices[2] = (r3 >> 20) & 0x3F;
            _tableIndices[1] = (r3 >> 14) & 0x3F;
            _tableIndices[0] = (r3 >> 8) & 0x3F;
            _startPads[3] = (r3 >> 6) & 3;
            _startPads[2] = (r3 >> 4) & 3;
            _startPads[1] = (r3 >> 2) & 3;
            _startPads[0] = r3 & 3;
            uint mult5Idx = 0;
            for (int i = 0; i < 4; i++)
            {
                r3 = IOUtil.ReadU32LE(Data, Offset);
                Offset += 4;
                uint r5 = IOUtil.ReadU32LE(Data, Offset);
                Offset += 4;
                for (int j = 0; j < 10; j++)
                {
                    _tableSamples[i][j] = r3 >> 29;
                    _tableSamples[i][10 + j] = r5 >> 29;
                    r3 <<= 3;
                    r5 <<= 3;
                }
                _tableSamples[i][20] = (r5 >> 31) | ((r3 >> 30) << 1);
                r5 <<= 1;
                mult5Idx = (mult5Idx << 1) | (r5 >> 31);
            }
            _multipliers[5] = Multiplier5Table[mult5Idx];
        }

        private void DecodeSubFrame(short[] dst, int dstOffset, uint[] tableSamples, uint table, uint startPad)
        {
            for (int i = 0; i < startPad; i++)
                dst[dstOffset++] = 0;
            for (int i = 0; i < 20; i++)
            {
                dst[dstOffset++] = (short)SampleValueTable[table][tableSamples[i]];
                dst[dstOffset++] = 0;
                dst[dstOffset++] = 0;
            }
            dst[dstOffset++] = (short)SampleValueTable[table][tableSamples[20]];
            for (int i = 0; i < 3 - startPad; i++)
                dst[dstOffset++] = 0;
        }

        private static readonly int[][] SampleValueTable =
        {
            new[] {-28, -20, -12, -4, 4, 12, 20, 28},
            new[] {-56, -40, -24, -8, 8, 24, 40, 56},
            new[] {-84, -60, -36, -12, 12, 36, 60, 84},
            new[] {-112, -80, -48, -16, 16, 48, 80, 112},
            new[] {-140, -100, -60, -20, 20, 60, 100, 140},
            new[] {-168, -120, -72, -24, 24, 72, 120, 168},
            new[] {-196, -140, -84, -28, 28, 84, 140, 196},
            new[] {-224, -160, -96, -32, 32, 96, 160, 224},
            new[] {-252, -180, -108, -36, 36, 108, 180, 252},
            new[] {-280, -200, -120, -40, 40, 120, 200, 280},
            new[] {-308, -220, -132, -44, 44, 132, 220, 308},
            new[] {-336, -240, -144, -48, 48, 144, 240, 336},
            new[] {-364, -260, -156, -52, 52, 156, 260, 364},
            new[] {-392, -280, -168, -56, 56, 168, 280, 392},
            new[] {-420, -300, -180, -60, 60, 180, 300, 420},
            new[] {-448, -320, -192, -64, 64, 192, 320, 448},
            new[] {-504, -360, -216, -72, 72, 216, 360, 504},
            new[] {-560, -400, -240, -80, 80, 240, 400, 560},
            new[] {-616, -440, -264, -88, 88, 264, 440, 616},
            new[] {-672, -480, -288, -96, 96, 288, 480, 672},
            new[] {-728, -520, -312, -104, 104, 312, 520, 728},
            new[] {-784, -560, -336, -112, 112, 336, 560, 784},
            new[] {-840, -600, -360, -120, 120, 360, 600, 840},
            new[] {-896, -640, -384, -128, 128, 384, 640, 896},
            new[] {-1008, -720, -432, -144, 144, 432, 720, 1008},
            new[] {-1120, -800, -480, -160, 160, 480, 800, 1120},
            new[] {-1232, -880, -528, -176, 176, 528, 880, 1232},
            new[] {-1344, -960, -576, -192, 192, 576, 960, 1344},
            new[] {-1456, -1040, -624, -208, 208, 624, 1040, 1456},
            new[] {-1568, -1120, -672, -224, 224, 672, 1120, 1568},
            new[] {-1680, -1200, -720, -240, 240, 720, 1200, 1680},
            new[] {-1792, -1280, -768, -256, 256, 768, 1280, 1792},
            new[] {-2016, -1440, -864, -288, 288, 864, 1440, 2016},
            new[] {-2240, -1600, -960, -320, 320, 960, 1600, 2240},
            new[] {-2464, -1760, -1056, -352, 352, 1056, 1760, 2464},
            new[] {-2688, -1920, -1152, -384, 384, 1152, 1920, 2688},
            new[] {-2912, -2080, -1248, -416, 416, 1248, 2080, 2912},
            new[] {-3136, -2240, -1344, -448, 448, 1344, 2240, 3136},
            new[] {-3360, -2400, -1440, -480, 480, 1440, 2400, 3360},
            new[] {-3584, -2560, -1536, -512, 512, 1536, 2560, 3584},
            new[] {-4032, -2880, -1728, -576, 576, 1728, 2880, 4032},
            new[] {-4480, -3200, -1920, -640, 640, 1920, 3200, 4480},
            new[] {-4928, -3520, -2112, -704, 704, 2112, 3520, 4928},
            new[] {-5376, -3840, -2304, -768, 768, 2304, 3840, 5376},
            new[] {-5824, -4160, -2496, -832, 832, 2496, 4160, 5824},
            new[] {-6272, -4480, -2688, -896, 896, 2688, 4480, 6272},
            new[] {-6720, -4800, -2880, -960, 960, 2880, 4800, 6720},
            new[] {-7168, -5120, -3072, -1024, 1024, 3072, 5120, 7168},
            new[] {-8063, -5759, -3456, -1152, 1152, 3456, 5760, 8064},
            new[] {-8959, -6399, -3840, -1280, 1280, 3840, 6400, 8960},
            new[] {-9855, -7039, -4224, -1408, 1408, 4224, 7040, 9856},
            new[] {-10751, -7679, -4608, -1536, 1536, 4608, 7680, 10752},
            new[] {-11647, -8319, -4992, -1664, 1664, 4992, 8320, 11648},
            new[] {-12543, -8959, -5376, -1792, 1792, 5376, 8960, 12544},
            new[] {-13439, -9599, -5760, -1920, 1920, 5760, 9600, 13440},
            new[] {-14335, -10239, -6144, -2048, 2048, 6144, 10240, 14336},
            new[] {-16127, -11519, -6912, -2304, 2304, 6912, 11519, 16127},
            new[] {-17919, -12799, -7680, -2560, 2560, 7680, 12799, 17919},
            new[] {-19711, -14079, -8448, -2816, 2816, 8448, 14079, 19711},
            new[] {-21503, -15359, -9216, -3072, 3072, 9216, 15359, 21503},
            new[] {-23295, -16639, -9984, -3328, 3328, 9984, 16639, 23295},
            new[] {-25087, -17919, -10752, -3584, 3584, 10752, 17919, 25087},
            new[] {-26879, -19199, -11520, -3840, 3840, 11520, 19199, 26879},
            new[] {-28671, -20479, -12288, -4096, 4096, 12288, 20479, 28671}
        };

        private static readonly int[] Multiplier01Table =
        {
            -32665, -32460, -32256, -32051, -31846, -31641, -31436,
            -31232, -30719, -29901, -29081, -28261, -27443, -26623,
            -25805, -24985, -24165, -23347, -22527, -21300, -19660,
            -18024, -16384, -14744, -13108, -11468, -9832, -8192,
            -6552, -4916, -3276, -1640, 0, 1640, 3276, 4916,
            6552, 8192, 9832, 11468, 13108, 14744, 16384, 18024,
            19660, 21300, 22527, 23347, 24167, 24985, 25805, 26623,
            27443, 28261, 29081, 29901, 30719, 31232, 31436, 31641,
            31846, 32051, 32256, 32460
        };

        private static readonly int[] Multiplier2Table =
        {
            -27443, -26623, -25805, -24985, -24165, -23347, -22527,
            -21300, -19660, -18024, -16384, -14744, -13108, -11468,
            -9832, -8192, -6552, -4916, -3276, -1640, 0, 1640,
            3276, 4916, 6552, 8192, 9832, 11468, 13108, 14744,
            16384, 18024
        };

        private static readonly int[] Multiplier3Table =
        {
            -18024, -16384, -14744, -13108, -11468, -9832, -8192,
            -6552, -4916, -3276, -1640, 0, 1640, 3276, 4916,
            6552, 8192, 9832, 11468, 13108, 14744, 16384, 18024,
            19660, 21300, 22527, 23347, 24167, 24985, 25805, 26623,
            27443
        };

        private static readonly int[] Multiplier4Table =
        {
            -19664, -17260, -14860, -12456, -10052, -7648, -5248,
            -2844, -440, 1960, 4364, 6768, 9172, 11572, 13976,
            16380
        };

        private static readonly int[] Multiplier5Table =
        {
            -9832, -7644, -5460, -3276, -1092, 1092, 3276,
            5460, 7644, 9832, 12016, 14200, 16384, 18568, 20752,
            22527
        };

        private static readonly int[] Multiplier6Table =
        {
            -13108, -9176, -5244, -1312, 2620, 6552, 10484,
            14412
        };

        private static readonly int[] Multiplier7Table =
        {
            -6556, -2844, 872, 4584, 8296, 12012, 15724, 19436
        };
    }
}