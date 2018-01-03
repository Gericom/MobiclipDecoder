using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMobiclip.Codec.Mobiclip
{
    public enum ByteOrder : byte
    {
        LittleEndian,
        BigEndian
    }

    public enum EndianSize : byte
    {
        Short,
        Int,
        Long
    }

    public class BitReader
    {
        ByteOrder ByteOrder;
        EndianSize EndianSize;
        byte[] Data;

        uint Position;

        int nrBitsRemaining = 0;
        ulong buffer = 0;

        public BitReader(byte[] Data, ByteOrder ByteOrder = ByteOrder.LittleEndian, EndianSize EndianSize = EndianSize.Short)
        {
            this.ByteOrder = ByteOrder;
            this.EndianSize = EndianSize;
            this.Data = Data;
            Position = 0;
        }

        /// <summary>
        /// Reads an unsigned int. Increases the position.
        /// </summary>
        /// <returns></returns>
        public uint ReadVarUnsignedInt()
        {
            return ReadBits(32);
        }

        /// <summary>
        /// Reads a signed int. Increases the position.
        /// </summary>
        /// <returns></returns>
        public int ReadVarSignedInt()
        {
            return (int)ReadBits(32);
        }

        /// <summary>
        /// Reads a single bit.
        /// </summary>
        /// <returns></returns>
        public bool ReadBit()
        {
            return Convert.ToBoolean(ReadBits(1));
        }

        /// <summary>
        /// Reads a defined amount of bits. Can't read more than 32 bits. Increases the position.
        /// </summary>
        /// <param name="Count">Amount of bits to read</param>
        /// <returns></returns>
        public uint ReadBits(int Count)
        {
            if (Count > 32)
                Count = 32;
            if (Count <= 0)
                return 0;

            uint result = 0;
            for (int i = Count - 1; i >= 0; i--)
            {
                if (nrBitsRemaining <= 0)
                    FillBuffer();

                result |= (uint)((buffer >> --nrBitsRemaining) & 1) << i;
            }

            return result;
        }

        public uint SeekBits(int Count)
        {
            var origBitsRemain = nrBitsRemaining;
            var origPosition = Position;
            var origBuffer = buffer;

            var result = ReadBits(Count);

            Position = origPosition;
            nrBitsRemaining = origBitsRemain;
            buffer = origBuffer;

            return result;
        }

        private void FillBuffer()
        {
            switch (EndianSize)
            {
                case EndianSize.Short:
                    Position += 2;
                    nrBitsRemaining = 16;
                    buffer = 0;
                    var bytePos = Position % 8;
                    if (ByteOrder == ByteOrder.LittleEndian)
                        for (int i = 1; i >= 0; i--)
                            buffer |= (ulong)Data[bytePos + i] << i * 8;
                    else if (ByteOrder == ByteOrder.BigEndian)
                        for (int i = 0; i < 2; i++)
                            buffer |= (ulong)Data[bytePos + i] << i * 8;
                    break;
                case EndianSize.Int:
                    Position += 4;
                    nrBitsRemaining = 32;
                    buffer = 0;
                    bytePos = Position % 8;
                    if (ByteOrder == ByteOrder.LittleEndian)
                        for (int i = 3; i >= 0; i--)
                            buffer |= (ulong)Data[bytePos + i] << i * 8;
                    else if (ByteOrder == ByteOrder.BigEndian)
                        for (int i = 0; i < 4; i++)
                            buffer |= (ulong)Data[bytePos + i] << i * 8;
                    break;
                case EndianSize.Long:
                    Position += 8;
                    nrBitsRemaining = 64;
                    buffer = 0;
                    bytePos = Position % 8;
                    if (ByteOrder == ByteOrder.LittleEndian)
                        for (int i = 7; i >= 0; i--)
                            buffer |= (ulong)Data[bytePos + i] << i * 8;
                    else if (ByteOrder == ByteOrder.BigEndian)
                        for (int i = 0; i < 8; i++)
                            buffer |= (ulong)Data[bytePos + i] << i * 8;
                    break;
                default:
                    throw new NotSupportedException("Unknown EndianSize.");
            }
        }
    }
}
