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
                            int audiorate = -1;
                            int audiochannels = 0;
                            VideoStream vs = null;
                            FileStream stream = File.OpenRead(args[1]);
                            var d = new MoLiveDemux(stream);//@"d:\Old\Temp\3DS Files\Moflex Audio\law_end(3d).moflex")));
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
                                else if (Chunk is MoLiveStreamAudio && (int)((MoLiveStreamAudio)Chunk).CodecId == 1)
                                {
                                    if (audio == null)
                                    {
                                        audio = new MemoryStream();
                                        audiochannels = (int)((MoLiveStreamAudio)Chunk).Channel;
                                        audiorate = (int)((MoLiveStreamAudio)Chunk).Frequency;
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
                                    audio.Write(result, 0, result.Length);
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
                            bool[] isinit = new bool[dm.Header.NbChannel];
                            for (int i = 0; i < dm.Header.NbChannel; i++)
                            {
                                channels[i] = new List<short>();
                                decoders[i] = new IMAADPCMDecoder();
                                sxd[i] = new SxDecoder();
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
