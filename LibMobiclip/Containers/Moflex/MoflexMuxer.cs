using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibMobiclip.Utils;

namespace LibMobiclip.Containers.Moflex
{
    public class MoflexMuxer
    {
        protected Stream mDestinationStream;

        private long ts = 1;

        public MoflexMuxer(Stream DestinationStream)
        {
            mDestinationStream = DestinationStream;
        }

        public void WriteSynchroHeader()
        {
            byte[] header = new byte[14];
            header[0] = 0x4C;
            header[1] = 0x32;
            //2 and 3 is checksum
            IOUtil.WriteU64BE(header, 4, (ulong)ts);
            //packetsize
            IOUtil.WriteU16BE(header, 12, 0x1000);//tempoarly. should do this better

            uint v19 = (uint)((ts >> 32) & 0xFFFFFFFF);
            if ((int)((ts >> 32) - 1) < 0) v19 &= 0x7FFFFFFF;
            ushort crc = (ushort)((((ulong)ts >> 16) & 0xFFFF) ^ (v19 >> 16) ^ 0xAAAA ^ (v19 & 0xFFFF) ^ ((ulong)ts & 0xFFFF));
            IOUtil.WriteU16BE(header, 2, crc);
            mDestinationStream.Write(header, 0, header.Length);
        }

        public void WriteSynchroChunk(MoLiveChunk Chunk)
        {
            if (Chunk == null)
            {
                MoLive.WriteVariableByte(mDestinationStream, 0);
                MoLive.WriteVariableByte(mDestinationStream, 0);
                return;
            }
            MoLive.WriteVariableByte(mDestinationStream, Chunk.Id);
            MoLive.WriteVariableByte(mDestinationStream, Chunk.Size);
            Chunk.Write(mDestinationStream);
        }

        public void WriteDataBlock()
        {
            mDestinationStream.WriteByte(1);
        }

        public void WriteEp(int Ep, byte[] Data, int Offset, int Length, bool IsEndFrame = false)
        {
            if (Data == null)
            {
                mDestinationStream.WriteByte(0);
                return;
            }
            int totalnrbits = 0;
            int nrbits = (Ep == 0 ? 1 : (int)Math.Floor(Math.Log(Ep, 2)) + 1);
            ulong val = 0x8000000000000000ul >> (nrbits - 1);
            totalnrbits += nrbits;
            val |= (ulong)Ep << ((64 - totalnrbits) - nrbits);
            totalnrbits += nrbits;
            val |= (IsEndFrame ? 1ul : 0ul) << ((64 - totalnrbits) - 1);
            totalnrbits++;
            if (IsEndFrame)
            {
                val |= 1ul << ((64 - totalnrbits) - 1);
                totalnrbits++;
                val |= 0ul << ((64 - totalnrbits) - 1);
                totalnrbits++;

                val |= 0ul << ((64 - totalnrbits) - 1);
                totalnrbits++;
                val |= 1ul << ((64 - totalnrbits) - 1);
                totalnrbits++;
                val |= 0ul << ((64 - totalnrbits) - 28);
                totalnrbits += 28;
            }
            val |= (ulong)(Length - 1) << ((64 - totalnrbits) - 13);
            totalnrbits += 13;
            int nrbytes = (totalnrbits + 4) / 8;
            //val >>= (64 - (nrbytes * 8));
            for (int i = 0; i < nrbytes; i++)
            {
                mDestinationStream.WriteByte((byte)(val >> 56));
                val <<= 8;
            }
            mDestinationStream.Write(Data, Offset, Length);
        }
    }
}
