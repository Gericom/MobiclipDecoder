using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LibMobiclip.Containers.Moflex
{
	public abstract class MoLiveChunk
	{
		public uint Id;
		public uint Size;

		public virtual bool IsStream() { return false; }
        public abstract int Read(byte[] Data, int Offset);
        public abstract void Write(Stream Destination);
	}
}
