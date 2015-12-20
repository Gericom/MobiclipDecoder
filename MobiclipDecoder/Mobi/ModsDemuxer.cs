using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MobiclipDecoder.IO;

namespace MobiclipDecoder.Mobi
{
    public class ModsDemuxer
    {
        private Stream Stream;

        public ModsDemuxer(Stream Stream)
        {
            this.Stream = Stream;
            Header = new ModsHeader(Stream);
            //Skip some value to get at the start of the first frame
            Stream.Position += 4;
        }

        public ModsHeader Header { get; private set; }
        public class ModsHeader
        {
            public ModsHeader(Stream Stream)
            {
                byte[] Data = new byte[0x30];
                Stream.Read(Data, 0, 0x30);
                ModsString = Encoding.ASCII.GetString(Data, 0, 4);
                TagId = IOUtil.ReadU16LE(Data, 4);
                TagIdSizeDword = IOUtil.ReadU16LE(Data, 6);
                FrameCount = IOUtil.ReadU32LE(Data, 8);
                Width = IOUtil.ReadU32LE(Data, 0xC);
                Height = IOUtil.ReadU32LE(Data, 0x10);
                Fps = IOUtil.ReadU32LE(Data, 0x14);
                AudioCodec = IOUtil.ReadU16LE(Data, 0x18);
                NbChannel = IOUtil.ReadU16LE(Data, 0x1A);
                Frequency = IOUtil.ReadU32LE(Data, 0x1C);
                BiggestFrame = IOUtil.ReadU32LE(Data, 0x20);
                AudioOffset = IOUtil.ReadU32LE(Data, 0x24);
                KeyframeIndexOffset = IOUtil.ReadU32LE(Data, 0x28);
                KeyframeCount = IOUtil.ReadU32LE(Data, 0x2C);
            }
            public String ModsString;
            public UInt16 TagId;
            public UInt16 TagIdSizeDword;
            public UInt32 FrameCount;
            public UInt32 Width;
            public UInt32 Height;
            public UInt32 Fps;
            public UInt16 AudioCodec;
            public UInt16 NbChannel;
            public UInt32 Frequency;
            public UInt32 BiggestFrame;
            public UInt32 AudioOffset;
            public UInt32 KeyframeIndexOffset;
            public UInt32 KeyframeCount;
        }

        public byte[] ReadFrame(out uint NrAudioPackets)
        {
            byte[] tmp = new byte[4];
            Stream.Read(tmp, 0, 4);
            uint PacketInfo = IOUtil.ReadU32LE(tmp, 0);
            uint PacketSize = PacketInfo >> 14;
            NrAudioPackets = PacketInfo & 0x3FFF;
            byte[] completepacket = new byte[PacketSize];
            Stream.Read(completepacket, 0, (int)PacketSize);
            return completepacket;
        }
    }
}
