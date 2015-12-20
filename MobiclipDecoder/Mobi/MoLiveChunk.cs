using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobiclipDecoder.Mobi
{
	public abstract class MoLiveChunk
	{
		public uint Id;
		public uint Size;

		public virtual bool IsStream() { return false; }
        public abstract int Read(byte[] Data, int Offset);
	}
}
