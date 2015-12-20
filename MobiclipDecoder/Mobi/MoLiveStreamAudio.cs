using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobiclipDecoder.IO;

namespace MobiclipDecoder.Mobi
{
	public class MoLiveStreamAudio : MoLiveStreamCodec
	{
		public uint Frequency;
		public uint Channel;

        public override int Read(byte[] Data, int Offset)
        {
            if (Data == null || Data.Length == 0) return -1;
            int offset = Offset;
            StreamIndex = Data[offset++];
            if (offset >= Data.Length) return -1;
            CodecId = Data[offset++];
            if (Data.Length - offset < 0x4) return -1;
            Frequency = IOUtil.ReadU24BE(Data, offset) + 1;
            Channel = (uint)(Data[offset + 3] + 1);
            offset += 4;
            return offset;
        }
	}
}
