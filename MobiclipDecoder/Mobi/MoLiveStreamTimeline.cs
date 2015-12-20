using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobiclipDecoder.Mobi
{
	public class MoLiveStreamTimeline : MoLiveStream
	{
		public uint AssociatedStreamIndex;

        public override int Read(byte[] Data, int Offset)
        {
            if (Data == null || Data.Length == 0) return -1;
            int offset = Offset;
            StreamIndex = Data[offset++];
            if (offset >= Data.Length) return -1;
            AssociatedStreamIndex = Data[offset++];
            return offset;
        }
	}
}
