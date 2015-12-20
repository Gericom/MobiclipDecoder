using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MobiclipDecoder.Mobi
{
	public class MoLive
	{
		public static bool ReadVariableByte(Stream src, out uint value, ref uint pos, uint psize)
		{
			value = 0;
			if (pos == psize) return false;
			byte data = (byte)src.ReadByte();
			pos++;
			if ((data & 0x80) == 0) { value = data; return true; }
			if (pos == psize) return false;
			value = (uint)(data & 0x7F) << 7;
			data = (byte)src.ReadByte();
			pos++;
			if ((data & 0x80) == 0) { value |= data; return true; }
			if (pos == psize) return false;
			value = ((uint)(data & 0x7F) | value) << 7;
			data = (byte)src.ReadByte();
			pos++;
			if ((data & 0x80) == 0) { value |= data; return true; }
			if (pos == psize) return false;
			value = (((uint)(data & 0x7F) | value) << 7) | ((byte)src.ReadByte());
			pos++;
			return true;
		}

		public static bool ReadVariableByte(byte[] src, out uint value, ref uint pos, uint psize)
		{
			value = 0;
			if (pos == psize) return false;
			byte data = src[pos++];
			if ((data & 0x80) == 0) { value = data; return true; }
			if (pos == psize) return false;
			value = (uint)(data & 0x7F) << 7;
			data = src[pos++];
			if ((data & 0x80) == 0) { value |= data; return true; }
			if (pos == psize) return false;
			value = ((uint)(data & 0x7F) | value) << 7;
			data = src[pos++];
			if ((data & 0x80) == 0) { value |= data; return true; }
			if (pos == psize) return false;
			value = (((uint)(data & 0x7F) | value) << 7) | src[pos++];
			return true;
		}
		
	}
}
