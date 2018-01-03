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

        public uint Position;

        public int nrBitsRemaining = 0;
        public ulong buffer = 0;

        public BitReader(byte[] Data, ByteOrder ByteOrder = ByteOrder.LittleEndian, EndianSize EndianSize = EndianSize.Short)
        {
            this.ByteOrder = ByteOrder;
            this.EndianSize = EndianSize;
            this.Data = Data;
            Position = 0;

            FillBuffer();
        }

        //Elias gamme coding
        public uint ReadVarUnsignedInt()
        {
            var nrZeroes = CLZ(); //get leading zeroes
            SkipBits(nrZeroes + 1);//Remove leading zeroes and remove stop bit

            uint result = ReadBits(nrZeroes);
            result += (uint)(1 << nrZeroes);
            result--;

            return result;
        }
        /*r3 <<= r10;
            nrBitsRemaining -= r10 << 1;
            if (--nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);*/

        //Elias gamme coding
        public int ReadVarSignedInt()
        {
            int nrZeroes = CLZ();
            SkipBits(nrZeroes + 1);

            int result = (int)ReadBits(nrZeroes);
            result += 1 << nrZeroes;
            if ((result & 1) != 0) result = 1 - result;
            result >>= 1;

            return result;
        }
        /*r3 <<= r10;
            nrBitsRemaining -= r10 << 1;
            if (--nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);*/

        public int CLZ()
        {
            var intBuf = PeekBits(32);

            int leadingZeros = 0;
            while (intBuf != 0)
            {
                intBuf = intBuf >> 1;
                leadingZeros++;
            }

            return (32 - leadingZeros);
        }

        /// <summary>
        /// Reads an unsigned int. Increases the position.
        /// </summary>
        /// <returns></returns>
        /*public uint ReadVarUnsignedInt()
        {
            return ReadBits(32);
        }*/

        /// <summary>
        /// Reads a signed int. Increases the position.
        /// </summary>
        /// <returns></returns>
        /*public int ReadVarSignedInt()
        {
            return (int)ReadBits(32);
        }*/

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
            while (Count > 0)
            {
                if (nrBitsRemaining > Count)
                {
                    result |= (uint)((buffer >> (nrBitsRemaining - Count)) & (ulong)((1 << Count) - 1));

                    nrBitsRemaining -= Count;
                    Count = 0;
                }
                else
                {
                    result |= (uint)(buffer & (ulong)((1 << nrBitsRemaining) - 1)) << (Count - nrBitsRemaining);

                    Count -= nrBitsRemaining;
                    nrBitsRemaining = 0;
                    if (Position < Data.Length)
                        FillBuffer();
                    else
                    {
                        Position = (uint)Data.Length;
                        Count = 0;
                    }
                }
            }

            return result;
        }

        public uint PeekBits(int Count)
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

        public void SkipBits(int Count)
        {
            if (Count > 0)
                switch (EndianSize)
                {
                    case EndianSize.Short:
                        if (Count < nrBitsRemaining)
                        {
                            nrBitsRemaining -= Count;
                        }
                        else
                        {
                            int fullSkips = (Count - nrBitsRemaining) / 16;
                            int restBits = Count - (nrBitsRemaining + fullSkips * 16);

                            Position += (uint)fullSkips * 2;
                            if (Position < Data.Length)
                            {
                                FillBuffer();
                                nrBitsRemaining = 16 - restBits;
                            }
                            else
                            {
                                Position = (uint)Data.Length;
                                nrBitsRemaining = 0;
                            }
                        }
                        break;
                    case EndianSize.Int:
                        if (Count < nrBitsRemaining)
                        {
                            nrBitsRemaining -= Count;
                        }
                        else
                        {
                            int fullSkips = (Count - nrBitsRemaining) / 32;
                            int restBits = Count - (nrBitsRemaining + fullSkips * 32);

                            Position += (uint)fullSkips * 4;
                            if (Position < Data.Length)
                            {
                                FillBuffer();
                                nrBitsRemaining = 32 - restBits;
                            }
                            else
                            {
                                Position = (uint)Data.Length;
                                nrBitsRemaining = 0;
                            }
                        }
                        break;
                    case EndianSize.Long:
                        if (Count < nrBitsRemaining)
                        {
                            nrBitsRemaining -= Count;
                        }
                        else
                        {
                            int fullSkips = (Count - nrBitsRemaining) / 64;
                            int restBits = Count - (nrBitsRemaining + fullSkips * 64);

                            Position += (uint)fullSkips * 8;
                            if (Position < Data.Length)
                            {
                                FillBuffer();
                                nrBitsRemaining = 64 - restBits;
                            }
                            else
                            {
                                Position = (uint)Data.Length;
                                nrBitsRemaining = 0;
                            }
                        }
                        break;
                    default:
                        throw new NotSupportedException("Unknown EndianSize.");
                }
        }

        private void FillBuffer()
        {
            if (Position < Data.Length)
                switch (EndianSize)
                {
                    case EndianSize.Short:
                        buffer = 0;
                        var bytePos = Position % 8;
                        if (ByteOrder == ByteOrder.LittleEndian)
                            for (int i = 1; i >= 0; i--)
                                buffer |= (ulong)Data[bytePos + i] << i * 8;
                        else if (ByteOrder == ByteOrder.BigEndian)
                            for (int i = 0; i < 2; i++)
                                buffer |= (ulong)Data[bytePos + i] << i * 8;

                        Position += 2;
                        nrBitsRemaining += 16;
                        break;
                    case EndianSize.Int:
                        buffer = 0;
                        bytePos = Position % 8;
                        if (ByteOrder == ByteOrder.LittleEndian)
                            for (int i = 3; i >= 0; i--)
                                buffer |= (ulong)Data[bytePos + i] << i * 8;
                        else if (ByteOrder == ByteOrder.BigEndian)
                            for (int i = 0; i < 4; i++)
                                buffer |= (ulong)Data[bytePos + i] << i * 8;

                        Position += 4;
                        nrBitsRemaining += 32;
                        break;
                    case EndianSize.Long:
                        buffer = 0;
                        bytePos = Position % 8;
                        if (ByteOrder == ByteOrder.LittleEndian)
                            for (int i = 7; i >= 0; i--)
                                buffer |= (ulong)Data[bytePos + i] << i * 8;
                        else if (ByteOrder == ByteOrder.BigEndian)
                            for (int i = 0; i < 8; i++)
                                buffer |= (ulong)Data[bytePos + i] << i * 8;

                        Position += 8;
                        nrBitsRemaining += 64;
                        break;
                    default:
                        throw new NotSupportedException("Unknown EndianSize.");
                }
            else
                throw new OverflowException("You can't read past the array.");
        }
    }
}
