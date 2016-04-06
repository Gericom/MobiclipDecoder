using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LibMobiclip.Codec.Mobiclip
{
    public class BitWriter
    {
        private List<byte> Result = new List<byte>();

        private uint Bits = 0;
        private int BitCount = 0;

        public void WriteBits(uint Value, int NrBits)
        {
            if (NrBits <= 0) return;
            Bits |= (Value & ((1u << NrBits) - 1)) << ((32 - NrBits) - BitCount);
            BitCount += NrBits;
            if (BitCount >= 16) Flush();
        }

        //Elias gamma coding
        public void WriteVarIntUnsigned(uint Value)
        {
            int NrBits = 32 - CLZ((Value + 1) / 2);
            WriteBits(0, NrBits);
            WriteBits(1, 1);//stop bit
            Value -= ((1u << NrBits) - 1);
            WriteBits(Value, NrBits);
        }

        public void WriteVarIntSigned(int Value)
        {
            uint val;
            if (Value <= 0) val = (uint)(1 - (Value * 2));
            else val = (uint)(Value * 2);
            int NrBits = 32 - CLZ(val / 2);
            WriteBits(0, NrBits);
            WriteBits(1, 1);//stop bit
            val -= (1u << NrBits);
            WriteBits(val, NrBits);
        }

        private static int CLZ(uint value)
        {
            int leadingZeros = 0;
            while (value != 0)
            {
                value = value >> 1;
                leadingZeros++;
            }

            return (32 - leadingZeros);
        }

        public void Flush()
        {
            if (BitCount <= 0) return;
            Result.Add((byte)((Bits >> 16) & 0xFF));
            Result.Add((byte)((Bits >> 24) & 0xFF));
            BitCount -= 16;
            Bits <<= 16;
        }

        public byte[] PeekStream()
        {
            if (BitCount <= 0) return Result.ToArray();
            List<byte> res = new List<byte>();
            res.AddRange(Result);
            res.Add((byte)((Bits >> 16) & 0xFF));
            res.Add((byte)((Bits >> 24) & 0xFF));
            return res.ToArray();
        }

        public byte[] ToArray()
        {
            Flush();
            return Result.ToArray();
        }

        public static int GetNrBitsRequiredVarIntUnsigned(uint Value)
        {
            int result = 0;
            int NrBits = 32 - CLZ((Value + 1) / 2);
            result += NrBits;
            result++;//stop bit
            Value -= ((1u << NrBits) - 1);
            result += NrBits;
            return result;
        }

        public static int GetNrBitsRequiredVarIntSigned(int Value)
        {
            int result = 0;
            uint val;
            if (Value <= 0) val = (uint)(1 - (Value * 2));
            else val = (uint)(Value * 2);
            int NrBits = 32 - CLZ(val / 2);
            result += NrBits;
            result++;//stop bit
            val -= (1u << NrBits);
            result += NrBits;
            return result;
        }
    }
}
