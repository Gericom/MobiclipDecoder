using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibMobiclip.Utils;

namespace LibMobiclip.Containers.Moflex
{
    public class MoLiveStreamVideo : MoLiveStreamCodec
    {
        public MoLiveStreamVideo() { }

        public MoLiveStreamVideo(uint FpsRate, uint FpsScale, uint Width, uint Height, uint PelRatioRate, uint PelRatioScale)
        {
            Id = 1;
            Size = 12;
            this.FpsRate = FpsRate;
            this.FpsScale = FpsScale;
            this.Width = Width;
            this.Height = Height;
            this.PelRatioRate = PelRatioRate;
            this.PelRatioScale = PelRatioScale;
        }

        public uint FpsRate;
        public uint FpsScale;
        public uint Width;
        public uint Height;
        public uint PelRatioRate;
        public uint PelRatioScale;

        public override int Read(byte[] Data, int Offset)
        {
            if (Data == null || Data.Length == 0) return -1;
            int offset = Offset;
            StreamIndex = Data[offset++];
            if (offset >= Data.Length) return -1;
            CodecId = Data[offset++];
            if (Data.Length - offset < 0xA) return -1;
            FpsRate = IOUtil.ReadU16BE(Data, offset);
            FpsScale = IOUtil.ReadU16BE(Data, offset + 2);
            Width = IOUtil.ReadU16BE(Data, offset + 4);
            Height = IOUtil.ReadU16BE(Data, offset + 6);
            PelRatioRate = Data[offset + 8];
            PelRatioScale = Data[offset + 9];
            offset += 0xA;
            return offset;
        }

        public override void Write(Stream Destination)
        {
            byte[] result = new byte[12];
            result[0] = (byte)StreamIndex;
            result[1] = (byte)CodecId;
            IOUtil.WriteU16BE(result, 2, (ushort)FpsRate);
            IOUtil.WriteU16BE(result, 4, (ushort)FpsScale);
            IOUtil.WriteU16BE(result, 6, (ushort)Width);
            IOUtil.WriteU16BE(result, 8, (ushort)Height);
            result[10] = (byte)PelRatioRate;
            result[11] = (byte)PelRatioScale;
            Destination.Write(result, 0, result.Length);
        }
    }
}
