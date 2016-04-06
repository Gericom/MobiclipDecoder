using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMobiclip.Utils;
using System.Runtime.InteropServices;

namespace LibMobiclip.Codec.Majesco
{
    //Implementation of the Majesco compression algorithm, described by patent US7353233
    public unsafe class MajescoInflater
    {
        private const int Literals = 256 + 32;
        private const int Distances = 32;
        private const int CodeMaxBits = 15;
        private const int PrimaryTableBits = 8;

        private static readonly ushort[] mDistanceAndBytesToCopy =
        { 
             0x0001,  0, // Distance code 0 
                 0,  0, // Bytes to copy code 0 
                         // (escape code, not used) 
             0x0002,  0, // Distance code 1 
                3, 0, // Bytes to copy code 1 
             0x0003,  0, // Distance code 2 
                4, 0, // Bytes to copy code 2 
             0x0004,  0, // Distance code 3 
                5, 0, // Bytes to copy code 3 
             0x0005,  1, // Distance code 4 
                6, 0, // Bytes to copy code 4 
             0x0007,  1, // Distance code 5 
                7, 0, // Bytes to copy code 5 
             0x0009,  2, // Distance code 6 
                8, 0, // Bytes to copy code 6 
             0x000d,  2, // Distance code 7 
                9, 0, // Bytes to copy code 7 
             0x0011,  3, // Distance code 8 
                10, 0, // Bytes to copy code 8 
             0x0019,  3, // Distance code 9 
                11, 1, // Bytes to copy code 9 
             0x0021,  4, // Distance code 10 
                13, 1, // Bytes to copy code 10 
             0x0031,  4, // Distance code 11 
                15, 1, // Bytes to copy code 11 
             0x0041,  5, // Distance code 12 
                17, 1, // Bytes to copy code 12 
             0x0061,  5, // Distance code 13 
                19, 2, // Bytes to copy code 13 
             0x0081,  6, // Distance code 14 
                23, 2, // Bytes to copy code 14 
             0x00c1,  6, // Distance code 15 
                27, 2, // Bytes to copy code 15 
             0x0101,  7, // Distance code 16 
                31, 2, // Bytes to copy code 16 
             0x0181,  7, // Distance code 17 
                35, 3, // Bytes to copy code 17 
             0x0201,  8, // Distance code 18 
                43, 3, // Bytes to copy code 18 
             0x0301,  8, // Distance code 19 
                51, 3, // Bytes to copy code 19 
             0x0401,  9, // Distance code 20 
                59, 3, // Bytes to copy code 20 
             0x0601,  9, // Distance code 21 
                67, 4, // Bytes to copy code 21 
             0x0801, 10, // Distance code 22 
                83, 4, // Bytes to copy code 22 
             0x0c01, 10, // Distance code 23 
                99, 4, // Bytes to copy code 23 
             0x1001, 11, // Distance code 24 
               115, 4, // Bytes to copy code 24 
             0x1801, 11, // Distance code 25 
               131, 5, // Bytes to copy code 25 
             0x2001, 12, // Distance code 26 
               163, 5, // Bytes to copy code 26 
             0x3001, 12, // Distance code 27 
               195, 5, // Bytes to copy code 27 
             0x4001, 13, // Distance code 28 
               227, 5, // Bytes to copy code 28 
             0x6001, 13, // Distance code 29 
               258,  0, // Bytes to copy code 29 
        };

        private static readonly byte[] unk_3002DA0 =
        { 
            0x10, 0x11, 0x12, 0, 8, 7, 9, 6, 0xA, 5, 0xB, 4, 0xC, 3, 0xD, 2, 0xE, 1, 0xF, 0
        };

        private struct HuffmanTable
        {
            public HuffmanTableNode* pRoot;
            public uint SmallestLength;
            public uint LargestLength;
        }

        private struct HuffmanTableNode
        {
            public ushort Value;
            public byte Length;
        }

        private byte[] mData;
        private int mDataOffset;

        private uint mBits;
        private int mNrBitsLeft;

        private uint mOutputSize;
        private byte[] mOutputData;
        private int mOutputDataOffset;

        private int field_38;
        private int field_3C;

        private MajescoInflater(byte[] Data, int Offset)
        {
            mData = Data;
            mDataOffset = Offset;
            mOutputSize = IOUtil.ReadU32LE(Data, mDataOffset);
            mDataOffset += 4;
            mOutputData = new byte[mOutputSize];
            mOutputDataOffset = 0;
            mBits = 0;
            mNrBitsLeft = 0;
            field_38 = 0;
        }

        private unsafe void UncompressBlock()
        {
            int v4 = (int)mOutputSize - field_38;
            if (v4 <= 0) return;
            while (true)
            {
                switch (field_3C)
                {
                    case 0:
                        switch (ReadBits(2))
                        {
                            case 0: field_3C = 5; break;
                            case 1: field_3C = 2; break;
                            case 2: field_3C = 1; break;
                            case 3: field_3C = 7; break;
                        }
                        break;
                    case 1:
                        {
                            if (mNrBitsLeft < 14)
                                FillBits();
                            uint v20 = (mBits >> 27) + 256;
                            mBits <<= 5;
                            uint v21 = (mBits >> 27) + 1;
                            mBits <<= 5;
                            uint v22 = (mBits >> 28) + 4;
                            mBits <<= 4;
                            mNrBitsLeft -= 14;
                            byte* symbolsLengths = stackalloc byte[19];
                            int i = 0;
                            if (((int)mBits) >> 28 != -4)
                            {
                                do
                                {
                                    uint val = ReadBits(3);
                                    symbolsLengths[unk_3002DA0[i++]] = (byte)val;
                                } while (i < v22);
                            }
                        }
                        break;
                }
                if (v4 <= 0) break;
            }
        }

        private static unsafe uint CreateDecodeTable(HuffmanTable* pTable, uint SymbolsCount, byte* pSymbolsLengths)
        {
            // --------- Sort by length the symbol table (counter sort) 
            byte* LengthOccuranceCount = stackalloc byte[CodeMaxBits + 1];
            //memset( LengthOccuranceCount, 0, sizeof ( LengthOccuranceCount ) ); 

            // --------- CollectSymbolLengths 
            byte* pSymbolLength = pSymbolsLengths;
            for (uint Counter = SymbolsCount; Counter > 0; --Counter)
            {
                ++LengthOccuranceCount[*pSymbolLength++];
            }
            // --------- SortSymbolLengthsPositions 
            HuffmanTableNode* SymbolLengthsList = stackalloc HuffmanTableNode[Literals];

            // the symbols, sorted by length 

            HuffmanTableNode*[] pSymbolLengthPosition = new HuffmanTableNode*[CodeMaxBits + 1];

            // pointers to the first entry in to table above, 
            // for each symbol length 

            HuffmanTableNode* pLastPosition = SymbolLengthsList;

            pTable->SmallestLength = CodeMaxBits + 1; // set max value 
            pTable->LargestLength = 0;

            for (uint Counter = 1; Counter <= CodeMaxBits; ++Counter) // NOTE: starts at 1 as there're no 0-length symbols 
            {

                pSymbolLengthPosition[Counter] = pLastPosition;

                pLastPosition += LengthOccuranceCount[Counter];

                // Also find the smallest and the largest codelength 
                if (LengthOccuranceCount[Counter] != 0)
                {

                    if (Counter < pTable->SmallestLength)
                    {

                        pTable->SmallestLength = Counter;

                    }
                    if (Counter > pTable->LargestLength)
                    {

                        pTable->LargestLength = Counter;

                    }

                }

            }
            // ----------- CreateSortedSymbolList 

            uint UsedSymbolsCount = 0;

            pSymbolLength = pSymbolsLengths;

            for (uint Counter = 0; Counter < SymbolsCount; ++Counter, ++pSymbolLength)
            {
                if (*pSymbolLength != 0)
                {

                    HuffmanTableNode* pSymbolTableNode =

                    pSymbolLengthPosition[*pSymbolLength]++;

                    pSymbolTableNode->Value = (ushort)Counter;
                    pSymbolTableNode->Length = *pSymbolLength;

                }

            }

            UsedSymbolsCount = (uint)(pSymbolLengthPosition[CodeMaxBits] - SymbolLengthsList);

            // ---------------------------------------------------------- 
            // Two tables are needed to decode a symbol: 
            // - The first one will lookup the symbol's first 
            // PrimaryTableBits bits. Statistically, there are many 
            // chances that the lookup will end here. 
            // - If the symbol code is longer, a second table is needed. 
            // Several secondary tables may be created. 
            int CodeDecal = 1 << (int)(PrimaryTableBits - pTable->SmallestLength);

            HuffmanTableNode* pCurrentTableNode = pTable->pRoot;
            HuffmanTableNode* pCurrentSymbol = SymbolLengthsList;
            byte* pCurrentLengthOccurence = LengthOccuranceCount + pTable->SmallestLength;
            for (; CodeDecal != 0 && pCurrentLengthOccurence <= LengthOccuranceCount + pTable->LargestLength; CodeDecal >>= 1)
            {

                for (uint SymbolsLeft = *pCurrentLengthOccurence++; SymbolsLeft > 0; --SymbolsLeft, ++pCurrentSymbol)
                {
                    // ------------- Fill table 
                    for (uint Counter = (uint)CodeDecal; Counter > 0; --Counter)
                    {
                        *pCurrentTableNode++ = *pCurrentSymbol;
                    }
                }
            }
            if (pTable->LargestLength <= PrimaryTableBits)
            {
                return (1 << PrimaryTableBits);
            }
            else

            // -------------------------------------- Fill the secondary tables 
            {

                HuffmanTableNode* pNextTableNode = pTable->pRoot + (1 << PrimaryTableBits);
                // 

                uint RemainingBits = pTable->LargestLength - PrimaryTableBits;

                uint InitialRemainingBits = RemainingBits;

                pCurrentLengthOccurence = LengthOccuranceCount + pTable->LargestLength;

                uint CurrentSymbolCode = (1u << (int)pTable->LargestLength) - 1;

                CodeDecal = 0;
                pCurrentSymbol = SymbolLengthsList + UsedSymbolsCount - 1;

                pCurrentTableNode = pNextTableNode;

                for (; RemainingBits != 0; CurrentSymbolCode >>= 1, --RemainingBits, ++CodeDecal)
                {

                    for (uint SymbolsLeft = *pCurrentLengthOccurence--; SymbolsLeft > 0; --SymbolsLeft, --pCurrentSymbol)
                    {

                        for (uint Counter = 1u << (int)CodeDecal; Counter > 0; --Counter)
                        {

                            *pNextTableNode++ = *pCurrentSymbol;

                        }
                        uint LastSymbolCode = CurrentSymbolCode >> (int)RemainingBits;

                        --CurrentSymbolCode;
                        if (((CurrentSymbolCode >> (int)RemainingBits) ^ LastSymbolCode) != 0)// top PrimaryTableBits bits changed? 
                        {
                            --pCurrentTableNode;
                            pCurrentTableNode->Length = (byte)(32 - InitialRemainingBits);

                            // extra bits that need reading 

                            pCurrentTableNode->Value = (ushort)(pNextTableNode - pCurrentTableNode - 1);

                            // offset to first entry in the secondary table 

                            CodeDecal = SymbolsLeft == 1 ? -1 : 0;

                            InitialRemainingBits = SymbolsLeft == 1 ?

                           RemainingBits - 1 :

                            RemainingBits;

                        }

                    }

                }
                return (uint)(pNextTableNode - pTable->pRoot);
            }
        }

        private void CoreExpand_Static()
        {
            while (field_38 >= 0)
            {
                if (mNrBitsLeft < 15)
                    FillBits();
            }
        }

        private uint ReadBits(int nrBits)
        {
            if (mNrBitsLeft < nrBits)
                FillBits();
            uint result = mBits >> (32 - nrBits);
            mNrBitsLeft -= nrBits;
            mBits <<= nrBits;
            return result;
        }

        private void FillBits()
        {
            mBits |= (uint)(IOUtil.ReadU16LE(mData, mDataOffset) << (16 - mNrBitsLeft));
            mDataOffset += 2;
            mNrBitsLeft += 16;
        }

        public static byte[] Inflate(byte[] Data, int Offset)
        {
            MajescoInflater inflater = new MajescoInflater(Data, Offset);
            return null;
        }

        public static uint GetOutputSize(byte[] Data, int Offset)
        {
            return IOUtil.ReadU32LE(Data, Offset);
        }
    }
}
