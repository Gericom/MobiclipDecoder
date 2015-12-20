using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobiclipDecoder.Mobi
{
    public class VX2DecoderData
    {
        public AsmData ArmData;
        public byte[] Buffer;
        public int MaxBufferedFrames;
        public int Index;
        public bool IsIntraFrame;
        public int Slice;
        public int Stride;
        public int CpuId;
    }
}
