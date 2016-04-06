using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMobiclip.Utils
{
    public class IOUtil
    {
        public static short ReadS16LE(byte[] Data, int Offset)
        {
            return (short)((Data[Offset + 1] << 8) | Data[Offset]);
        }

        public static short[] ReadS16sLE(byte[] Data, int Offset, int Count)
        {
            short[] result = new short[Count];
            for (int i = 0; i < Count; i++)
            {
                result[i] = ReadS16LE(Data, Offset + i * 2);
            }
            return result;
        }

        public static void WriteS16LE(byte[] Data, int Offset, short Value)
        {
            Data[Offset] = (byte)(Value & 0xFF);
            Data[Offset + 1] = (byte)((Value >> 8) & 0xFF);
        }

        public static void WriteS16sLE(byte[] Data, int Offset, short[] Values)
        {
            for (int i = 0; i < Values.Length; i++)
            {
                WriteS16LE(Data, Offset + i * 2, Values[i]);
            }
        }

        public static ushort ReadU16LE(byte[] Data, int Offset)
        {
            return (ushort)((Data[Offset + 1] << 8) | Data[Offset]);
        }

        public static void WriteU16LE(byte[] Data, int Offset, ushort Value)
        {
            Data[Offset + 0] = (byte)((Value >> 0) & 0xFF);
            Data[Offset + 1] = (byte)((Value >> 8) & 0xFF);
        }

        public static void WriteU16BE(byte[] Data, int Offset, ushort Value)
        {
            Data[Offset + 0] = (byte)((Value >> 8) & 0xFF);
            Data[Offset + 1] = (byte)((Value >> 0) & 0xFF);
        }

        public static ushort ReadU16BE(byte[] Data, int Offset)
        {
            return (ushort)((Data[Offset] << 8) | Data[Offset + 1]);
        }

        public static uint ReadU24BE(byte[] Data, int Offset)
        {
            return (uint)((Data[Offset] << 16) | (Data[Offset + 1] << 8) | Data[Offset + 2]);
        }

        public static void WriteU24BE(byte[] Data, int Offset, uint Value)
        {
            Data[Offset + 0] = (byte)((Value >> 16) & 0xFF);
            Data[Offset + 1] = (byte)((Value >> 8) & 0xFF);
            Data[Offset + 2] = (byte)((Value >> 0) & 0xFF);
        }

        public static uint ReadU32LE(byte[] Data, int Offset)
        {
            return (uint)((Data[Offset + 3] << 24) | (Data[Offset + 2] << 16) | (Data[Offset + 1] << 8) | Data[Offset]);
        }

        public static unsafe void WriteU32LE(byte[] Data, int Offset, uint Value)
        {
            Data[Offset + 0] = (byte)((Value >> 0) & 0xFF);
            Data[Offset + 1] = (byte)((Value >> 8) & 0xFF);
            Data[Offset + 2] = (byte)((Value >> 16) & 0xFF);
            Data[Offset + 3] = (byte)((Value >> 24) & 0xFF);
        }

        public static uint ReadU32BE(byte[] Data, int Offset)
        {
            return (uint)((Data[Offset] << 24) | (Data[Offset + 1] << 16) | (Data[Offset + 2] << 8) | Data[Offset + 3]);
        }

        public static void WriteU64BE(byte[] Data, int Offset, ulong Value)
        {
            Data[Offset + 0] = (byte)((Value >> 56) & 0xFF);
            Data[Offset + 1] = (byte)((Value >> 48) & 0xFF);
            Data[Offset + 2] = (byte)((Value >> 40) & 0xFF);
            Data[Offset + 3] = (byte)((Value >> 32) & 0xFF);
            Data[Offset + 4] = (byte)((Value >> 24) & 0xFF);
            Data[Offset + 5] = (byte)((Value >> 16) & 0xFF);
            Data[Offset + 6] = (byte)((Value >> 8) & 0xFF);
            Data[Offset + 7] = (byte)((Value >> 0) & 0xFF);
        }
    }
}
