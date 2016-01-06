using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMobiclip.Containers.Moflex
{
	public abstract class MoLiveStream : MoLiveChunk
	{
		public int StreamIndex;

		public sealed override bool IsStream() { return true; }
	}
}
