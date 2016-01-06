using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibMobiclip.Utils;

namespace LibMobiclip.Containers.Moflex
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

        public override void Write(Stream Destination)
        {
            byte[] result = new byte[6];
            result[0] = (byte)StreamIndex;
            result[1] = (byte)CodecId;
            IOUtil.WriteU24BE(result, 2, Frequency - 1);
            result[5] = (byte)(Channel - 1);
            Destination.Write(result, 0, result.Length);
        }
	}
}
