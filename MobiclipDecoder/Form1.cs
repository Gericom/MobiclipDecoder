using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NAudio.Wave;
using AviFile;
using LibMobiclip.Utils;
using LibMobiclip.Containers.Moflex;
using LibMobiclip.Codec;
using LibMobiclip.Containers.Mods;

namespace MobiclipDecoder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Thread MobiThread = null;
        volatile bool StopThread = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            //tempoarly use a memorystream
            //Use a filestream later on
            /*AviManager m = new AviManager(@"d:\Projects\DS\Moflex\Test\TestVid.avi", true);
            VideoStream ss = m.GetVideoStream();
            FileStream fs = File.Create(@"d:\Projects\DS\Moflex\Test\TestVid_short2.moflex");
            int scale = 1;
            double rate = Math.Round(ss.FrameRate, 3);
            while ((rate % 1.0) != 0)
            {
                scale *= 10;
                rate *= 10;
            }
            ss.GetFrameOpen();
            MoflexSimpleVideoMuxer mux = new MoflexSimpleVideoMuxer(fs, ss.GetBitmap(0), (int)rate, scale);
            for (int i = 1; i < ss.CountFrames; i++)
            {
                mux.AddFrame(ss.GetBitmap(i));
            }
            ss.GetFrameClose();
            mux.FinalizeMoflex();
            fs.Close();
            return;*/
           /* MemoryStream m = new MemoryStream();
            MoflexMuxer mux = new MoflexMuxer(m);
            mux.WriteSynchroHeader();
            mux.WriteSynchroChunk(new MoLiveStreamVideo(25, 1, 400, 240, 1, 1));
            mux.WriteSynchroChunk(null);
            byte[] data = MobiclipEncoder.Encode(new Bitmap(new MemoryStream(File.ReadAllBytes(@"d:\Projects\DS\Moflex\TestPic3.png"))));
            if (data.Length <= (0x1000 - 0x80))//save margin
            {
                mux.WriteDataBlock();
                mux.WriteEp(0, data, 0, data.Length, true);
                mux.WriteEp(0, null, 0, 0);
            }
            else
            {
                int pos = 0;
                int left = data.Length;
                while (left >= (0x1000 - 0x80))
                {
                    mux.WriteDataBlock();
                    mux.WriteEp(0, data, pos, 0x1000 - 0x80, left == (0x1000 - 0x80));
                    mux.WriteEp(0, null, 0, 0);
                    pos += 0x1000 - 0x80;
                    left -= 0x1000 - 0x80;
                }
                if (left > 0)
                {
                    mux.WriteDataBlock();
                    mux.WriteEp(0, data, pos, left, true);
                    mux.WriteEp(0, null, 0, 0);
                }
            }
            m.Write(new byte[0x1000], 0, 0x1000);
            byte[] res = m.ToArray();
            m.Close();
            File.Create(@"d:\Projects\DS\Moflex\TestPic3.moflex").Close();
            File.WriteAllBytes(@"d:\Projects\DS\Moflex\TestPic3.moflex", res);*/

            //return;
            /*byte[] data = File.ReadAllBytes(@"d:\Temp\ev110.sfd");
            int offs = 0;//0x01D718 + 4;// 70 16 02 00; 
            while (true)
            {
                AsmData d = new AsmData(160, 120, AsmData.MobiclipVersion.Moflex3DS);
                d.Data = data;
                d.Offset = offs++;
                Bitmap b = d.MobiclipUnpack_0_0();
                if (b != null)
                {
                    //offs = d.Offset;// +4;
                }
            }*/
            /* var d = new MoLiveDemux(new MemoryStream(File.ReadAllBytes(@"d:\Old\Temp\3DS Files\MK7\Mobi\CourseSelectRace\CourseSelectRace.moflex")));
             uint res;
             while ((res = d.ReadPacket()) == 0) ;
             d.s2.Flush();
             d.s2.Close();*/
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = "All Supported Mobiclip Video Files (*.moflex;*.mods;*.dat)|*.moflex;*.mods;*.dat;*.mo|3DS Mobiclip Video Files (*.moflex)|*.moflex|DS Mobiclip Video Files (*.mods)|*.mods|Wii Mobiclip Video Files (*.dat;*.mo)|*.dat;*.mo";
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && f.FileName.Length > 0)
            {
                byte[] sig = new byte[4];
                Stream s = File.OpenRead(f.FileName);
                s.Read(sig, 0, 4);
                s.Close();
                if (sig[0] == 0x4C && sig[1] == 0x32 && sig[2] == 0xAA && sig[3] == 0xAB)
                {
                    MobiThread = new Thread((ParameterizedThreadStart)MoflexThreadMain);
                    MobiThread.Start(f.FileName);
                }
                else if (sig[0] == 0x4D && sig[1] == 0x4F && sig[2] == 0x44 && sig[3] == 0x53)
                {
                    MobiThread = new Thread((ParameterizedThreadStart)ModsThreadMain);
                    MobiThread.Start(f.FileName);
                }
                else if (sig[0] == 0x4D && sig[1] == 0x4F && sig[2] == 0x43 && sig[3] == 0x35)
                {
                    MobiThread = new Thread((ParameterizedThreadStart)MOC5ThreadMain);
                    MobiThread.Start(f.FileName);
                }
                else Application.Exit();
            }
            else Application.Exit();
        }

        private void MOC5ThreadMain(Object Args)
        {
            byte[] data = File.ReadAllBytes((String)Args);
            int offs = (int)IOUtil.ReadU32LE(data, 0x4) + 8;//(int)IOUtil.ReadU32LE(data, 0xD8);
            uint width = IOUtil.ReadU32LE(data, 0x1C);
            uint height = IOUtil.ReadU32LE(data, 0x20);
            Invoke((Action)delegate { ClientSize = new Size((int)width, (int)height); });
            double fps = IOUtil.ReadU32LE(data, 0xC) / 128d;
            TimeSpan ts = TimeSpan.FromMilliseconds(1000d / (double)(fps));
            LibMobiclip.Codec.MobiclipDecoder d = new LibMobiclip.Codec.MobiclipDecoder(width, height, LibMobiclip.Codec.MobiclipDecoder.MobiclipVersion.Moflex3DS);
            d.Data = data;
            while (!StopThread)
            {
                if (offs >= data.Length)
                {
                    Application.Exit();
                    break;
                }
                uint blocksize = IOUtil.ReadU32LE(data, offs);
                d.Offset = offs + 8;
                Bitmap b = d.MobiclipUnpack_0_0();
                if (lastval != 0)
                {
                    while ((s.Value - lastval) < (long)(ts.TotalSeconds * s.Frequency)) ;
                }
                lastval = s.Value;
                try
                {
                    pictureBox1.BeginInvoke((Action)delegate
                    {
                        pictureBox1.Image = b;
                        pictureBox1.Invalidate();
                    });
                }
                catch { }
                offs += 4 + (int)(blocksize & ~1);
                while ((offs % 4) != 0) offs++;
            }
        }

        private void ModsThreadMain(Object Args)
        {
            byte[] data = File.ReadAllBytes((String)Args);
            ModsDemuxer dm = new ModsDemuxer(new MemoryStream(data));
            /*if (dm.Header.AudioCodec == 3 && dm.Header.NbChannel > 0 && dm.Header.Frequency > 0)
            {
                AudioBuffer = new BufferedWaveProvider(new WaveFormat((int)dm.Header.Frequency, 16, dm.Header.NbChannel));
                AudioBuffer.DiscardOnBufferOverflow = true;
                AudioBuffer.BufferLength = 1024 * 512;
                Player = new WaveOut();
                Player.DesiredLatency = 150;
                Player.Init(AudioBuffer);
                Player.Play();
            }*/
            Invoke((Action)delegate { ClientSize = new Size((int)dm.Header.Width, (int)dm.Header.Height); });
            LibMobiclip.Codec.MobiclipDecoder d = new LibMobiclip.Codec.MobiclipDecoder(dm.Header.Width, dm.Header.Height, LibMobiclip.Codec.MobiclipDecoder.MobiclipVersion.ModsDS);
            TimeSpan ts = TimeSpan.FromMilliseconds(1000d / (double)(dm.Header.Fps / (double)0x01000000));
            /*int CurChannel = 0;
            List<short>[] channels = new List<short>[dm.Header.NbChannel];
            IMAADPCMDecoder[] decoders = new IMAADPCMDecoder[dm.Header.NbChannel];
            bool[] isinit = new bool[dm.Header.NbChannel];
            for (int i = 0; i < dm.Header.NbChannel; i++)
            {
                channels[i] = new List<short>();
                decoders[i] = new IMAADPCMDecoder();
                isinit[i] = false;
            }*/
            while (!StopThread)
            {
                uint NrAudioPackets;
                byte[] framedata = dm.ReadFrame(out NrAudioPackets);
                d.Data = framedata;
                d.Offset = 0;
                Bitmap b = d.MobiclipUnpack_0_0();
                /*int Offset = d.Offset + 2;
                while (Offset + NrAudioPackets * 128 > framedata.Length)
                    Offset -= 2;
                if (NrAudioPackets > 0 && AudioBuffer != null)
                {
                    for (int i = 0; i < NrAudioPackets; i++)
                    {
                        channels[CurChannel].AddRange(decoders[CurChannel].GetWaveData(framedata, Offset, 128 + (!isinit[CurChannel] ? 4 : 0)));
                        Offset += 128 + (!isinit[CurChannel] ? 4 : 0);
                        isinit[CurChannel] = true;
                        CurChannel++;
                        if (CurChannel >= dm.Header.NbChannel) CurChannel = 0;
                    }
                }
                int smallest = int.MaxValue;
                for (int i = 0; i < dm.Header.NbChannel; i++)
                {
                    if (channels[i].Count < smallest) smallest = channels[i].Count;
                }
                if (smallest > 0)
                {
                    //Gather samples
                    short[][] samps = new short[dm.Header.NbChannel][];
                    for (int i = 0; i < dm.Header.NbChannel; i++)
                    {
                        samps[i] = new short[smallest];
                        channels[i].CopyTo(0, samps[i], 0, smallest);
                        channels[i].RemoveRange(0, smallest);
                    }
                    byte[] result = InterleaveChannels(samps);
                    AudioBuffer.AddSamples(result, 0, result.Length);
                }*/
                if (lastval != 0)
                {
                    while ((s.Value - lastval) < (long)(ts.TotalSeconds * s.Frequency)) ;
                }
                lastval = s.Value;
                try
                {
                    pictureBox1.BeginInvoke((Action)delegate
                    {
                        pictureBox1.Image = b;
                        pictureBox1.Invalidate();
                    });
                }
                catch { }
            }
        }

        LibMobiclip.Codec.MobiclipDecoder ddd;
        bool left = false;

        private void MoflexThreadMain(Object Args)
        {
            FileStream s = File.OpenRead((String)Args);
            var d = new MoLiveDemux(s);//@"d:\Old\Temp\3DS Files\Moflex Audio\law_end(3d).moflex")));
            d.OnCompleteFrameReceived += new MoLiveDemux.CompleteFrameReceivedEventHandler(d_OnCompleteFrameReceived);
            bool left = false;
            while (!StopThread)
            {
                d.ReadPacket();
            }
            s.Close();
        }
        Bitmap lastleft = null;
        BufferedWaveProvider AudioBuffer = null;
        WaveOut Player = null;

        HiResTimer s = new HiResTimer();
        long lastval = 0;
        bool sizeset = false;

        void d_OnCompleteFrameReceived(MoLiveChunk Chunk, byte[] Data)
        {
            if (Chunk is MoLiveStreamVideo || Chunk is MoLiveStreamVideoWithLayout)
            {
                if (!sizeset) Invoke((Action)delegate { ClientSize = new Size((int)((MoLiveStreamVideo)Chunk).Width, (int)((MoLiveStreamVideo)Chunk).Height); });
                sizeset = true;
                left = !left;
                if (ddd == null) ddd = new LibMobiclip.Codec.MobiclipDecoder(((MoLiveStreamVideo)Chunk).Width, ((MoLiveStreamVideo)Chunk).Height, LibMobiclip.Codec.MobiclipDecoder.MobiclipVersion.Moflex3DS);
                ddd.Data = Data;
                ddd.Offset = 0;
                Bitmap b = ddd.MobiclipUnpack_0_0();
                if (!(Chunk is MoLiveStreamVideoWithLayout) || left)
                {
                    TimeSpan ts = TimeSpan.FromMilliseconds((!(Chunk is MoLiveStreamVideoWithLayout) ? 1000d : 2000d) / ((double)((MoLiveStreamVideo)Chunk).FpsRate / (double)((MoLiveStreamVideo)Chunk).FpsScale));
                    if (lastval != 0)
                    {
                        while ((s.Value - lastval) < (long)(ts.TotalSeconds * s.Frequency)) ;//milliseconds) ;
                    }
                    lastval = s.Value;
                    try
                    {
                        pictureBox1.BeginInvoke((Action)delegate
                        {
                            pictureBox1.Image = b;
                            pictureBox1.Invalidate();
                        });
                    }
                    catch { }
                }
                //while (s.ElapsedMilliseconds < 500 / (int)((MoLiveStreamVideo)Chunk).FpsRate) ;
                /*if (left) lastleft = b;
                else
                {
                    Bitmap ana = CreateAnaglyph(lastleft, b);
                    try
                    {
                        pictureBox1.BeginInvoke((Action)delegate
                        {
                            pictureBox1.Image = ana;
                            pictureBox1.Invalidate();
                        });
                    }
                    catch { }
                    while (s.ElapsedMilliseconds < 1000 / (int)((MoLiveStreamVideo)Chunk).FpsRate) ;
                }*/
                //s.Stop();
            }
            else if (Chunk is MoLiveStreamAudio && (int)((MoLiveStreamAudio)Chunk).CodecId == 1)
            {
                //  new Thread((ThreadStart)delegate
                //    {
                if (AudioBuffer == null)
                {
                    AudioBuffer = new BufferedWaveProvider(new WaveFormat((int)((MoLiveStreamAudio)Chunk).Frequency, 16, (int)((MoLiveStreamAudio)Chunk).Channel));
                    AudioBuffer.DiscardOnBufferOverflow = true;
                    AudioBuffer.BufferLength = 1024 * 512;
                    Player = new WaveOut();
                    Player.DesiredLatency = 150;
                    Player.Init(AudioBuffer);
                    Player.Play();
                }
                IMAADPCMDecoder[] decoders = new IMAADPCMDecoder[(int)((MoLiveStreamAudio)Chunk).Channel];
                List<short>[] channels = new List<short>[(int)((MoLiveStreamAudio)Chunk).Channel];
                for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                {
                    decoders[i] = new IMAADPCMDecoder();
                    decoders[i].GetWaveData(Data, 4 * i, 4);
                    channels[i] = new List<short>();
                }

                int offset = 4 * (int)((MoLiveStreamAudio)Chunk).Channel;
                int size = 128 * (int)((MoLiveStreamAudio)Chunk).Channel;
                while (offset + size < Data.Length)
                {
                    for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                    {
                        channels[i].AddRange(decoders[i].GetWaveData(Data, offset, 128));
                        offset += 128;
                    }
                }
                short[][] channelsresult = new short[(int)((MoLiveStreamAudio)Chunk).Channel][];
                for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                {
                    channelsresult[i] = channels[i].ToArray();
                }
                byte[] result = InterleaveChannels(channelsresult);
                AudioBuffer.AddSamples(result, 0, result.Length);
                // }).Start();
            }
        }

        private byte[] InterleaveChannels(params Int16[][] Channels)
        {
            if (Channels.Length == 0) return new byte[0];
            byte[] Result = new byte[Channels[0].Length * Channels.Length * 2];
            for (int i = 0; i < Channels[0].Length; i++)
            {
                for (int j = 0; j < Channels.Length; j++)
                {
                    Result[i * 2 * Channels.Length + j * 2] = (byte)(Channels[j][i] & 0xFF);
                    Result[i * 2 * Channels.Length + j * 2 + 1] = (byte)(Channels[j][i] >> 8);
                }
            }
            return Result;
        }

        private unsafe Bitmap CreateAnaglyph(Bitmap Left, Bitmap Right)
        {
            if (Left == null || Right == null) return null;
            Bitmap result = new Bitmap(Left.Width, Left.Height);
            BitmapData dres = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            BitmapData dl = Left.LockBits(new Rectangle(0, 0, Left.Width, Left.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dr = Right.LockBits(new Rectangle(0, 0, Right.Width, Right.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    Color left = Color.FromArgb(((int*)(((byte*)dl.Scan0) + y * dl.Stride + x * 4))[0]);
                    Color right = Color.FromArgb(((int*)(((byte*)dr.Scan0) + y * dr.Stride + x * 4))[0]);
                    ((int*)(((byte*)dres.Scan0) + y * dres.Stride + x * 4))[0] = Color.FromArgb(
                        left.R,
                        right.G,
                        right.B).ToArgb();
                }
            }
            Left.UnlockBits(dl);
            Right.UnlockBits(dr);
            result.UnlockBits(dres);
            return result;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MobiThread != null && MobiThread.IsAlive)
            {
                StopThread = true;
            }
            if (AudioBuffer != null)
            {
                Player.Stop();
                Player.Dispose();
                Player = null;
                AudioBuffer = null;
            }
            MobiThread = null;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            TopMost = true;
            TopMost = false;
        }
    }
}
