using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobiclipDecoder.Mobi
{
	public abstract class MoLiveStream : MoLiveChunk
	{
		public int StreamIndex;

		public sealed override bool IsStream() { return true; }
	}
}
