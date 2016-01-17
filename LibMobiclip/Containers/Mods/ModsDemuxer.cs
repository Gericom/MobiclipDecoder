using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibMobiclip.Utils;

namespace LibMobiclip.Containers.Mods
{
    public class ModsDemuxer
    {
        private Stream Stream;
        private uint CurFrame;
        private int NextKeyFrame;

        public ModsDemuxer(Stream Stream)
        {
            this.Stream = Stream;
            Header = new ModsHeader(Stream);
            if (Header.AudioOffset != 0)
            {
                AudioCodebooks = new byte[Header.NbChannel][];
                Stream.Position = Header.AudioOffset;
                for (int i = 0; i < Header.NbChannel; i++)
                {
                    AudioCodebooks[i] = new byte[0xC34];
                    Stream.Read(AudioCodebooks[i], 0, 0xC34);
                }
            }
            KeyFrames = new KeyFrameInfo[Header.KeyframeCount];
            Stream.Position = Header.KeyframeIndexOffset;
            byte[] tmp = new byte[8];
            for (int i = 0; i < Header.KeyframeCount; i++)
            {
                KeyFrames[i] = new KeyFrameInfo();
                Stream.Read(tmp, 0, 8);
                KeyFrames[i].FrameNumber = IOUtil.ReadU32LE(tmp, 0);
                KeyFrames[i].DataOffset = IOUtil.ReadU32LE(tmp, 4);
            }
            JumpToKeyFrame(0);
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
        public byte[][] AudioCodebooks;
        public KeyFrameInfo[] KeyFrames;
        public class KeyFrameInfo
        {
            public UInt32 FrameNumber;
            public UInt32 DataOffset;
        }

        private void JumpToKeyFrame(int KeyFrame)
        {
            if (KeyFrame >= Header.KeyframeCount) return;
            Stream.Position = KeyFrames[KeyFrame].DataOffset;
            CurFrame = KeyFrames[KeyFrame].FrameNumber;
            if (KeyFrame + 1 < KeyFrames.Length) NextKeyFrame = KeyFrame + 1;
            else NextKeyFrame = -1;
        }

        public byte[] ReadFrame(out uint NrAudioPackets, out bool IsKeyFrame)
        {
            NrAudioPackets = 0;
            IsKeyFrame = false;
            if (CurFrame >= Header.FrameCount) return null;
            if (NextKeyFrame >= 0 && NextKeyFrame < KeyFrames.Length && CurFrame == KeyFrames[NextKeyFrame].FrameNumber)
            {
                IsKeyFrame = true;
                if (NextKeyFrame + 1 < KeyFrames.Length) NextKeyFrame++;
                else NextKeyFrame = -1;
            }
            CurFrame++;
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
