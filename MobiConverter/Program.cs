using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibMobiclip.Containers.Moflex;
using LibMobiclip.Codec;
using System.Drawing;
using AviFile;
using LibMobiclip.Codec.Mobiclip;
using LibMobiclip.Containers.Mods;
using LibMobiclip.Codec.Sx;
using LibMobiclip.Utils;
using LibMobiclip.Codec.FastAudio;

namespace MobiConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MobiConverter by Gericom");
            Console.WriteLine();
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }
            switch (args[0])
            {
                case "-d":
                    {
                        if (args.Length != 2 && args.Length != 3)
                            goto default;
                        if (!File.Exists(args[1]))
                        {
                            Console.WriteLine("Error! File not found: " + args[1]);
                            return;
                        }
                        String outfile = (args.Length == 3) ? args[2] : Path.ChangeExtension(args[1], "avi");
                        byte[] sig = new byte[4];
                        Stream s = File.OpenRead(args[1]);
                        s.Read(sig, 0, 4);
                        s.Close();
                        if (sig[0] == 0x4C && sig[1] == 0x32 && sig[2] == 0xAA && sig[3] == 0xAB)//moflex
                        {
                            Console.WriteLine("Moflex container detected!");
                            Console.Write("Converting: ");
                            Console.CursorVisible = false;
                            MobiclipDecoder ddd = null;
                            AviManager m = new AviManager(outfile, false);
                            MemoryStream audio = null;
                            FastAudioDecoder[] mFastAudioDecoders = null;
                            int audiorate = -1;
                            int audiochannels = 0;
                            VideoStream vs = null;
                            FileStream stream = File.OpenRead(args[1]);
                            var d = new MoLiveDemux(stream);
                            int PlayingVideoStream = -1;
                            d.OnCompleteFrameReceived += delegate(MoLiveChunk Chunk, byte[] Data)
                            {
                                if ((Chunk is MoLiveStreamVideo || Chunk is MoLiveStreamVideoWithLayout) && ((PlayingVideoStream == -1) || ((MoLiveStream)Chunk).StreamIndex == PlayingVideoStream))
                                {
                                    if (ddd == null)
                                    {
                                        ddd = new MobiclipDecoder(((MoLiveStreamVideo)Chunk).Width, ((MoLiveStreamVideo)Chunk).Height, MobiclipDecoder.MobiclipVersion.Moflex3DS);
                                        PlayingVideoStream = ((MoLiveStream)Chunk).StreamIndex;
                                    }
                                    ddd.Data = Data;
                                    ddd.Offset = 0;
                                    Bitmap b = ddd.DecodeFrame();
                                    if (vs == null) vs = m.AddVideoStream(false, Math.Round(((double)((MoLiveStreamVideo)Chunk).FpsRate) / ((double)((MoLiveStreamVideo)Chunk).FpsScale), 3), b);
                                    else vs.AddFrame(b);
                                }
                                else if (Chunk is MoLiveStreamAudio)
                                {
                                    if (audio == null)
                                    {
                                        audio = new MemoryStream();
                                        audiochannels = (int)((MoLiveStreamAudio)Chunk).Channel;
                                        audiorate = (int)((MoLiveStreamAudio)Chunk).Frequency;
                                    }
                                    switch ((int)((MoLiveStreamAudio)Chunk).CodecId)
                                    {
                                        case 0://fastaudio
                                            {
                                                if (mFastAudioDecoders == null)
                                                {
                                                    mFastAudioDecoders = new FastAudioDecoder[(int)((MoLiveStreamAudio)Chunk).Channel];
                                                    for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                                                    {
                                                        mFastAudioDecoders[i] = new FastAudioDecoder();
                                                    }
                                                }
                                                List<short>[] channels = new List<short>[(int)((MoLiveStreamAudio)Chunk).Channel];
                                                for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                                                {
                                                    channels[i] = new List<short>();
                                                }

                                                int offset = 0;
                                                int size = 40;
                                                while (offset + size < Data.Length)
                                                {
                                                    for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                                                    {
                                                        mFastAudioDecoders[i].Data = Data;
                                                        mFastAudioDecoders[i].Offset = offset;
                                                        channels[i].AddRange(mFastAudioDecoders[i].Decode());
                                                        offset = mFastAudioDecoders[i].Offset;
                                                    }
                                                }
                                                short[][] channelsresult = new short[(int)((MoLiveStreamAudio)Chunk).Channel][];
                                                for (int i = 0; i < (int)((MoLiveStreamAudio)Chunk).Channel; i++)
                                                {
                                                    channelsresult[i] = channels[i].ToArray();
                                                }
                                                byte[] result = InterleaveChannels(channelsresult);
                                                audio.Write(result, 0, result.Length);
                                            }
                                            break;
                                        case 1://IMA-ADPCM
                                            {
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
                                                audio.Write(result, 0, result.Length);
                                            }
                                            break;
                                        case 2://PCM16
                                            {
                                                audio.Write(Data, 0, Data.Length - (Data.Length % ((int)((MoLiveStreamAudio)Chunk).Channel * 2)));
                                            }
                                            break;
                                    }
                                }
                            };
                            bool left = false;
                            int counter = 0;
                            while (true)
                            {
                                uint error = d.ReadPacket();
                                if (error == 73)
                                    break;
                                //report progress
                                if (counter == 0)
                                {
                                    Console.Write("{0,3:D}%", stream.Position * 100 / stream.Length);
                                    Console.CursorLeft -= 4;
                                }
                                counter++;
                                if (counter == 50) counter = 0;
                            }
                            if (audio != null)
                            {
                                byte[] adata = audio.ToArray();
                                audio.Close();
                                var sinfo = new Avi.AVISTREAMINFO();
                                sinfo.fccType = Avi.streamtypeAUDIO;
                                sinfo.dwScale = audiochannels * 2;
                                sinfo.dwRate = audiorate * audiochannels * 2;
                                sinfo.dwSampleSize = audiochannels * 2;
                                sinfo.dwQuality = -1;
                                var sinfo2 = new Avi.PCMWAVEFORMAT();
                                sinfo2.wFormatTag = 1;
                                sinfo2.nChannels = (short)audiochannels;
                                sinfo2.nSamplesPerSec = audiorate;
                                sinfo2.nAvgBytesPerSec = audiorate * audiochannels * 2;
                                sinfo2.nBlockAlign = (short)(audiochannels * 2);
                                sinfo2.wBitsPerSample = 16;
                                unsafe
                                {
                                    fixed (byte* pAData = &adata[0])
                                    {
                                        m.AddAudioStream((IntPtr)pAData, sinfo, sinfo2, adata.Length);
                                    }
                                }
                            }
                            m.Close();
                            stream.Close();
                            Console.WriteLine("Done!");
                            Console.CursorVisible = true;
                        }
                        else if (sig[0] == 0x4D && sig[1] == 0x4F && sig[2] == 0x44 && sig[3] == 0x53)
                        {
                            //mods
                            Console.WriteLine("Mods container detected!");
                            Console.Write("Converting: ");
                            Console.CursorVisible = false;
                            AviManager m = new AviManager(outfile, false);
                            FileStream stream = File.OpenRead(args[1]);
                            ModsDemuxer dm = new ModsDemuxer(stream);
                            MemoryStream audio = null;
                            if ((dm.Header.AudioCodec == 1 || dm.Header.AudioCodec == 3) && dm.Header.NbChannel > 0 && dm.Header.Frequency > 0)
                            {
                                audio = new MemoryStream();
                            }
                            MobiclipDecoder d = new MobiclipDecoder(dm.Header.Width, dm.Header.Height, MobiclipDecoder.MobiclipVersion.ModsDS);
                            VideoStream vs = null;
                            int CurChannel = 0;
                            List<short>[] channels = new List<short>[dm.Header.NbChannel];
                            IMAADPCMDecoder[] decoders = new IMAADPCMDecoder[dm.Header.NbChannel];
                            SxDecoder[] sxd = new SxDecoder[dm.Header.NbChannel];
                            FastAudioDecoder[] fad = new FastAudioDecoder[dm.Header.NbChannel];
                            bool[] isinit = new bool[dm.Header.NbChannel];
                            for (int i = 0; i < dm.Header.NbChannel; i++)
                            {
                                channels[i] = new List<short>();
                                decoders[i] = new IMAADPCMDecoder();
                                sxd[i] = new SxDecoder();
                                fad[i] = new FastAudioDecoder();
                                isinit[i] = false;
                            }
                            int counter = 0;
                            while (true)
                            {
                                uint NrAudioPackets;
                                bool IsKeyFrame;
                                byte[] framedata = dm.ReadFrame(out NrAudioPackets, out IsKeyFrame);
                                if (framedata == null) break;
                                d.Data = framedata;
                                d.Offset = 0;
                                Bitmap b = d.DecodeFrame();
                                if (vs == null) vs = m.AddVideoStream(false, Math.Round(dm.Header.Fps / (double)0x01000000, 3), b);
                                else vs.AddFrame(b);
                                if (NrAudioPackets > 0 && audio != null)
                                {
                                    int Offset = d.Offset - 2;
                                    if (dm.Header.TagId == 0x334E && (IOUtil.ReadU16LE(framedata, 0) & 0x8000) != 0)
                                        Offset += 4;
                                    if (dm.Header.AudioCodec == 3)
                                    {
                                        if (IsKeyFrame)
                                        {
                                            for (int i = 0; i < dm.Header.NbChannel; i++)
                                            {
                                                channels[i] = new List<short>();
                                                decoders[i] = new IMAADPCMDecoder();
                                                sxd[i] = new SxDecoder();
                                                fad[i] = new FastAudioDecoder();
                                                isinit[i] = false;
                                            }
                                        }
                                        for (int i = 0; i < NrAudioPackets; i++)
                                        {
                                            channels[CurChannel].AddRange(decoders[CurChannel].GetWaveData(framedata, Offset, 128 + (!isinit[CurChannel] ? 4 : 0)));
                                            Offset += 128 + (!isinit[CurChannel] ? 4 : 0);
                                            isinit[CurChannel] = true;
                                            CurChannel++;
                                            if (CurChannel >= dm.Header.NbChannel) CurChannel = 0;
                                        }
                                    }
                                    else if (dm.Header.AudioCodec == 1)
                                    {
                                        for (int i = 0; i < NrAudioPackets; i++)
                                        {
                                            if (!isinit[CurChannel]) sxd[CurChannel].Codebook = dm.AudioCodebooks[CurChannel];
                                            isinit[CurChannel] = true;
                                            sxd[CurChannel].Data = framedata;
                                            sxd[CurChannel].Offset = Offset;
                                            channels[CurChannel].AddRange(sxd[CurChannel].Decode());
                                            Offset = sxd[CurChannel].Offset;
                                            CurChannel++;
                                            if (CurChannel >= dm.Header.NbChannel) CurChannel = 0;
                                        }
                                    }
                                    else if (dm.Header.AudioCodec == 2)
                                    {
                                        for (int i = 0; i < NrAudioPackets; i++)
                                        {
                                            fad[CurChannel].Data = framedata;
                                            fad[CurChannel].Offset = Offset;
                                            channels[CurChannel].AddRange(fad[CurChannel].Decode());
                                            Offset = fad[CurChannel].Offset;
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
                                        audio.Write(result, 0, result.Length);
                                    }
                                }
                                //report progress
                                if (counter == 0)
                                {
                                    Console.Write("{0,3:D}%", stream.Position * 100 / stream.Length);
                                    Console.CursorLeft -= 4;
                                }
                                counter++;
                                if (counter == 50) counter = 0;
                            }
                            if (audio != null)
                            {
                                byte[] adata = audio.ToArray();
                                audio.Close();
                                var sinfo = new Avi.AVISTREAMINFO();
                                sinfo.fccType = Avi.streamtypeAUDIO;
                                sinfo.dwScale = dm.Header.NbChannel * 2;
                                sinfo.dwRate = (int)dm.Header.Frequency * dm.Header.NbChannel * 2;
                                sinfo.dwSampleSize = dm.Header.NbChannel * 2;
                                sinfo.dwQuality = -1;
                                var sinfo2 = new Avi.PCMWAVEFORMAT();
                                sinfo2.wFormatTag = 1;
                                sinfo2.nChannels = (short)dm.Header.NbChannel;
                                sinfo2.nSamplesPerSec = (int)dm.Header.Frequency;
                                sinfo2.nAvgBytesPerSec = (int)dm.Header.Frequency * dm.Header.NbChannel * 2;
                                sinfo2.nBlockAlign = (short)(dm.Header.NbChannel * 2);
                                sinfo2.wBitsPerSample = 16;
                                unsafe
                                {
                                    fixed (byte* pAData = &adata[0])
                                    {
                                        m.AddAudioStream((IntPtr)pAData, sinfo, sinfo2, adata.Length);
                                    }
                                }
                            }
                            m.Close();
                            stream.Close();
                            Console.WriteLine("Done!");
                            Console.CursorVisible = true;
                            return;
                        }
                        else if (sig[0] == 0x4D && sig[1] == 0x4F && sig[2] == 0x43 && sig[3] == 0x35)
                        {
                            //moc5
                            Console.WriteLine("MOC5 container detected!");
                            Console.WriteLine("Error! Not supported yet!");
                            return;
                        }
                        else if (Path.GetExtension(args[1]).ToLower() == ".vx2")
                        {
                            //mods
                            Console.WriteLine("VX2 container detected!");
                            Console.Write("Converting: ");
                            Console.CursorVisible = false;
                            AviManager m = new AviManager(outfile, false);
                            FileStream fs = File.OpenRead(args[1]);
                            MemoryStream audio = new MemoryStream();
                            MobiclipDecoder d = new MobiclipDecoder(256, 192, MobiclipDecoder.MobiclipVersion.Moflex3DS);
                            VideoStream vs = null;
                            int framerate = 20;
                            int counter = 0;
                            int frame = 0;
                            while (true)
                            {
                                if (fs.Position >= fs.Length) break;
                                if ((frame % framerate) == 0)//Audio
                                {
                                    byte[] adata = new byte[32768 * 2];
                                    fs.Read(adata, 0, 32768 * 2);
                                    audio.Write(adata, 0, adata.Length);
                                }
                                int length = (fs.ReadByte() << 0) | (fs.ReadByte() << 8) | (fs.ReadByte() << 16) | (fs.ReadByte() << 24);
                                byte[] data = new byte[length];
                                fs.Read(data, 0, length);
                                d.Data = data;
                                d.Offset = 0;
                                Bitmap b = d.DecodeFrame();
                                if (vs == null) vs = m.AddVideoStream(false, framerate, b);
                                else vs.AddFrame(b);
                                frame++;
                                //report progress
                                if (counter == 0)
                                {
                                    Console.Write("{0,3:D}%", fs.Position * 100 / fs.Length);
                                    Console.CursorLeft -= 4;
                                }
                                counter++;
                                if (counter == 50) counter = 0;
                            }
                            if (audio != null)
                            {
                                byte[] adata = audio.ToArray();
                                audio.Close();
                                var sinfo = new Avi.AVISTREAMINFO();
                                sinfo.fccType = Avi.streamtypeAUDIO;
                                sinfo.dwScale = 1 * 2;
                                sinfo.dwRate = (int)32768 * 1 * 2;
                                sinfo.dwSampleSize = 1 * 2;
                                sinfo.dwQuality = -1;
                                var sinfo2 = new Avi.PCMWAVEFORMAT();
                                sinfo2.wFormatTag = 1;
                                sinfo2.nChannels = (short)1;
                                sinfo2.nSamplesPerSec = (int)32768;
                                sinfo2.nAvgBytesPerSec = (int)32768 * 1 * 2;
                                sinfo2.nBlockAlign = (short)(1 * 2);
                                sinfo2.wBitsPerSample = 16;
                                unsafe
                                {
                                    fixed (byte* pAData = &adata[0])
                                    {
                                        m.AddAudioStream((IntPtr)pAData, sinfo, sinfo2, adata.Length);
                                    }
                                }
                            }
                            m.Close();
                            fs.Close();
                            Console.WriteLine("Done!");
                            Console.CursorVisible = true;
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Error! Unrecognized format!");
                            return;
                        }
                        break;
                    }
                case "-e":
                    {

                        break;
                    }
                default:
                case "-h":
                    PrintUsage();
                    return;
            }
        }

        private static byte[] InterleaveChannels(params Int16[][] Channels)
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

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  MobiConverter.exe <mode> [options] [input] [output]");
            Console.WriteLine();
            Console.WriteLine("Modes:");
            Console.WriteLine("  -h            Show this help");
            Console.WriteLine("  -d            Convert Mobiclip to AVI");
            /*Console.WriteLine("  -e            Convert video to Mobiclip (still WIP!)");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("== Encoding (-e) ==");
            Console.WriteLine("  -3d           The video contains alternated left and right frames");
            Console.WriteLine("  -q <quality>  Encoding quality (12-52), lower is better, but larger file size");*/
            Console.WriteLine();
        }
    }
}
