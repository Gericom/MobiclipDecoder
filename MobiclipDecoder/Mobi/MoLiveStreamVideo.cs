using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobiclipDecoder.IO;

namespace MobiclipDecoder.Mobi
{
	public class MoLiveStreamVideo : MoLiveStreamCodec
	{
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
            PelRatioRate = Data[offset + 9];
            offset += 0xA;
            return offset;
        }
	}
}
