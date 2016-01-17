using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using LibMobiclip.Codec.Mobiclip;

namespace LibMobiclip.Containers.Moflex
{
    public class MoflexSimpleVideoMuxer : MoflexMuxer
    {
        public MoflexSimpleVideoMuxer(Stream Destination, Bitmap FirstFrame, int FrameRate, int FrameRateScale)
            : base(Destination)
        {
            WriteSynchroHeader();
            WriteSynchroChunk(new MoLiveStreamVideo((uint)FrameRate, (uint)FrameRateScale, (uint)FirstFrame.Width, (uint)FirstFrame.Height, 1, 1));
            WriteSynchroChunk(null);
            AddFrame(FirstFrame);
        }

        public void AddFrame(Bitmap Frame)
        {
            byte[] data = MobiclipEncoder.Encode(Frame);
            if (data.Length <= (0x1000 - 0x80))//save margin
            {
                WriteDataBlock();
                WriteEp(0, data, 0, data.Length, true);
                WriteEp(0, null, 0, 0);
            }
            else
            {
                int pos = 0;
                int left = data.Length;
                while (left >= (0x1000 - 0x80))
                {
                    WriteDataBlock();
                    WriteEp(0, data, pos, 0x1000 - 0x80, left == (0x1000 - 0x80));
                    WriteEp(0, null, 0, 0);
                    pos += 0x1000 - 0x80;
                    left -= 0x1000 - 0x80;
                }
                if (left > 0)
                {
                    WriteDataBlock();
                    WriteEp(0, data, pos, left, true);
                    WriteEp(0, null, 0, 0);
                }
            }
        }

        public void FinalizeMoflex()
        {
            mDestinationStream.Write(new byte[0x1000], 0, 0x1000);
            mDestinationStream.Flush();
        }
    }
}
