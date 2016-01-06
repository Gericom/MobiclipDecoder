using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LibMobiclip.Containers.Moflex
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

        public override void Write(Stream Destination)
        {
            Destination.WriteByte((byte)StreamIndex);
            Destination.WriteByte((byte)AssociatedStreamIndex);
        }
	}
}
