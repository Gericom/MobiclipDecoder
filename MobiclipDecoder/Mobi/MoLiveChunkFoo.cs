using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobiclipDecoder.Mobi
{
	public class MoLiveChunkFoo : MoLiveChunk
	{
		public byte[] Bar;

        public override int Read(byte[] Data, int Offset)
        {
            throw new NotImplementedException();
        }
	}
}
