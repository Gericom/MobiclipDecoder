using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LibMobiclip.Containers.Moflex
{
	public class MoLiveChunkFoo : MoLiveChunk
	{
		public byte[] Bar;

        public override int Read(byte[] Data, int Offset)
        {
            throw new NotImplementedException();
        }

        public override void Write(Stream Destination)
        {
            throw new NotImplementedException();
        }
	}
}
