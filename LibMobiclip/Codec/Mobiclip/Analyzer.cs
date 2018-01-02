﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMobiclip.Codec.Mobiclip.Encoder;
using System.Drawing;
using LibMobiclip.Utils;

namespace LibMobiclip.Codec.Mobiclip
{
    public class Analyzer
    {
        private class BlockScore
        {
            public int BlockConfigId;
            public int Score;
            public int NrBits;

            public override string ToString()
            {
                return "BlockConfigId: " + BlockConfigId.ToString("X4") + ", Score: " + Score + ", NrBits: " + NrBits;
            }
        }

        public class InterPredict2x2Result
        {
            public Point Delta;
            public int Frame;

            public Point BlockPos;

            public override string ToString()
            {
                return "Frame: " + Frame + ", Delta: { " + Delta.X + ", " + Delta.Y + " }, BlockPos: { " + BlockPos.X + ", " + BlockPos.Y + " }";
            }
        }

        private class InterPredict2x2ResultEqualityComparer : IEqualityComparer<InterPredict2x2Result>
        {
            public static readonly InterPredict2x2ResultEqualityComparer Instance = new InterPredict2x2ResultEqualityComparer();

            public bool Equals(InterPredict2x2Result x, InterPredict2x2Result y)
            {
                return x.Frame == y.Frame && x.Delta == y.Delta;
            }

            public int GetHashCode(InterPredict2x2Result obj)
            {
                return obj.Delta.GetHashCode() ^ obj.Frame.GetHashCode();
            }
        }

        public class PBlock
        {
            public enum DivisionType
            {
                Unset,
                None,      //| |
                Horizontal,//|||
                Vertical   //|-|
            }
            public PBlock(int Width, int Height, InterPredict2x2Result[] Contents)
            {
                this.Width = Width;
                this.Height = Height;
                //this.Contents = new InterPredict2x2Result[Contents.Length];
                this.Contents = Contents;
                this.Division = DivisionType.Unset;
            }

            public int Width { get; private set; }
            public int Height { get; private set; }

            public PBlock[] ChildBlocks;

            public InterPredict2x2Result[] Contents;

            public DivisionType Division { get; set; }
            public void Partitionize(int MinSize)
            {
                /* if (Width == 4 || Height == 4)
                 {
                     /*if (Width == 4)
                     {
                         for (int y = 0; y < Height / 2; y++)
                         {
                             int dx = (Contents[y * 2].Delta.X + Contents[y * 2 + 1].Delta.X) / 2;
                             int frame = (Contents[y * 2].Frame < Contents[y * 2 + 1].Frame) ? Contents[y * 2].Frame : Contents[y * 2 + 1].Frame;
                             Contents[y * 2].Delta.X = dx;
                             Contents[y * 2].Frame = frame;
                             Contents[y * 2 + 1].Delta.X = dx;
                             Contents[y * 2 + 1].Frame = frame;
                         }
                     }
                     if (Height == 4)
                     {
                         for (int x = 0; x < Width / 2; x++)
                         {
                             int dy = (Contents[0 * 2 + x].Delta.Y + Contents[1 * 2 + x].Delta.Y) / 2;
                             int frame = (Contents[0 * 2 + x].Frame < Contents[1 * 2 + x].Frame) ? Contents[0 * 2 + x].Frame : Contents[1 * 2 + x].Frame;
                             Contents[0 * 2 + x].Delta.Y = dy;
                             Contents[0 * 2 + x].Frame = frame;
                             Contents[1 * 2 + x].Delta.Y = dy;
                             Contents[1 * 2 + x].Frame = frame;
                         }
                     }/
                     int frame = int.MaxValue;
                     int dx = 0;
                     int dy = 0;
                     foreach (InterPredict2x2Result c in Contents)
                     {
                         if (c.Frame < frame) frame = c.Frame;
                         dx += c.Delta.X;
                         dy += c.Delta.Y;
                     }
                     dx /= Contents.Length;
                     dx &= ~1;
                     dy /= Contents.Length;
                     dy &= ~1;
                     Contents[0].Frame = frame;
                     Contents[0].Delta.X = dx;
                     Contents[0].Delta.Y = dy;
                     Division = DivisionType.None;
                     return;
                 }*/
                //First count the number of unique deltas in this block
                HashSet<InterPredict2x2Result> deltas = new HashSet<InterPredict2x2Result>(Contents, InterPredict2x2ResultEqualityComparer.Instance);
                if (deltas.Count == 1)
                {
                    Division = DivisionType.None;
                    return;
                }
                if (Width <= MinSize && Height <= MinSize)
                {
                    //We should take the mean of all deltas in the block
                    int dx = 0;
                    int dy = 0;
                    int frame = 0;
                    foreach (InterPredict2x2Result r in Contents)
                    {
                        dx += r.Delta.X;
                        dy += r.Delta.Y;
                        frame += r.Frame;
                    }
                    dx = (int)Math.Round(dx / (float)Contents.Length);
                    dy = (int)Math.Round(dy / (float)Contents.Length);
                    frame = (int)Math.Round(frame / (float)Contents.Length);
                    Contents = new InterPredict2x2Result[] { new InterPredict2x2Result() { Delta = new Point(dx, dy), Frame = frame } };
                    Division = DivisionType.None;
                    return;
                }
                DivisionType best = DivisionType.Unset;
                InterPredict2x2Result[] Left = null, Right = null, Top = null, Bottom = null;
                if (Width >= MinSize * 2 && Height >= MinSize * 2)
                {
                    Left = new InterPredict2x2Result[Height / 2 * Width / 4];
                    Right = new InterPredict2x2Result[Height / 2 * Width / 4];
                    Top = new InterPredict2x2Result[Height / 4 * Width / 2];
                    Bottom = new InterPredict2x2Result[Height / 4 * Width / 2];
                    int l = 0, r = 0, t = 0, b = 0;
                    for (int y = 0; y < Height / 2; y++)
                    {
                        for (int x = 0; x < Width / 2; x++)
                        {
                            if (x < Width / 4)
                                Left[l++] = Contents[y * (Width / 2) + x];
                            else
                                Right[r++] = Contents[y * (Width / 2) + x];
                            if (y < Height / 4)
                                Top[t++] = Contents[y * (Width / 2) + x];
                            else
                                Bottom[b++] = Contents[y * (Width / 2) + x];
                        }
                    }

                    //Let's see how much clustering is possible in the different parts:
                    int leftNrClusters = GetNrClusters(Left, Width / 4, Height / 2);
                    int rightNrClusters = GetNrClusters(Right, Width / 4, Height / 2);
                    int topNrClusters = GetNrClusters(Top, Width / 2, Height / 4);
                    int bottomNrClusters = GetNrClusters(Bottom, Width / 2, Height / 4);


                    //HashSet<InterPredict2x2Result> leftdeltas = new HashSet<InterPredict2x2Result>(Left, InterPredict2x2ResultEqualityComparer.Instance);
                    //HashSet<InterPredict2x2Result> rightdeltas = new HashSet<InterPredict2x2Result>(Right, InterPredict2x2ResultEqualityComparer.Instance);
                    //HashSet<InterPredict2x2Result> topdeltas = new HashSet<InterPredict2x2Result>(Top, InterPredict2x2ResultEqualityComparer.Instance);
                    //HashSet<InterPredict2x2Result> bottomdeltas = new HashSet<InterPredict2x2Result>(Bottom, InterPredict2x2ResultEqualityComparer.Instance);
                    /*HashSet<Tuple<int, Point>> leftdeltas = new HashSet<Tuple<int, Point>>();
                    HashSet<Tuple<int, Point>> rightdeltas = new HashSet<Tuple<int, Point>>();
                    HashSet<Tuple<int, Point>> topdeltas = new HashSet<Tuple<int, Point>>();
                    HashSet<Tuple<int, Point>> bottomdeltas = new HashSet<Tuple<int, Point>>();
                    for (int y = 0; y < Height / 2; y++)
                    {
                        for (int x = 0; x < Width / 2; x++)
                        {
                            InterPredict2x2Result r = Contents[y * (Width / 2) + x];
                            if (x < Width / 4)
                            {
                                if (!leftdeltas.Contains(new Tuple<int, Point>(r.Frame, r.Delta)))
                                    leftdeltas.Add(new Tuple<int, Point>(r.Frame, r.Delta));
                            }
                            else
                            {
                                if (!rightdeltas.Contains(new Tuple<int, Point>(r.Frame, r.Delta)))
                                    rightdeltas.Add(new Tuple<int, Point>(r.Frame, r.Delta));
                            }
                            if (y < Height / 4)
                            {
                                if (!topdeltas.Contains(new Tuple<int, Point>(r.Frame, r.Delta)))
                                    topdeltas.Add(new Tuple<int, Point>(r.Frame, r.Delta));
                            }
                            else
                            {
                                if (!bottomdeltas.Contains(new Tuple<int, Point>(r.Frame, r.Delta)))
                                    bottomdeltas.Add(new Tuple<int, Point>(r.Frame, r.Delta));
                            }
                        }
                    }*/
                    if (leftNrClusters + rightNrClusters < topNrClusters + bottomNrClusters)//leftdeltas.Count + rightdeltas.Count < topdeltas.Count + bottomdeltas.Count)
                        best = DivisionType.Horizontal;
                    else best = DivisionType.Vertical;
                }
                else if (Width >= MinSize * 2)
                {
                    best = DivisionType.Horizontal;
                    Left = new InterPredict2x2Result[Height / 2 * Width / 4];
                    Right = new InterPredict2x2Result[Height / 2 * Width / 4];
                    int l = 0, r = 0;
                    for (int y = 0; y < Height / 2; y++)
                    {
                        for (int x = 0; x < Width / 2; x++)
                        {
                            if (x < Width / 4)
                                Left[l++] = Contents[y * (Width / 2) + x];
                            else
                                Right[r++] = Contents[y * (Width / 2) + x];
                        }
                    }
                }
                else
                {
                    best = DivisionType.Vertical;
                    Top = new InterPredict2x2Result[Height / 2 * Width / 4];
                    Bottom = new InterPredict2x2Result[Height / 2 * Width / 4];
                    int t = 0, b = 0;
                    for (int y = 0; y < Height / 2; y++)
                    {
                        for (int x = 0; x < Width / 2; x++)
                        {
                            if (y < Height / 4)
                                Top[t++] = Contents[y * (Width / 2) + x];
                            else
                                Bottom[b++] = Contents[y * (Width / 2) + x];
                        }
                    }
                }
                //Execute the best partition
                if (best == DivisionType.Horizontal)
                {
                    /*InterPredict2x2Result[] Left = new InterPredict2x2Result[Height / 2 * Width / 4];
                    InterPredict2x2Result[] Right = new InterPredict2x2Result[Height / 2 * Width / 4];
                    int l = 0, r = 0;
                    for (int y = 0; y < Height / 2; y++)
                    {
                        for (int x = 0; x < Width / 2; x++)
                        {
                            if (x < Width / 4)
                                Left[l++] = Contents[y * (Width / 2) + x];
                            else
                                Right[r++] = Contents[y * (Width / 2) + x];
                        }
                    }*/
                    Division = DivisionType.Horizontal;
                    ChildBlocks = new PBlock[2];
                    ChildBlocks[0] = new PBlock(Width / 2, Height, Left);
                    ChildBlocks[0].Partitionize(MinSize);
                    ChildBlocks[1] = new PBlock(Width / 2, Height, Right);
                    ChildBlocks[1].Partitionize(MinSize);
                }
                else
                {
                    /*InterPredict2x2Result[] Top = new InterPredict2x2Result[Height / 4 * Width / 2];
                     InterPredict2x2Result[] Bottom = new InterPredict2x2Result[Height / 4 * Width / 2];
                     int t = 0, b = 0;
                     for (int y = 0; y < Height / 2; y++)
                     {
                         for (int x = 0; x < Width / 2; x++)
                         {
                             if (y < Height / 4)
                                 Top[t++] = Contents[y * (Width / 2) + x];
                             else
                                 Bottom[b++] = Contents[y * (Width / 2) + x];
                         }
                     }*/
                    Division = DivisionType.Vertical;
                    ChildBlocks = new PBlock[2];
                    ChildBlocks[0] = new PBlock(Width, Height / 2, Top);
                    ChildBlocks[0].Partitionize(MinSize);
                    ChildBlocks[1] = new PBlock(Width, Height / 2, Bottom);
                    ChildBlocks[1].Partitionize(MinSize);
                }
            }

            private static int GetNrClusters(InterPredict2x2Result[] Block, int Width, int Height)
            {
                // List<InterPredict2x2Result> mClustered = new List<InterPredict2x2Result>();
                bool[] IsClustered = new bool[Block.Length];
                int NrClusters = 0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (/*mClustered.Contains(Block[y * Width + x])*/IsClustered[y * Width + x]) continue;
                        int clusterx = x;
                        int clustery = y;
                        int clusterwidth = 1;
                        int clusterheight = 1;
                        int clusterframe = Block[y * Width + x].Frame;
                        Point clusterdelta = Block[y * Width + x].Delta;
                        IsClustered[y * Width + x] = true;
                        //mClustered.Add(Block[y * Width + x]);
                        while (true)
                        {
                            //Can we expand the width?
                            bool canexpandwidth = (clusterx + clusterwidth) < Width;
                            //List<InterPredict2x2Result> possiblecluster = new List<InterPredict2x2Result>();
                            List<int> possiblecluster = new List<int>();
                            if (canexpandwidth)
                            {
                                for (int y2 = 0; y2 < clusterheight; y2++)
                                {
                                    InterPredict2x2Result rr = Block[((clustery + y2) * Width) + clusterx + clusterwidth];
                                    if (IsClustered[((clustery + y2) * Width) + clusterx + clusterwidth])//mClustered.Contains(rr))
                                    {
                                        canexpandwidth = false;
                                        break;
                                    }
                                    possiblecluster.Add(((clustery + y2) * Width) + clusterx + clusterwidth);
                                    //possiblecluster.Add(rr);
                                    if (rr.Frame != clusterframe || rr.Delta != clusterdelta)
                                    {
                                        canexpandwidth = false;
                                        break;
                                    }
                                }
                                if (canexpandwidth)
                                {
                                    clusterwidth++;
                                    //mClustered.AddRange(possiblecluster);
                                    foreach (int q in possiblecluster)
                                        IsClustered[q] = true;
                                }
                            }
                            //Can we expand the height?
                            bool canexpandheight = (clustery + clusterheight) < Height;
                            if (canexpandheight)
                            {
                                possiblecluster.Clear();
                                for (int x2 = 0; x2 < clusterwidth; x2++)
                                {
                                    InterPredict2x2Result rr = Block[((clustery + clusterheight) * Width) + clusterx + x2];
                                    if (IsClustered[((clustery + clusterheight) * Width) + clusterx + x2])//mClustered.Contains(rr))
                                    {
                                        canexpandheight = false;
                                        break;
                                    }
                                    possiblecluster.Add(((clustery + clusterheight) * Width) + clusterx + x2);//rr);
                                    if (rr.Frame != clusterframe || rr.Delta != clusterdelta)
                                    {
                                        canexpandheight = false;
                                        break;
                                    }
                                }
                                if (canexpandheight)
                                {
                                    clusterheight++;
                                    //mClustered.AddRange(possiblecluster);
                                    foreach (int q in possiblecluster)
                                        IsClustered[q] = true;
                                }
                            }
                            if (!canexpandwidth && !canexpandheight) break;
                        }
                        NrClusters++;
                    }
                }
                return NrClusters;
            }

            public byte[] GetCompvalsY(MobiEncoder Encoder, MacroBlock Block, int X, int Y)
            {
                if (Division == DivisionType.Unset) return null;
                if (Division == DivisionType.None)
                    return FrameUtil.GetPBlock(Encoder.PastFramesY[Contents[0].Frame], Contents[0].Delta.X, Contents[0].Delta.Y, (uint)Width, (uint)Height, (Block.Y + Y) * Encoder.Stride + Block.X + X, Encoder.Stride);
                byte[] result = new byte[Width * Height];
                if (Division == DivisionType.Horizontal)
                {
                    byte[] left = ChildBlocks[0].GetCompvalsY(Encoder, Block, X, Y);
                    byte[] right = ChildBlocks[1].GetCompvalsY(Encoder, Block, X + Width / 2, Y);
                    for (int y = 0; y < Height; y++)
                    {
                        Array.Copy(left, y * Width / 2, result, y * Width, Width / 2);
                        Array.Copy(right, y * Width / 2, result, y * Width + Width / 2, Width / 2);
                    }
                }
                else
                {
                    byte[] top = ChildBlocks[0].GetCompvalsY(Encoder, Block, X, Y);
                    byte[] bottom = ChildBlocks[1].GetCompvalsY(Encoder, Block, X, Y + Height / 2);
                    Array.Copy(top, result, top.Length);
                    Array.Copy(bottom, 0, result, Width * Height / 2, bottom.Length);
                }
                return result;
            }

            public byte[] GetCompvalsU(MobiEncoder Encoder, MacroBlock Block, int X, int Y)
            {
                if (Division == DivisionType.Unset) return null;
                if (Division == DivisionType.None)
                    return FrameUtil.GetPBlock(Encoder.PastFramesUV[Contents[0].Frame], Contents[0].Delta.X >> 1, Contents[0].Delta.Y >> 1, (uint)Width >> 1, (uint)Height >> 1, ((Block.Y + Y) / 2) * Encoder.Stride + ((Block.X + X) / 2), Encoder.Stride);
                byte[] result = new byte[(Width / 2) * (Height / 2)];
                if (Division == DivisionType.Horizontal)
                {
                    byte[] left = ChildBlocks[0].GetCompvalsU(Encoder, Block, X, Y);
                    byte[] right = ChildBlocks[1].GetCompvalsU(Encoder, Block, X + Width / 2, Y);
                    for (int y = 0; y < Height / 2; y++)
                    {
                        Array.Copy(left, y * Width / 4, result, y * Width / 2, Width / 4);
                        Array.Copy(right, y * Width / 4, result, y * Width / 2 + Width / 4, Width / 4);
                    }
                }
                else
                {
                    byte[] top = ChildBlocks[0].GetCompvalsU(Encoder, Block, X, Y);
                    byte[] bottom = ChildBlocks[1].GetCompvalsU(Encoder, Block, X, Y + Height / 2);
                    Array.Copy(top, result, top.Length);
                    Array.Copy(bottom, 0, result, Width / 2 * Height / 4, bottom.Length);
                }
                return result;
            }

            public byte[] GetCompvalsV(MobiEncoder Encoder, MacroBlock Block, int X, int Y)
            {
                if (Division == DivisionType.Unset) return null;
                if (Division == DivisionType.None)
                    return FrameUtil.GetPBlock(Encoder.PastFramesUV[Contents[0].Frame], Contents[0].Delta.X >> 1, Contents[0].Delta.Y >> 1, (uint)Width >> 1, (uint)Height >> 1, ((Block.Y + Y) / 2) * Encoder.Stride + ((Block.X + X) / 2) + Encoder.Stride / 2, Encoder.Stride);
                byte[] result = new byte[(Width / 2) * (Height / 2)];
                if (Division == DivisionType.Horizontal)
                {
                    byte[] left = ChildBlocks[0].GetCompvalsV(Encoder, Block, X, Y);
                    byte[] right = ChildBlocks[1].GetCompvalsV(Encoder, Block, X + Width / 2, Y);
                    for (int y = 0; y < Height / 2; y++)
                    {
                        Array.Copy(left, y * Width / 4, result, y * Width / 2, Width / 4);
                        Array.Copy(right, y * Width / 4, result, y * Width / 2 + Width / 4, Width / 4);
                    }
                }
                else
                {
                    byte[] top = ChildBlocks[0].GetCompvalsV(Encoder, Block, X, Y);
                    byte[] bottom = ChildBlocks[1].GetCompvalsV(Encoder, Block, X, Y + Height / 2);
                    Array.Copy(top, result, top.Length);
                    Array.Copy(bottom, 0, result, Width / 2 * Height / 4, bottom.Length);
                }
                return result;
            }

            private static readonly int[] SizeToIdx = new int[17]
            {
                -1, -1, 0, -1, 1, -1, -1, -1, 2, -1, -1, -1, -1, -1, -1, -1, 3
            };

            private static readonly int[, ,] HuffEncodeValTable = new int[4, 4, 10]
            {
                {
                    {0 >> 1, 6 >> 1, 5, 4, 2, 3, -1, -1, -1, -1},
                    {0 >> 2, 12 >> 2, 10 >> 1, 6 >> 1, 4 >> 1, 9, -1, -1, 8, -1},
                    {12 >> 1, 0 >> 3, 10 >> 1, 15, 14, 9, -1, -1, 8, -1},
                    {14 >> 1, 0 >> 3, 10 >> 1, 13, 9, 8, -1, -1, 12, -1}
                },
                {
                    {0 >> 2, 12 >> 2, 10 >> 1, 6 >> 1, 9, 4 >> 1, -1, -1, -1, 8},
                    {0 >> 2, 12 >> 2, 10 >> 1, 4 >> 1, 9, 8, -1, -1, 7, 6},
                    {20 >> 2, 0 >> 4, 16 >> 2, 28 >> 1, 31, 30, -1, -1, 26 >> 1, 24 >> 1},
                    {0 >> 2, 12 >> 2, 6 >> 1, 10, 5, 4, -1, -1, 11, 8 >> 1}
                },
                {
                    {12 >> 1, 0 >> 3, 10 >> 1, 15, 14, 9, -1, -1, -1, 8},
                    {20 >> 2, 0 >> 4, 16 >> 2, 28 >> 1, 31, 30, -1, -1, 26 >> 1, 24 >> 1},
                    {10 >> 1, 12 >> 2, 4 >> 1, 0 >> 1, 3, 2, -1, -1, 8 >> 1, 6 >> 1},
                    {28 >> 2, 16 >> 3, 8 >> 2, 12 >> 1, 15, 14, -1, -1, 24 >> 2, 0 >> 3}
                },
                {
                    {12 >> 1, 0 >> 3, 10 >> 1, 15, 9, 8, -1, -1, -1, 14},
                    {0 >> 2, 12 >> 2, 6 >> 1, 10, 5, 4, -1, -1, 8 >> 1, 11},
                    {20 >> 2, 24 >> 3, 0 >> 2, 16 >> 1, 19, 18, -1, -1, 8 >> 3, 4 >> 2},
                    {1, 16 >> 3, 8 >> 2, 12 >> 1, 27, 26, 14 >> 1, 24 >> 1, 0 >> 3, 28 >> 2}
                }
            };

            private static readonly int[, ,] HuffEncodeBitTable = new int[4, 4, 10]
            {
                {
                    {2, 2, 3, 3, 3, 3, -1, -1, -1, -1},
                    {2, 2, 3, 3, 3, 4, -1, -1, 4, -1},
                    {3, 1, 3, 4, 4, 4, -1, -1, 4, -1},
                    {3, 1, 3, 4, 4, 4, -1, -1, 4, -1}
                },
                {
                    {2, 2, 3, 3, 4, 3, -1, -1, -1, 4},
                    {2, 2, 3, 3, 4, 4, -1, -1, 4, 4},
                    {3, 1, 3, 4, 5, 5, -1, -1, 4, 4},
                    {2, 2, 3, 4, 4, 4, -1, -1, 4, 3}
                },
                {
                    {3, 1, 3, 4, 4, 4, -1, -1, -1, 4},
                    {3, 1, 3, 4, 5, 5, -1, -1, 4, 4},
                    {3, 2, 3, 3, 4, 4, -1, -1, 3, 3},
                    {3, 2, 3, 4, 5, 5, -1, -1, 3, 2}
                },
                {
                    {3, 1, 3, 4, 4, 4, -1, -1, -1, 4},
                    {2, 2, 3, 4, 4, 4, -1, -1, 3, 4},
                    {3, 2, 3, 4, 5, 5, -1, -1, 2, 3},
                    {1, 3, 4, 5, 6, 6, 5, 5, 3, 4}
                }
            };

            public void Encode(BitWriter b, int dXBase, int dYBase, ref Point PredictionStackEntry)
            {
                //Encoding is different for every block size, due to different huffman tables
                if (Division == DivisionType.Horizontal)
                {
                    b.WriteBits((uint)HuffEncodeValTable[SizeToIdx[Width], SizeToIdx[Height], 9], HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 9]);
                    ChildBlocks[0].Encode(b, dXBase, dYBase, ref PredictionStackEntry);
                    ChildBlocks[1].Encode(b, dXBase, dYBase, ref PredictionStackEntry);
                }
                else if (Division == DivisionType.Vertical)
                {
                    b.WriteBits((uint)HuffEncodeValTable[SizeToIdx[Width], SizeToIdx[Height], 8], HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 8]);
                    ChildBlocks[0].Encode(b, dXBase, dYBase, ref PredictionStackEntry);
                    ChildBlocks[1].Encode(b, dXBase, dYBase, ref PredictionStackEntry);
                }
                else if (Division == DivisionType.None)
                {
                    int xdiff = Contents[0].Delta.X - dXBase;
                    int ydiff = Contents[0].Delta.Y - dYBase;
                    if (Contents[0].Frame == 0 && xdiff == 0 && ydiff == 0)
                        b.WriteBits((uint)HuffEncodeValTable[SizeToIdx[Width], SizeToIdx[Height], 0], HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 0]);
                    else
                    {
                        switch (Contents[0].Frame)
                        {
                            case 0: b.WriteBits((uint)HuffEncodeValTable[SizeToIdx[Width], SizeToIdx[Height], 1], HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 1]); break;//type 1
                            case 1: b.WriteBits((uint)HuffEncodeValTable[SizeToIdx[Width], SizeToIdx[Height], 2], HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 2]); break;//type 2
                            case 2: b.WriteBits((uint)HuffEncodeValTable[SizeToIdx[Width], SizeToIdx[Height], 3], HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 3]); break;//type 3
                            case 3: b.WriteBits((uint)HuffEncodeValTable[SizeToIdx[Width], SizeToIdx[Height], 4], HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 4]); break;//type 4
                            case 4: b.WriteBits((uint)HuffEncodeValTable[SizeToIdx[Width], SizeToIdx[Height], 5], HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 5]); break;//type 5
                        }
                        b.WriteVarIntSigned(xdiff);//dx
                        b.WriteVarIntSigned(ydiff);//dy
                    }
                    PredictionStackEntry.X = Contents[0].Delta.X;
                    PredictionStackEntry.Y = Contents[0].Delta.Y;
                }
            }

            public int GetNrBitsRequired(int dXBase, int dYBase, ref Point PredictionStackEntry)
            {
                int NrBits = 0;
                if (Division == DivisionType.Horizontal)
                {
                    NrBits += HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 9];
                    NrBits += ChildBlocks[0].GetNrBitsRequired(dXBase, dYBase, ref PredictionStackEntry);
                    NrBits += ChildBlocks[1].GetNrBitsRequired(dXBase, dYBase, ref PredictionStackEntry);
                }
                else if (Division == DivisionType.Vertical)
                {
                    NrBits += HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 8];
                    NrBits += ChildBlocks[0].GetNrBitsRequired(dXBase, dYBase, ref PredictionStackEntry);
                    NrBits += ChildBlocks[1].GetNrBitsRequired(dXBase, dYBase, ref PredictionStackEntry);
                }
                else if (Division == DivisionType.None)
                {
                    int xdiff = Contents[0].Delta.X - dXBase;
                    int ydiff = Contents[0].Delta.Y - dYBase;
                    if (Contents[0].Frame == 0 && xdiff == 0 && ydiff == 0)
                        NrBits += HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 0];
                    else
                    {
                        switch (Contents[0].Frame)
                        {
                            case 0: NrBits += HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 1]; break;//type 1
                            case 1: NrBits += HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 2]; break;//type 2
                            case 2: NrBits += HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 3]; break;//type 3
                            case 3: NrBits += HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 4]; break;//type 4
                            case 4: NrBits += HuffEncodeBitTable[SizeToIdx[Width], SizeToIdx[Height], 5]; break;//type 5
                        }
                        NrBits += BitWriter.GetNrBitsRequiredVarIntSigned(xdiff);
                        NrBits += BitWriter.GetNrBitsRequiredVarIntSigned(ydiff);
                    }
                    PredictionStackEntry.X = Contents[0].Delta.X;
                    PredictionStackEntry.Y = Contents[0].Delta.Y;
                }
                return NrBits;
            }
        }

        private static unsafe InterPredict2x2Result InterPredict2x2(MobiEncoder Encoder, MacroBlock Block, int X, int Y)
        {
            //Use the Three Step Search algorithm
            byte[] cmp = Block.YData2x2[Y * 8 + X];
            InterPredict2x2Result result = new InterPredict2x2Result();
            result.BlockPos = new Point(X, Y);
            int resultscore = int.MaxValue;
            for (int i = 0; i < 5; i++)//It gives better quality at low bitrates if it's possible to refer to more frames
            {
                if (Encoder.PastFramesY[i] == null) break;
                int S = 6;
                int centerx = 0;
                int centery = 0;
                int centerscore = 0;
                while (S >= 1)
                {
                    int bestscore = int.MaxValue;
                    int bestx = 0;
                    int besty = 0;
                    for (int y = -S; y <= S; y += S)
                    {
                        if (Block.Y + y + centery + Y * 2 - 16 < 0 || Block.Y + /*2*/16 + y + centery + Y * 2 > Encoder.Height) continue;
                        for (int x = -S; x <= S; x += S)
                        {
                            if (Block.X + x + centerx + X * 2 - 16 < 0 || Block.X + /*2*/16 + x + centerx + X * 2 > Encoder.Width) continue;
                            //byte[] block = FrameUtil.GetPBlock2x2NoHalf(Encoder.PastFramesY[i], x + centerx, y + centery, (Block.Y + Y * 2) * Encoder.Stride + (Block.X + X * 2), Encoder.Stride);//FrameUtil.GetPBlock(Encoder.PastFramesY[i], (x + centerx) * 2, (y + centery) * 2, 2, 2, (Block.Y + Y * 2) * Encoder.Stride + (Block.X + X * 2), Encoder.Stride);
                            fixed (byte* pSrc = &Encoder.PastFramesY[i][(Block.Y + Y * 2) * Encoder.Stride + (Block.X + X * 2) + x + centerx + (y + centery) * Encoder.Stride])
                            {
                                int a = cmp[0] - pSrc[0];
                                if (a < 0) a = -a;
                                int b = cmp[1] - pSrc[1];
                                if (b < 0) b = -b;
                                int c = cmp[2] - pSrc[Encoder.Stride];
                                if (c < 0) c = -c;
                                int d = cmp[3] - pSrc[Encoder.Stride + 1];
                                if (d < 0) d = -d;
                                int score = a + b + c + d;
                                if (score < bestscore ||
                                    (score == bestscore &&
                                        (((x + centerx) < 0) ? -(x + centerx) : (x + centerx)) +
                                        (((y + centery) < 0) ? -(y + centery) : (y + centery)) <
                                        ((bestx < 0) ? -bestx : bestx) +
                                        ((besty < 0) ? -besty : besty)))
                                {
                                    //if (score == 0)
                                    //    return new InterPredict2x2Result() { Delta = new Point((x + centerx) * 2, (y + centery) * 2), Frame = i };
                                    bestx = x + centerx;
                                    besty = y + centery;
                                    bestscore = score;
                                }
                            }
                        }
                    }
                    S /= 2;
                    centerx = bestx;
                    centery = besty;
                    centerscore = bestscore;
                }
                if (centerscore < resultscore ||
                    (centerscore == resultscore &&
                        (((centerx * 2) < 0) ? -(centerx * 2) : (centerx * 2)) +
                        (((centery * 2) < 0) ? -(centery * 2) : (centery * 2)) <
                        ((result.Delta.X < 0) ? -result.Delta.X : result.Delta.X) +
                        ((result.Delta.Y < 0) ? -result.Delta.Y : result.Delta.Y)))
                {
                    result.Delta = new Point(centerx * 2, centery * 2);
                    result.Frame = i;
                    resultscore = centerscore;
                }
            }
            return result;
        }

        private static PBlock[] SolveInterPredictionPuzzle(MobiEncoder Encoder, MacroBlock Block)
        {
            //We're going to investigate all 2x2 blocks, and see if we can merge them into bigger blocks
            InterPredict2x2Result[] results = new InterPredict2x2Result[8 * 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    results[y * 8 + x] = InterPredict2x2(Encoder, Block, x, y);
                }
            }
            PBlock baseblock2 = new PBlock(16, 16, results);
            baseblock2.Partitionize(2);
            PBlock baseblock4 = new PBlock(16, 16, results);
            baseblock4.Partitionize(4);
            PBlock baseblock8 = new PBlock(16, 16, results);
            baseblock8.Partitionize(8);
            PBlock baseblock16 = new PBlock(16, 16, results);
            baseblock16.Partitionize(16);
            return new PBlock[] { baseblock2, baseblock4, baseblock8, baseblock16 };
        }

        private static bool[] Type8Supported =
        {
            true, true, true, true,
            true, false, true, false,
            true, true, true, false,
            true, false, true, false
        };

        private static int CalcScore8x8(byte[] Block, byte[] Cmpvls)
        {
            int[] Block2 = new int[64];
            for (int i = 0; i < 64; i++)
            {
                Block2[i] = Block[i] - Cmpvls[i];
            }
            int[] dct = MobiEncoder.DCT64(Block2);
            int score3 = 0;
            for (int i = 0; i < 64; i++)
            {
                score3 += (dct[i] < 0 ? -dct[i] : dct[i]);
            }
            return score3;
        }

        private static int CalcScore4x4(byte[] Block, byte[] Cmpvls)
        {
            int[] Block2 = new int[16];
            for (int i = 0; i < 16; i++)
            {
                Block2[i] = Block[i] - Cmpvls[i];
            }
            int[] dct = MobiEncoder.DCT16(Block2);
            int score3 = 0;
            for (int i = 0; i < 16; i++)
            {
                score3 += (dct[i] < 0 ? -dct[i] : dct[i]);
            }
            return score3;
        }

        public static PBlock[] ConfigureBlockY(MobiEncoder Encoder, MacroBlock Block, bool PFrame, PBlock[] Configs = null)
        {
            float labda = 0.85f * (float)Math.Pow(2, (Encoder.Quantizer - 12) / 3f);
            List<BlockScore> scores = new List<BlockScore>();
            PBlock[] PredictionConfigs = Configs;
            if (PFrame)//Yay! We can try to use inter prediction
            {
                if (PredictionConfigs != null)
                {
                    for (int i = 0; i < PredictionConfigs.Length; i++)
                    {
                        byte[] compvals = PredictionConfigs[i].GetCompvalsY(Encoder, Block, 0, 0);
                        int score = 0;
                        Point pse = new Point();
                        int encbits = 0;
                        int NrBits = encbits = PredictionConfigs[i].GetNrBitsRequired(0, 0, ref pse);//Use 0,0 and fake pse for now, should work fair enough
                        score += CalcScore8x8(Block.YData8x8[0], FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0));
                        MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits);//FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0));
                        score += CalcScore8x8(Block.YData8x8[1], FrameUtil.GetBlockPixels8x8(compvals, 8, 0, 16, 0));
                        MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits);
                        score += CalcScore8x8(Block.YData8x8[2], FrameUtil.GetBlockPixels8x8(compvals, 0, 8, 16, 0));
                        MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits);
                        score += CalcScore8x8(Block.YData8x8[3], FrameUtil.GetBlockPixels8x8(compvals, 8, 8, 16, 0));
                        MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits);
                        scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (i << 8), Score = score, NrBits = NrBits });
                        score = 0;
                        NrBits = encbits + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                        int b = 0;
                        for (int y = 0; y < 16; y += 8)
                        {
                            for (int x = 0; x < 16; x += 8)
                            {
                                int b2 = 0;
                                for (int y2 = 0; y2 < 8; y2 += 4)
                                {
                                    for (int x2 = 0; x2 < 8; x2 += 4)
                                    {
                                        score += CalcScore4x4(Block.YData4x4[b][b2], FrameUtil.GetBlockPixels4x4(compvals, x + x2, y + y2, 16, 0));
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, FrameUtil.GetBlockPixels4x4(compvals, x + x2, y + y2, 16, 0), Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref NrBits);
                                        b2++;
                                    }
                                }
                                b++;
                            }
                        }
                        scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (i << 8) | (1 << 5), Score = score, NrBits = NrBits });
                    }
                }
                else
                {
                    PredictionConfigs = new PBlock[5];
                    {
                        PBlock[] configs = SolveInterPredictionPuzzle(Encoder, Block);
                        for (int i = 0; i < 4; i++)
                        {
                            PredictionConfigs[i] = configs[i];
                            byte[] compvals = PredictionConfigs[i].GetCompvalsY(Encoder, Block, 0, 0);
                            int score = 0;
                            Point pse = new Point();
                            int encbits = 0;
                            int NrBits = encbits = PredictionConfigs[i].GetNrBitsRequired(0, 0, ref pse);//Use 0,0 and fake pse for now, should work fair enough
                            score += CalcScore8x8(Block.YData8x8[0], FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0));
                            MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits);//FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0));
                            score += CalcScore8x8(Block.YData8x8[1], FrameUtil.GetBlockPixels8x8(compvals, 8, 0, 16, 0));
                            MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits);
                            score += CalcScore8x8(Block.YData8x8[2], FrameUtil.GetBlockPixels8x8(compvals, 0, 8, 16, 0));
                            MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits);
                            score += CalcScore8x8(Block.YData8x8[3], FrameUtil.GetBlockPixels8x8(compvals, 8, 8, 16, 0));
                            MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits);
                            scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (i << 8), Score = score, NrBits = NrBits });
                            score = 0;
                            NrBits = encbits + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                            int b = 0;
                            for (int y = 0; y < 16; y += 8)
                            {
                                for (int x = 0; x < 16; x += 8)
                                {
                                    int b2 = 0;
                                    for (int y2 = 0; y2 < 8; y2 += 4)
                                    {
                                        for (int x2 = 0; x2 < 8; x2 += 4)
                                        {
                                            score += CalcScore4x4(Block.YData4x4[b][b2], FrameUtil.GetBlockPixels4x4(compvals, x + x2, y + y2, 16, 0));
                                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, FrameUtil.GetBlockPixels4x4(compvals, x + x2, y + y2, 16, 0), Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref NrBits);
                                            b2++;
                                        }
                                    }
                                    b++;
                                }
                            }
                            scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (i << 8) | (1 << 5), Score = score, NrBits = NrBits });
                        }
                    }
                    //simple full 16x16 block of previous frame as compvals
                    {
                        PredictionConfigs[4] = new PBlock(16, 16, new InterPredict2x2Result[] { new InterPredict2x2Result() { Delta = new Point(), Frame = 0 } });
                        PredictionConfigs[4].Division = PBlock.DivisionType.None;
                        byte[] compvals = PredictionConfigs[4].GetCompvalsY(Encoder, Block, 0, 0);
                        int score = 0;
                        Point pse = new Point();
                        int encbits = 0;
                        int NrBits = encbits = PredictionConfigs[4].GetNrBitsRequired(0, 0, ref pse);//Use 0,0 and fake pse for now, should work fair enough
                        score += CalcScore8x8(Block.YData8x8[0], FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0));
                        MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits);//FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0));
                        score += CalcScore8x8(Block.YData8x8[1], FrameUtil.GetBlockPixels8x8(compvals, 8, 0, 16, 0));
                        MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits);
                        score += CalcScore8x8(Block.YData8x8[2], FrameUtil.GetBlockPixels8x8(compvals, 0, 8, 16, 0));
                        MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits);
                        score += CalcScore8x8(Block.YData8x8[3], FrameUtil.GetBlockPixels8x8(compvals, 8, 8, 16, 0));
                        MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits);
                        scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (4 << 8), Score = score, NrBits = NrBits });
                        score = 0;
                        NrBits = encbits + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                        int b = 0;
                        for (int y = 0; y < 16; y += 8)
                        {
                            for (int x = 0; x < 16; x += 8)
                            {
                                int b2 = 0;
                                for (int y2 = 0; y2 < 8; y2 += 4)
                                {
                                    for (int x2 = 0; x2 < 8; x2 += 4)
                                    {
                                        score += CalcScore4x4(Block.YData4x4[b][b2], FrameUtil.GetBlockPixels4x4(compvals, x + x2, y + y2, 16, 0));
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, FrameUtil.GetBlockPixels4x4(compvals, x + x2, y + y2, 16, 0), Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref NrBits);
                                        b2++;
                                    }
                                }
                                b++;
                            }
                        }
                        scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (4 << 8) | (1 << 5), Score = score, NrBits = NrBits });
                    }
                }
            }
            /*for (int i = 0; i <= 7; i++)
            {
                if ((i == 0 && Block.Y < 8) || (i == 1 && Block.X < 8) || i == 2 || (i >= 4 && (Block.Y < 8 || Block.X < 8)))
                    continue;
                int score = 0;
                int NrBits = (PFrame ? 5 : 0);
                score += CalcScore8x8(Block.YData8x8[0], MacroBlock.GetCompvals8x8(i, Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, i, ref NrBits);
                score += CalcScore8x8(Block.YData8x8[1], MacroBlock.GetCompvals8x8(i, Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, i, ref NrBits);
                score += CalcScore8x8(Block.YData8x8[2], MacroBlock.GetCompvals8x8(i, Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, i, ref NrBits);
                score += CalcScore8x8(Block.YData8x8[3], MacroBlock.GetCompvals8x8(i, Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, i, ref NrBits);
                scores.Add(new BlockScore() { BlockConfigId = i, Score = score, NrBits = NrBits });
                score = 0;
                NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                int b = 0;
                for (int y = 0; y < 16; y += 8)
                {
                    for (int x = 0; x < 16; x += 8)
                    {
                        int b2 = 0;
                        for (int y2 = 0; y2 < 8; y2 += 4)
                        {
                            for (int x2 = 0; x2 < 8; x2 += 4)
                            {
                                score += CalcScore4x4(Block.YData4x4[b][b2], MacroBlock.GetCompvals4x4(10 + i, Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0));
                                MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + i, ref NrBits);
                                b2++;
                            }
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = i | (1 << 5), Score = score, NrBits = NrBits });
            }*/
            if (Block.X >= 8 && Block.Y >= 8)
            {
                byte[] pixels = Block.YData16x16;

                byte[] plane0 = MacroBlock.PredictIntraPlane16x16(Encoder.YDec, Block.Y * Encoder.Stride + Block.X, Encoder.Stride, 0);

                int[] bestps = new int[256];
                for (int i = 0; i < 256; i++)
                {
                    int x = i % 16;
                    int y = i / 16;
                    int coef = (x + 1) * 2 * (y + 1);
                    int diff = pixels[i] - plane0[i];
                    if (diff >= 0)
                        bestps[i] = (diff * 256 + (coef >> 1)) / coef;
                    else
                        bestps[i] = (diff * 256 - (coef >> 1)) / coef;
                }

                int bestp2 = 0;
                for (int y = 16 - 5; y < 16; y++)
                {
                    for (int x = 16 - 5; x < 16; x++)
                    {
                        bestp2 += bestps[x + y * 16];
                    }
                }
                bestp2 /= 25;
                if (plane0[255] + bestp2 * 2 < 0)
                    bestp2 = -((plane0[255] - 1) / 2);
                else if (plane0[255] + bestp2 * 2 > 255)
                    bestp2 = ((255 - plane0[255]) + 1) / 2;

                byte[] plane = MacroBlock.PredictIntraPlane16x16(Encoder.YDec, Block.Y * Encoder.Stride + Block.X, Encoder.Stride, bestp2);

                int score = 0;
                int NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntSigned(bestp2);
                score += CalcScore8x8(Block.YData8x8[0], FrameUtil.GetBlockPixels8x8(plane, 0, 0, 16, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits);
                score += CalcScore8x8(Block.YData8x8[1], FrameUtil.GetBlockPixels8x8(plane, 8, 0, 16, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits);
                score += CalcScore8x8(Block.YData8x8[2], FrameUtil.GetBlockPixels8x8(plane, 0, 8, 16, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits);
                score += CalcScore8x8(Block.YData8x8[3], FrameUtil.GetBlockPixels8x8(plane, 8, 8, 16, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits);
                scores.Add(new BlockScore() { BlockConfigId = 2 | (bestp2 << 8), Score = score, NrBits = NrBits });
                score = 0;
                NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntSigned(bestp2) + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                int b = 0;
                for (int y = 0; y < 16; y += 8)
                {
                    for (int x = 0; x < 16; x += 8)
                    {
                        int b2 = 0;
                        for (int y2 = 0; y2 < 8; y2 += 4)
                        {
                            for (int x2 = 0; x2 < 8; x2 += 4)
                            {
                                score += CalcScore4x4(Block.YData4x4[b][b2], FrameUtil.GetBlockPixels4x4(plane, x + x2, y + y2, 16, 0));
                                MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, FrameUtil.GetBlockPixels4x4(plane, x + x2, y + y2, 16, 0), Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref NrBits);
                                b2++;
                            }
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 2 | (bestp2 << 8) | (1 << 5), Score = score, NrBits = NrBits });
            }
            //Use 8x8 and try to find the best predictor for each block
            //If they're all the same, use the single predictor mode
            int[][] Types = new int[4][];
            bool[] Use8x8Subblock = new bool[4];
            int[][] PlaneParams = new int[4][];
            {
                int score = 0;
                int NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;// +2 * 16;
                Types[0] = new int[4];
                Types[1] = new int[4];
                Types[2] = new int[4];
                Types[3] = new int[4];
                PlaneParams[0] = new int[4];
                PlaneParams[1] = new int[4];
                PlaneParams[2] = new int[4];
                PlaneParams[3] = new int[4];
                //For every 4x4 block, find out which predictor is the best.
                int b = 0;
                for (int y = 0; y < 16; y += 8)
                {
                    for (int x = 0; x < 16; x += 8)
                    {
                        int bestscore8x8 = int.MaxValue;
                        int bestnrbits8x8 = int.MaxValue;
                        int besttype8x8 = -1;
                        int planeparam8x8 = 0;
                        //try out 8x8 prediction
                        for (int i = 0; i <= 8; i++)
                        {
                            if ((Block.Y + y == 0 && i == 0) || (Block.X + x == 0 && i == 1) || i == 2 || ((Block.X + x == 0 || Block.Y + y == 0) && i >= 4 && i <= 7) ||
                                 (i == 8 && ((x > 0 && y > 0) || (Block.Y + y == 0) || (Block.X + x + 8) >= Encoder.Width)))
                                continue;
                            int subnrbits = 0;
                            int subscore = CalcScore8x8(Block.YData8x8[b], MacroBlock.GetCompvals8x8(i, Encoder.YDec, Block.X + x, Block.Y + y, Encoder.Stride, 0));
                            MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[b], Encoder.YDec, Block.X + x, Block.Y + y, Encoder.Stride, 0, i, ref subnrbits);
                            if (subscore < bestscore8x8 || (subscore == bestscore8x8 && subnrbits < bestnrbits8x8)
                                || (b != 0 && /*Use8x8Subblock[b - 1] &&*/ subscore == bestscore8x8 && subnrbits == bestnrbits8x8 && i == Types[b - 1][0]))
                            {
                                bestscore8x8 = subscore;
                                bestnrbits8x8 = subnrbits;
                                besttype8x8 = i;
                            }
                        }
                        //byte[] cmpvls4x4 = new byte[64];
                        int total4x4score = 0;
                        int total4x4nrbits = 0;
                        int[] types4x4 = new int[4];
                        int b2 = 0;
                        for (int y2 = 0; y2 < 8; y2 += 4)
                        {
                            for (int x2 = 0; x2 < 8; x2 += 4)
                            {
                                int subscore_best = int.MaxValue;
                                int subnrbits_best = int.MaxValue;
                                int subtype_best = -1;
                                for (int i = 0; i <= 8; i++)
                                {
                                    if ((Block.X + x + x2 > 0 && Block.Y + y + y2 > 0) && i == 2)
                                    {
                                        byte[] pixels = Block.YData4x4[b][b2];
                                        byte[] plane0 = MacroBlock.PredictIntraPlane4x4(Encoder.YDec, (Block.Y + y + y2) * Encoder.Stride + (Block.X + x + x2), Encoder.Stride, 0);
                                        int[] bestps = new int[256];
                                        for (int i2 = 0; i2 < 16; i2++)
                                        {
                                            int x6 = i2 % 4;
                                            int y6 = i2 / 4;
                                            int coef = (x6 + 1) * 4 * (y6 + 1);
                                            int diff = pixels[i] - plane0[i];
                                            if (diff >= 0)
                                                bestps[i] = (diff * 32 + (coef >> 1)) / coef;
                                            else
                                                bestps[i] = (diff * 32 - (coef >> 1)) / coef;
                                        }

                                        int bestp2 = 0;
                                        for (int y6 = 4 - 2; y6 < 4; y6++)
                                        {
                                            for (int x6 = 4 - 2; x6 < 4; x6++)
                                            {
                                                bestp2 += bestps[x6 + y6 * 4];
                                            }
                                        }
                                        bestp2 /= 4;
                                        if (plane0[15] + bestp2 * 2 < 0)
                                            bestp2 = -((plane0[15] - 1) / 2);
                                        else if (plane0[15] + bestp2 * 2 > 255)
                                            bestp2 = ((255 - plane0[15]) + 1) / 2;

                                        PlaneParams[b][b2] = bestp2;

                                        byte[] plane = MacroBlock.PredictIntraPlane4x4(Encoder.YDec, (Block.Y + y + y2) * Encoder.Stride + (Block.X + x + x2), Encoder.Stride, bestp2);
                                        int subnrbits2 = BitWriter.GetNrBitsRequiredVarIntSigned(bestp2);
                                        int subscore2 = CalcScore4x4(Block.YData4x4[b][b2], plane);
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, plane, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref subnrbits2);
                                        if (subscore2 < subscore_best || (subscore2 == subscore_best && subnrbits2 < subnrbits_best)
                                            || (b2 != 0 && subscore2 == subscore_best && subnrbits2 == subnrbits_best && i == types4x4[b2 - 1]))
                                        {
                                            subscore_best = subscore2;
                                            subnrbits_best = subnrbits2;
                                            subtype_best = i;
                                        }
                                        continue;
                                    }
                                    if ((Block.Y + y + y2 == 0 && i == 0) || (Block.X + x + x2 == 0 && i == 1) || i == 2 || ((Block.X + x + x2 == 0 || Block.Y + y + y2 == 0) && i >= 4 && i <= 7) ||
                                        (i == 8 && (!Type8Supported[y + y2 + (x + x2) / 4] || (Block.Y + y + y2 == 0) || (Block.X + x + x2 + 4) >= Encoder.Width)))
                                        continue;
                                    int subnrbits = 0;
                                    int subscore = CalcScore4x4(Block.YData4x4[b][b2], MacroBlock.GetCompvals4x4(10 + i, Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0));
                                    MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + i, ref subnrbits);
                                    if (subscore < subscore_best || (subscore == subscore_best && subnrbits < subnrbits_best)
                                        || (b2 != 0 && subscore == subscore_best && subnrbits == subnrbits_best && i == types4x4[b2 - 1])
                                        || (b != 0 && b2 == 0 && subscore == subscore_best && subnrbits == subnrbits_best && i == Types[b - 1][0]))
                                    {
                                        subscore_best = subscore;
                                        subnrbits_best = subnrbits;
                                        subtype_best = i;
                                    }
                                }
                                total4x4nrbits += subnrbits_best;
                                total4x4score += subscore_best;
                                types4x4[b2] = subtype_best;
                                int tmpbits = 0;
                                if (subtype_best == 2)
                                {
                                    tmpbits += BitWriter.GetNrBitsRequiredVarIntSigned(PlaneParams[b][b2]);
                                    byte[] plane = MacroBlock.PredictIntraPlane4x4(Encoder.YDec, (Block.Y + y + y2) * Encoder.Stride + (Block.X + x + x2), Encoder.Stride, PlaneParams[b][b2]);
                                    //FrameUtil.SetBlockPixels4x4(cmpvls4x4, x2, y2, 8, 0, plane);
                                    MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, plane, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref tmpbits);
                                }
                                else
                                {
                                    //FrameUtil.SetBlockPixels4x4(cmpvls4x4, x2, y2, 8, 0, MacroBlock.GetCompvals4x4(10 + subtype_best, Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0));
                                    MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + subtype_best, ref tmpbits);
                                }
                                b2++;
                            }
                        }
                        //total4x4score = GetScore8x8(Block.YData8x8[b], cmpvls4x4);
                        if (bestscore8x8 <= total4x4score)
                        {
                            score += bestscore8x8;
                            NrBits += bestnrbits8x8;
                            Types[b][0] = besttype8x8;
                            Use8x8Subblock[b] = true;
                            PlaneParams[b][0] = planeparam8x8;
                            int tmpbits = 0;
                            if (besttype8x8 == 2)
                            {
                                tmpbits += BitWriter.GetNrBitsRequiredVarIntSigned(planeparam8x8);
                                byte[] plane = MacroBlock.PredictIntraPlane8x8(Encoder.YDec, (Block.Y + y) * Encoder.Stride + (Block.X + x), Encoder.Stride, planeparam8x8);
                                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[b], Encoder.YDec, plane, Block.X + x, Block.Y + y, Encoder.Stride, 0, ref tmpbits);
                            }
                            else
                                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[b], Encoder.YDec, Block.X + x, Block.Y + y, Encoder.Stride, 0, besttype8x8, ref tmpbits);
                            NrBits += 2;
                        }
                        else
                        {
                            score += total4x4score;
                            NrBits += total4x4nrbits;
                            Types[b] = types4x4;
                            Use8x8Subblock[b] = false;
                            NrBits += 2 * 4;
                        }
                        b++;
                    }
                }
                int allthesametype = Types[0][0];
                bool allthesame = allthesametype != 2 && allthesametype != 8;
                if (allthesame)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (!Use8x8Subblock[i])
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                if (Types[i][j] != allthesametype)
                                {
                                    allthesame = false;
                                    break;
                                }
                            }
                            if (!allthesame)
                                break;
                        }
                        else
                        {
                            if (Types[i][0] != allthesametype)
                            {
                                allthesame = false;
                                break;
                            }
                        }
                    }
                }

                if (allthesame)//Use8x8Subblock[0] && Use8x8Subblock[1] && Use8x8Subblock[2] && Use8x8Subblock[3] && Types[0][0] != 2 & Types[0][0] != 8 && Types[0][0] == Types[1][0] && Types[1][0] == Types[2][0] && Types[2][0] == Types[3][0])
                {
                    int NrBits4 = (PFrame ? 5 : 0);
                    b = 0;
                    for (int y = 0; y < 16; y += 8)
                    {
                        for (int x = 0; x < 16; x += 8)
                        {
                            if (!Use8x8Subblock[b])
                            {
                                int b2 = 0;
                                for (int y2 = 0; y2 < 8; y2 += 4)
                                {
                                    for (int x2 = 0; x2 < 8; x2 += 4)
                                    {
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + Types[0][0], ref NrBits4);
                                        b2++;
                                    }
                                }
                            }
                            else
                            {
                                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[b], Encoder.YDec, Block.X + x, Block.Y + y, Encoder.Stride, 0, Types[0][0], ref NrBits4);
                            }
                            b++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = 1 | (1 << 7), Score = score, NrBits = NrBits4 });
                }
                else
                    scores.Add(new BlockScore() { BlockConfigId = 0 | (1 << 7), Score = score, NrBits = NrBits });
            }
            BlockScore best = null;
            foreach (BlockScore s in scores)
            {
                if (
                    best == null ||
                    s.Score + labda * s.NrBits < best.Score + labda * best.NrBits)
                    //s.Score < best.Score || (s.Score == best.Score && s.NrBits < best.NrBits))
                    //s.NrBits < best.NrBits ||
                    //(s.NrBits == best.NrBits && s.Score < best.Score))
                    // s.Score < best.Score ||
                    //(s.Score == best.Score && s.NrBits < best.NrBits))
                    //(s.Score == best.Score && (s.BlockConfigId & 0x20) == 0 && (best.BlockConfigId & 0x20) != 0) ||
                    //(s.Score == best.Score && (s.BlockConfigId & 0x40) != 0 && (best.BlockConfigId & 0x40) != 0 && (s.BlockConfigId & 0x1F) < (best.BlockConfigId & 0x1F)))
                    best = s;
            }
            Block.YUseComplex8x8[0] = true;
            Block.YUseComplex8x8[1] = true;
            Block.YUseComplex8x8[2] = true;
            Block.YUseComplex8x8[3] = true;
            if (((best.BlockConfigId >> 7) & 1) == 1)
            {
                Block.YUse4x4[0] = !Use8x8Subblock[0];
                Block.YUse4x4[1] = !Use8x8Subblock[1];
                Block.YUse4x4[2] = !Use8x8Subblock[2];
                Block.YUse4x4[3] = !Use8x8Subblock[3];
                for (int i = 0; i < 4; i++)
                {
                    if (!Block.YUse4x4[i]) continue;
                    for (int j = 0; j < 4; j++)
                    {
                        Block.YUseDCT4x4[i][j] = true;
                    }
                }
                if ((best.BlockConfigId & 1) == 1)
                {
                    Block.YPredictionMode = Types[0][0];
                }
                else
                {
                    Block.UseIntraSubBlockMode = true;
                    Block.YIntraSubBlockModeTypes = Types;
                    Block.YIntraSubBlockModePlaneParams = PlaneParams;
                }
            }
            else if (((best.BlockConfigId >> 6) & 1) == 1)
            {
                Block.UseInterPrediction = true;
                Block.InterPredictionConfig = PredictionConfigs[best.BlockConfigId >> 8];
                //Block.InterPredictionFrame = best.BlockConfigId & 0x1F;
                //Block.InterPredictionDelta = new Point((int)(((best.BlockConfigId >> 8) & 0xFF) << 24) >> 24, (int)(((best.BlockConfigId >> 16) & 0xFF) << 24) >> 24);
                Block.UVUseComplex8x8[0] = true;
                Block.UVUseComplex8x8[1] = true;
                byte[] U = PredictionConfigs[best.BlockConfigId >> 8].GetCompvalsU(Encoder, Block, 0, 0);
                byte[] V = PredictionConfigs[best.BlockConfigId >> 8].GetCompvalsV(Encoder, Block, 0, 0);
                int NrBits = 0;
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, U, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, ref NrBits);
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, V, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, ref NrBits);
                best.NrBits += NrBits;
                //What's wrong with this?
                if (((best.BlockConfigId >> 5) & 1) == 1)
                {
                    Block.YUse4x4[0] = true;
                    Block.YUse4x4[1] = true;
                    Block.YUse4x4[2] = true;
                    Block.YUse4x4[3] = true;
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            Block.YUseDCT4x4[i][j] = true;
                        }
                    }
                }
            }
            else
            {
                Block.YPredictionMode = best.BlockConfigId & 0x1F;
                if (Block.YPredictionMode == 2)
                {
                    Block.YPredict16x16Arg = ((int)best.BlockConfigId >> 8);
                }
                if (((best.BlockConfigId >> 5) & 1) == 1)
                {
                    Block.YUse4x4[0] = true;
                    Block.YUse4x4[1] = true;
                    Block.YUse4x4[2] = true;
                    Block.YUse4x4[3] = true;
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            Block.YUseDCT4x4[i][j] = true;
                        }
                    }
                }
            }
            //return best.NrBits;
            return PredictionConfigs;
        }

        //This analyzer should determine the best block configuration for a macro block (mb)
        public static int ConfigureBlockY_old(MobiEncoder Encoder, MacroBlock Block, bool PFrame)
        {
            float labda = 0.85f * (float)Math.Pow(2, (Encoder.Quantizer - 12) / 3f);
            List<BlockScore> scores = new List<BlockScore>();
            PBlock[] PredictionConfigs = null;
            if (PFrame)//Yay! We can try to use inter prediction
            {
                PredictionConfigs = new PBlock[5];
                {
                    PBlock[] configs = SolveInterPredictionPuzzle(Encoder, Block);
                    for (int i = 0; i < 4; i++)
                    {
                        PredictionConfigs[i] = configs[i];
                        byte[] compvals = PredictionConfigs[i].GetCompvalsY(Encoder, Block, 0, 0);
                        int score = 0;
                        Point pse = new Point();
                        int encbits = 0;
                        int NrBits = encbits = PredictionConfigs[i].GetNrBitsRequired(0, 0, ref pse);//Use 0,0 and fake pse for now, should work fair enough
                        score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits));//FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0));
                        score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits));
                        score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                        score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                        scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (i << 8), Score = score, NrBits = NrBits });
                        score = 0;
                        NrBits = encbits + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                        int b = 0;
                        for (int y = 0; y < 16; y += 8)
                        {
                            for (int x = 0; x < 16; x += 8)
                            {
                                int b2 = 0;
                                for (int y2 = 0; y2 < 8; y2 += 4)
                                {
                                    for (int x2 = 0; x2 < 8; x2 += 4)
                                    {
                                        score += GetScore4x4(Block.YData4x4[b][b2],
                                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, FrameUtil.GetBlockPixels4x4(compvals, x + x2, y + y2, 16, 0), Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref NrBits));
                                        b2++;
                                    }
                                }
                                b++;
                            }
                        }
                        scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (i << 8) | (1 << 5), Score = score, NrBits = NrBits });
                    }
                }
                //simple full 16x16 block of previous frame as compvals
                {
                    PredictionConfigs[4] = new PBlock(16, 16, new InterPredict2x2Result[] { new InterPredict2x2Result() { Delta = new Point(), Frame = 0 } });
                    PredictionConfigs[4].Division = PBlock.DivisionType.None;
                    byte[] compvals = PredictionConfigs[4].GetCompvalsY(Encoder, Block, 0, 0);
                    int score = 0;
                    Point pse = new Point();
                    int encbits = 0;
                    int NrBits = encbits = PredictionConfigs[4].GetNrBitsRequired(0, 0, ref pse);//Use 0,0 and fake pse for now, should work fair enough
                    score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits));//FrameUtil.GetBlockPixels8x8(compvals, 0, 0, 16, 0));
                    score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(compvals, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                    scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (4 << 8), Score = score, NrBits = NrBits });
                    score = 0;
                    NrBits = encbits + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                    int b = 0;
                    for (int y = 0; y < 16; y += 8)
                    {
                        for (int x = 0; x < 16; x += 8)
                        {
                            int b2 = 0;
                            for (int y2 = 0; y2 < 8; y2 += 4)
                            {
                                for (int x2 = 0; x2 < 8; x2 += 4)
                                {
                                    score += GetScore4x4(Block.YData4x4[b][b2],
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, FrameUtil.GetBlockPixels4x4(compvals, x + x2, y + y2, 16, 0), Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref NrBits));
                                    b2++;
                                }
                            }
                            b++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = (1 << 6) | (4 << 8) | (1 << 5), Score = score, NrBits = NrBits });
                }
            }
            //8x8
            if (Block.Y >= 8)
            {
                int score = 0;
                int NrBits = (PFrame ? 5 : 0);
                score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, 0, ref NrBits));
                score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, 0, ref NrBits));
                score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, 0, ref NrBits));
                score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, 0, ref NrBits));
                scores.Add(new BlockScore() { BlockConfigId = 0, Score = score, NrBits = NrBits });
                score = 0;
                NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                int b = 0;
                for (int y = 0; y < 16; y += 8)
                {
                    for (int x = 0; x < 16; x += 8)
                    {
                        int b2 = 0;
                        for (int y2 = 0; y2 < 8; y2 += 4)
                        {
                            for (int x2 = 0; x2 < 8; x2 += 4)
                            {
                                score += GetScore4x4(Block.YData4x4[b][b2],
                                    MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + 0, ref NrBits));
                                b2++;
                            }
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 0 | (1 << 5), Score = score, NrBits = NrBits });
            }
            if (Block.X >= 8)
            {
                int score = 0;
                int NrBits = (PFrame ? 5 : 0);
                score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, 1, ref NrBits));
                score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, 1, ref NrBits));
                score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, 1, ref NrBits));
                score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, 1, ref NrBits));
                scores.Add(new BlockScore() { BlockConfigId = 1, Score = score, NrBits = NrBits });
                score = 0;
                NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                int b = 0;
                for (int y = 0; y < 16; y += 8)
                {
                    for (int x = 0; x < 16; x += 8)
                    {
                        int b2 = 0;
                        for (int y2 = 0; y2 < 8; y2 += 4)
                        {
                            for (int x2 = 0; x2 < 8; x2 += 4)
                            {
                                score += GetScore4x4(Block.YData4x4[b][b2],
                                    MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + 1, ref NrBits));
                                b2++;
                            }
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 1 | (1 << 5), Score = score, NrBits = NrBits });
            }
            //Block type 3
            {
                int score = 0;
                int NrBits = (PFrame ? 5 : 0);
                score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, 3, ref NrBits));
                score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, 3, ref NrBits));
                score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, 3, ref NrBits));
                score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, 3, ref NrBits));
                scores.Add(new BlockScore() { BlockConfigId = 3, Score = score, NrBits = NrBits });
                score = 0;
                NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                int b = 0;
                for (int y = 0; y < 16; y += 8)
                {
                    for (int x = 0; x < 16; x += 8)
                    {
                        int b2 = 0;
                        for (int y2 = 0; y2 < 8; y2 += 4)
                        {
                            for (int x2 = 0; x2 < 8; x2 += 4)
                            {
                                score += GetScore4x4(Block.YData4x4[b][b2],
                                    MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + 3, ref NrBits));
                                b2++;
                            }
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 3 | (1 << 5), Score = score, NrBits = NrBits });
            }
            if (Block.X >= 8 && Block.Y >= 8)
            {
                //Try type 2 with param 0 for now
                {
                    /*byte[] plane = MacroBlock.PredictIntraPlane16x16(Encoder.YDec, Block.Y * Encoder.Stride + Block.X, Encoder.Stride, 0);

                    int score = 0;
                    int NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntSigned(0);
                    score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                    scores.Add(new BlockScore() { BlockConfigId = 2 | (0 << 8), Score = score, NrBits = NrBits });
                    score = 0;
                    NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntSigned(0) + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                    int b = 0;
                    for (int y = 0; y < 16; y += 8)
                    {
                        for (int x = 0; x < 16; x += 8)
                        {
                            int b2 = 0;
                            for (int y2 = 0; y2 < 8; y2 += 4)
                            {
                                for (int x2 = 0; x2 < 8; x2 += 4)
                                {
                                    score += GetScore4x4(Block.YData4x4[b][b2],
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, FrameUtil.GetBlockPixels4x4(plane, x + x2, y + y2, 16, 0), Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref NrBits));
                                    b2++;
                                }
                            }
                            b++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = 2 | (0 << 8) | (1 << 5), Score = score, NrBits = NrBits });*/
                    /*int bestp = int.MaxValue;//0;
                    int bestpscore = int.MaxValue;//score;
                    int bestpnrbits = int.MaxValue;//NrBits;
                    for (int p = -128; p < 128; p+=8)
                    {
                        byte[] plane1 = MacroBlock.PredictIntraPlane16x16(Encoder.YDec, Block.Y * Encoder.Stride + Block.X, Encoder.Stride, p);
                        int score = 0;
                        int NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntSigned(p);
                        score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane1, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits));
                        score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane1, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits));
                        score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane1, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                        score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane1, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                        if (score < bestpscore || (score == bestpscore && NrBits < bestpnrbits))// || (score == bestpscore && NrBits == bestpnrbits && Math.Abs(p) < Math.Abs(bestp)))
                        {
                            bestpscore = score;
                            bestp = p;
                            bestpnrbits = NrBits;
                        }
                    }*/

                    byte[] pixels = Block.YData16x16;// FrameUtil.GetBlockPixels16x16(Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0);

                    /*int bestp = int.MaxValue;//0;
                    int bestpscore = int.MaxValue;//score;
                    for (int p = -128; p < 128; p += 8)
                    {
                        byte[] plane1 = MacroBlock.PredictIntraPlane16x16(Encoder.YDec, Block.Y * Encoder.Stride + Block.X, Encoder.Stride, p);
                        int score = 0;
                        for (int i = 0; i < 256; i++)
                        {
                            score += Math.Abs(pixels[i] - plane1[i]);
                        }
                        if (score < bestpscore)// || (score == bestpscore && NrBits < bestpnrbits))// || (score == bestpscore && NrBits == bestpnrbits && Math.Abs(p) < Math.Abs(bestp)))
                        {
                            bestpscore = score;
                            bestp = p;
                           // bestpnrbits = NrBits;
                        }
                    }*/

                    byte[] plane0 = MacroBlock.PredictIntraPlane16x16(Encoder.YDec, Block.Y * Encoder.Stride + Block.X, Encoder.Stride, 0);
                    /*int score0 = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        score0 += Math.Abs(pixels[i] - plane[i]);
                    }*/


                    // float pcalc = 0;
                    //int total = 0;

                    int[] bestps = new int[256];
                    // Dictionary<int, int> counts = new Dictionary<int, int>();
                    for (int i = 0; i < 256; i++)
                    {
                        int x = i % 16;
                        int y = i / 16;
                        int coef = (x + 1) * 2 * (y + 1);
                        bestps[i] = (pixels[i] - plane0[i]) * 256 / coef;
                    }

                    int bestp2 = 0;
                    for (int y = 16 - 5; y < 16; y++)
                    {
                        for (int x = 16 - 5; x < 16; x++)
                        {
                            bestp2 += bestps[x + y * 16];
                        }
                    }
                    bestp2 /= 25;//16;

                    //int bestp2 = (bestps[255] + bestps[254] + bestps[239] + bestps[238]) / 4;

                    /*int pcalc2 = 0;
                    for (int i = 0; i < Math.Min(ps.Length, 3); i++)
                    {
                        pcalc2 += ps[ps.Length - i - 1];
                    }
                    pcalc2 /= Math.Min(ps.Length, 3);*/
                    //pcalc /= total;// *2;
                    //pcalc *= 256;

                    byte[] plane = MacroBlock.PredictIntraPlane16x16(Encoder.YDec, Block.Y * Encoder.Stride + Block.X, Encoder.Stride, bestp2);//ps[ps.Length - 1]);//pcalc2);
                    /*int score2 = 0;
                    for (int i = 0; i < 256; i++)
                    {
                        score2 += Math.Abs(pixels[i] - plane2[i]);
                    }*/


                    //scores.Add(new BlockScore() { BlockConfigId = 2 | (bestp << 8), Score = bestpscore, NrBits = bestpnrbits });

                    int score = 0;
                    int NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntSigned(bestp2);
                    score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 0, 0, 16, 0), Block.X, Block.Y, Encoder.Stride, 0, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 8, 0, 16, 0), Block.X + 8, Block.Y, Encoder.Stride, 0, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 0, 8, 16, 0), Block.X, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, FrameUtil.GetBlockPixels8x8(plane, 8, 8, 16, 0), Block.X + 8, Block.Y + 8, Encoder.Stride, 0, ref NrBits));
                    scores.Add(new BlockScore() { BlockConfigId = 2 | (/*0*/bestp2 << 8), Score = score, NrBits = NrBits });
                    score = 0;
                    NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntSigned(bestp2) + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                    int b = 0;
                    for (int y = 0; y < 16; y += 8)
                    {
                        for (int x = 0; x < 16; x += 8)
                        {
                            int b2 = 0;
                            for (int y2 = 0; y2 < 8; y2 += 4)
                            {
                                for (int x2 = 0; x2 < 8; x2 += 4)
                                {
                                    score += GetScore4x4(Block.YData4x4[b][b2],
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, FrameUtil.GetBlockPixels4x4(plane, x + x2, y + y2, 16, 0), Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref NrBits));
                                    b2++;
                                }
                            }
                            b++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = 2 | (/*0*/bestp2 << 8) | (1 << 5), Score = score, NrBits = NrBits });
                }


                for (int i = 4; i <= 7; i++)
                {
                    int score = 0;
                    int NrBits = (PFrame ? 5 : 0);
                    score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, i, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, i, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, i, ref NrBits));
                    score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, i, ref NrBits));
                    scores.Add(new BlockScore() { BlockConfigId = i, Score = score, NrBits = NrBits });
                    score = 0;
                    NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;
                    int b = 0;
                    for (int y = 0; y < 16; y += 8)
                    {
                        for (int x = 0; x < 16; x += 8)
                        {
                            int b2 = 0;
                            for (int y2 = 0; y2 < 8; y2 += 4)
                            {
                                for (int x2 = 0; x2 < 8; x2 += 4)
                                {
                                    score += GetScore4x4(Block.YData4x4[b][b2],
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + i, ref NrBits));
                                    b2++;
                                }
                            }
                            b++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = i | (1 << 5), Score = score, NrBits = NrBits });
                }
            }
            int[][] Types = new int[4][];
            bool[] Use8x8Subblock = new bool[4];
            int[][] PlaneParams = new int[4][];
            //Try subblock intra prediction mode
            {
                int score = 0;
                int NrBits = (PFrame ? 5 : 0) + BitWriter.GetNrBitsRequiredVarIntUnsigned(1) * 4;// +2 * 16;
                Types[0] = new int[4];
                Types[1] = new int[4];
                Types[2] = new int[4];
                Types[3] = new int[4];
                PlaneParams[0] = new int[4];
                PlaneParams[1] = new int[4];
                PlaneParams[2] = new int[4];
                PlaneParams[3] = new int[4];
                //For every 4x4 block, find out which predictor is the best.
                int b = 0;
                for (int y = 0; y < 16; y += 8)
                {
                    for (int x = 0; x < 16; x += 8)
                    {
                        int bestscore8x8 = int.MaxValue;
                        int bestnrbits8x8 = int.MaxValue;
                        int besttype8x8 = -1;
                        int planeparam8x8 = 0;
                        //try out 8x8 prediction
                        for (int i = 0; i <= 8; i++)
                        {
                            //there seems to be some kind of bug with this
                            /*if ((Block.X + x > 0 && Block.Y + y > 0) && i == 2)
                            {
                                byte[] pixels = Block.YData8x8[b];
                                byte[] plane0 = MacroBlock.PredictIntraPlane8x8(Encoder.YDec, (Block.Y + y) * Encoder.Stride + (Block.X + x), Encoder.Stride, 0);
                                int[] bestps = new int[64];
                                for (int i2 = 0; i2 < 64; i2++)
                                {
                                    int x6 = i2 % 8;
                                    int y6 = i2 / 8;
                                    int coef = (x6 + 1) * 4 * (y6 + 1);
                                    bestps[i] = (pixels[i] - plane0[i]) * 128 / coef;
                                }

                                int bestp2 = 0;
                                for (int y6 = 8 - 3; y6 < 8; y6++)
                                {
                                    for (int x6 = 8 - 3; x6 < 8; x6++)
                                    {
                                        bestp2 += bestps[x6 + y6 * 8];
                                    }
                                }
                                bestp2 /= 9;

                                planeparam8x8 = bestp2;

                                byte[] plane = MacroBlock.PredictIntraPlane8x8(Encoder.YDec, (Block.Y + y) * Encoder.Stride + (Block.X + x), Encoder.Stride, bestp2);
                                int subnrbits2 = BitWriter.GetNrBitsRequiredVarIntSigned(bestp2);
                                int subscore2 = GetScore8x8(Block.YData8x8[b],
                                    MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[b], Encoder.YDec, plane, Block.X + x, Block.Y + y, Encoder.Stride, 0, ref subnrbits2));
                                if (subscore2 < bestscore8x8 || (subscore2 == bestscore8x8 && subnrbits2 < bestnrbits8x8))
                                {
                                    bestscore8x8 = subscore2;
                                    bestnrbits8x8 = subnrbits2;
                                    besttype8x8 = i;
                                }
                                continue;
                            }*/
                            if ((Block.Y + y == 0 && i == 0) || (Block.X + x == 0 && i == 1) || i == 2 || ((Block.X + x == 0 || Block.Y + y == 0) && i >= 4 && i <= 7) ||
                                 (i == 8 && ((x > 0 && y > 0) || (Block.Y + y == 0) || (Block.X + x + 8) >= Encoder.Width)))
                                continue;
                            int subnrbits = 0;
                            int subscore = GetScore8x8(Block.YData8x8[b],
                                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[b], Encoder.YDec, Block.X + x, Block.Y + y, Encoder.Stride, 0, i, ref subnrbits));
                            if (subscore < bestscore8x8 || (subscore == bestscore8x8 && subnrbits < bestnrbits8x8))
                            {
                                bestscore8x8 = subscore;
                                bestnrbits8x8 = subnrbits;
                                besttype8x8 = i;
                            }
                        }
                        int total4x4score = 0;
                        int total4x4nrbits = 0;
                        int[] types4x4 = new int[4];
                        int b2 = 0;
                        for (int y2 = 0; y2 < 8; y2 += 4)
                        {
                            for (int x2 = 0; x2 < 8; x2 += 4)
                            {
                                int subscore_best = int.MaxValue;
                                int subnrbits_best = int.MaxValue;
                                int subtype_best = -1;
                                for (int i = 0; i <= 8; i++)
                                {
                                    if ((Block.X + x + x2 > 0 && Block.Y + y + y2 > 0) && i == 2)
                                    {
                                        byte[] pixels = Block.YData4x4[b][b2];
                                        byte[] plane0 = MacroBlock.PredictIntraPlane4x4(Encoder.YDec, (Block.Y + y + y2) * Encoder.Stride + (Block.X + x + x2), Encoder.Stride, 0);
                                        int[] bestps = new int[256];
                                        for (int i2 = 0; i2 < 16; i2++)
                                        {
                                            int x6 = i2 % 4;
                                            int y6 = i2 / 4;
                                            int coef = (x6 + 1) * 4 * (y6 + 1);
                                            bestps[i] = (pixels[i] - plane0[i]) * 32 / coef;
                                        }

                                        int bestp2 = 0;
                                        for (int y6 = 4 - 2; y6 < 4; y6++)
                                        {
                                            for (int x6 = 4 - 2; x6 < 4; x6++)
                                            {
                                                bestp2 += bestps[x6 + y6 * 4];
                                            }
                                        }
                                        bestp2 /= 4;

                                        PlaneParams[b][b2] = bestp2;

                                        byte[] plane = MacroBlock.PredictIntraPlane4x4(Encoder.YDec, (Block.Y + y + y2) * Encoder.Stride + (Block.X + x + x2), Encoder.Stride, bestp2);
                                        int subnrbits2 = BitWriter.GetNrBitsRequiredVarIntSigned(bestp2);
                                        int subscore2 = GetScore4x4(Block.YData4x4[b][b2],
                                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, plane, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref subnrbits2));
                                        if (subscore2 < subscore_best || (subscore2 == subscore_best && subnrbits2 < subnrbits_best))
                                        {
                                            subscore_best = subscore2;
                                            subnrbits_best = subnrbits2;
                                            subtype_best = i;
                                        }
                                        continue;
                                    }
                                    if ((Block.Y + y + y2 == 0 && i == 0) || (Block.X + x + x2 == 0 && i == 1) || i == 2 || ((Block.X + x + x2 == 0 || Block.Y + y + y2 == 0) && i >= 4 && i <= 7) ||
                                        (i == 8 && (!Type8Supported[y + y2 + (x + x2) / 4] || (Block.Y + y + y2 == 0) || (Block.X + x + x2 + 4) >= Encoder.Width)))
                                        continue;
                                    int subnrbits = 0;
                                    int subscore = GetScore4x4(Block.YData4x4[b][b2],
                                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + i, ref subnrbits));
                                    if (subscore < subscore_best || (subscore == subscore_best && subnrbits < subnrbits_best))
                                    {
                                        subscore_best = subscore;
                                        subnrbits_best = subnrbits;
                                        subtype_best = i;
                                    }
                                }
                                total4x4nrbits += subnrbits_best;
                                total4x4score += subscore_best;
                                types4x4[b2] = subtype_best;
                                int tmpbits = 0;
                                if (subtype_best == 2)
                                {
                                    tmpbits += BitWriter.GetNrBitsRequiredVarIntSigned(PlaneParams[b][b2]);
                                    byte[] plane = MacroBlock.PredictIntraPlane4x4(Encoder.YDec, (Block.Y + y + y2) * Encoder.Stride + (Block.X + x + x2), Encoder.Stride, PlaneParams[b][b2]);
                                    MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, plane, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, ref tmpbits);
                                }
                                else
                                    MacroBlock.EncodeDecode4x4Block(Encoder, Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, 10 + subtype_best, ref tmpbits);
                                b2++;
                            }
                        }
                        if (bestscore8x8 <= total4x4score)
                        {
                            score += bestscore8x8;
                            NrBits += bestnrbits8x8;
                            Types[b][0] = besttype8x8;
                            Use8x8Subblock[b] = true;
                            PlaneParams[b][0] = planeparam8x8;
                            int tmpbits = 0;
                            if (besttype8x8 == 2)
                            {
                                tmpbits += BitWriter.GetNrBitsRequiredVarIntSigned(planeparam8x8);
                                byte[] plane = MacroBlock.PredictIntraPlane8x8(Encoder.YDec, (Block.Y + y) * Encoder.Stride + (Block.X + x), Encoder.Stride, planeparam8x8);
                                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[b], Encoder.YDec, plane, Block.X + x, Block.Y + y, Encoder.Stride, 0, ref tmpbits);
                            }
                            else
                                MacroBlock.EncodeDecode8x8Block(Encoder, Block.YData8x8[b], Encoder.YDec, Block.X + x, Block.Y + y, Encoder.Stride, 0, besttype8x8, ref tmpbits);
                            NrBits += 2;
                        }
                        else
                        {
                            score += total4x4score;
                            NrBits += total4x4nrbits;
                            Types[b] = types4x4;
                            Use8x8Subblock[b] = false;
                            NrBits += 2 * 4;
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 0 | (1 << 7), Score = score, NrBits = NrBits });
            }
        finalize:
            BlockScore best = null;
            foreach (BlockScore s in scores)
            {
                if (
                    best == null ||
                    s.Score + labda * s.NrBits < best.Score + labda * best.NrBits)
                    //s.NrBits < best.NrBits ||
                    //(s.NrBits == best.NrBits && s.Score < best.Score))
                    // s.Score < best.Score ||
                    //(s.Score == best.Score && s.NrBits < best.NrBits))
                    //(s.Score == best.Score && (s.BlockConfigId & 0x20) == 0 && (best.BlockConfigId & 0x20) != 0) ||
                    //(s.Score == best.Score && (s.BlockConfigId & 0x40) != 0 && (best.BlockConfigId & 0x40) != 0 && (s.BlockConfigId & 0x1F) < (best.BlockConfigId & 0x1F)))
                    best = s;
            }
            Block.YUseComplex8x8[0] = true;
            Block.YUseComplex8x8[1] = true;
            Block.YUseComplex8x8[2] = true;
            Block.YUseComplex8x8[3] = true;
            if (((best.BlockConfigId >> 7) & 1) == 1)
            {
                Block.YUse4x4[0] = !Use8x8Subblock[0];
                Block.YUse4x4[1] = !Use8x8Subblock[1];
                Block.YUse4x4[2] = !Use8x8Subblock[2];
                Block.YUse4x4[3] = !Use8x8Subblock[3];
                for (int i = 0; i < 4; i++)
                {
                    if (!Block.YUse4x4[i]) continue;
                    for (int j = 0; j < 4; j++)
                    {
                        Block.YUseDCT4x4[i][j] = true;
                    }
                }
                Block.UseIntraSubBlockMode = true;
                Block.YIntraSubBlockModeTypes = Types;
                Block.YIntraSubBlockModePlaneParams = PlaneParams;
            }
            else if (((best.BlockConfigId >> 6) & 1) == 1)
            {
                Block.UseInterPrediction = true;
                Block.InterPredictionConfig = PredictionConfigs[best.BlockConfigId >> 8];
                //Block.InterPredictionFrame = best.BlockConfigId & 0x1F;
                //Block.InterPredictionDelta = new Point((int)(((best.BlockConfigId >> 8) & 0xFF) << 24) >> 24, (int)(((best.BlockConfigId >> 16) & 0xFF) << 24) >> 24);
                Block.UVUseComplex8x8[0] = true;
                Block.UVUseComplex8x8[1] = true;
                byte[] U = PredictionConfigs[best.BlockConfigId >> 8].GetCompvalsU(Encoder, Block, 0, 0);
                byte[] V = PredictionConfigs[best.BlockConfigId >> 8].GetCompvalsV(Encoder, Block, 0, 0);
                int NrBits = 0;
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, U, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, ref NrBits);
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, V, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, ref NrBits);
                best.NrBits += NrBits;
                //What's wrong with this?
                if (((best.BlockConfigId >> 5) & 1) == 1)
                {
                    Block.YUse4x4[0] = true;
                    Block.YUse4x4[1] = true;
                    Block.YUse4x4[2] = true;
                    Block.YUse4x4[3] = true;
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            Block.YUseDCT4x4[i][j] = true;
                        }
                    }
                }
            }
            else
            {
                Block.YPredictionMode = best.BlockConfigId & 0x1F;
                if (Block.YPredictionMode == 2)
                {
                    Block.YPredict16x16Arg = ((int)best.BlockConfigId >> 8);
                }
                if (((best.BlockConfigId >> 5) & 1) == 1)
                {
                    Block.YUse4x4[0] = true;
                    Block.YUse4x4[1] = true;
                    Block.YUse4x4[2] = true;
                    Block.YUse4x4[3] = true;
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            Block.YUseDCT4x4[i][j] = true;
                        }
                    }
                }
            }
            return best.NrBits;
        }

        public static int ConfigureBlockUV(MobiEncoder Encoder, MacroBlock Block)
        {
            float labda = 0.85f * (float)Math.Pow(2, (Encoder.Quantizer - 12) / 3f);
            List<BlockScore> scores = new List<BlockScore>();
            for (int i = 0; i <= 7; i++)
            {
                if ((i == 0 && Block.Y < 8) || (i == 1 && Block.X < 8) || i == 2 || (i >= 4 && (Block.Y < 8 || Block.X < 8)))
                    continue;
                int score = 0;
                int NrBits = 0;
                score += CalcScore8x8(Block.UData8x8, MacroBlock.GetCompvals8x8(i, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, i, ref NrBits);
                score += CalcScore8x8(Block.VData8x8, MacroBlock.GetCompvals8x8(i, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2));
                MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, i, ref NrBits);
                scores.Add(new BlockScore() { BlockConfigId = i, Score = score, NrBits = NrBits });
                score = 0;
                NrBits = BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                int b2 = 0;
                for (int y2 = 0; y2 < 8; y2 += 4)
                {
                    for (int x2 = 0; x2 < 8; x2 += 4)
                    {
                        score += CalcScore4x4(Block.UData4x4[b2], MacroBlock.GetCompvals4x4(10 + i, Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0));
                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.UData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0, 10 + i, ref NrBits);
                        score += CalcScore4x4(Block.VData4x4[b2], MacroBlock.GetCompvals4x4(10 + i, Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2));
                        MacroBlock.EncodeDecode4x4Block(Encoder, Block.VData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2, 10 + i, ref NrBits);
                        b2++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = i | (1 << 5), Score = score, NrBits = NrBits });
            }
            if (Block.X >= 8 && Block.Y >= 8)
            {
                //Try type 2 with param 0 for now
                {
                    int bestu;
                    //u
                    {
                        byte[] pixels = Block.UData8x8;

                        byte[] plane0 = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2), Encoder.Stride, 0);

                        int[] bestps = new int[64];
                        // Dictionary<int, int> counts = new Dictionary<int, int>();
                        for (int i = 0; i < 64; i++)
                        {
                            int x = i % 8;
                            int y = i / 8;
                            int coef = (x + 1) * 4 * (y + 1);
                            int diff = pixels[i] - plane0[i];
                            if (diff >= 0)
                                bestps[i] = (diff * 128 + (coef >> 1)) / coef;
                            else
                                bestps[i] = (diff * 128 - (coef >> 1)) / coef;
                        }

                        int bestp2 = 0;
                        for (int y = 8 - 3; y < 8; y++)
                        {
                            for (int x = 8 - 3; x < 8; x++)
                            {
                                bestp2 += bestps[x + y * 8];
                            }
                        }
                        bestp2 /= 9;
                        if (plane0[63] + bestp2 * 2 < 0)
                            bestp2 = -((plane0[63] - 1) / 2);
                        else if (plane0[63] + bestp2 * 2 > 255)
                            bestp2 = ((255 - plane0[63]) + 1) / 2;
                        bestu = bestp2;
                    }

                    int bestv;
                    //v
                    {
                        byte[] pixels = Block.VData8x8;

                        byte[] plane0 = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2) + Encoder.Stride / 2, Encoder.Stride, 0);

                        int[] bestps = new int[64];
                        // Dictionary<int, int> counts = new Dictionary<int, int>();
                        for (int i = 0; i < 64; i++)
                        {
                            int x = i % 8;
                            int y = i / 8;
                            int coef = (x + 1) * 4 * (y + 1);
                            int diff = pixels[i] - plane0[i];
                            if (diff >= 0)
                                bestps[i] = (diff * 128 + (coef >> 1)) / coef;
                            else
                                bestps[i] = (diff * 128 - (coef >> 1)) / coef;
                        }

                        int bestp2 = 0;
                        for (int y = 8 - 3; y < 8; y++)
                        {
                            for (int x = 8 - 3; x < 8; x++)
                            {
                                bestp2 += bestps[x + y * 8];
                            }
                        }
                        bestp2 /= 9;
                        if (plane0[63] + bestp2 * 2 < 0)
                            bestp2 = -((plane0[63] - 1) / 2);
                        else if (plane0[63] + bestp2 * 2 > 255)
                            bestp2 = ((255 - plane0[63]) + 1) / 2;
                        bestv = bestp2;
                    }

                    byte[] planeu = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2), Encoder.Stride, bestu);
                    int score = 0;
                    int NrBits = BitWriter.GetNrBitsRequiredVarIntSigned(bestu) + BitWriter.GetNrBitsRequiredVarIntSigned(bestv);
                    score += CalcScore8x8(Block.UData8x8, planeu);
                    MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, planeu, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, ref NrBits);
                    byte[] planev = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2) + Encoder.Stride / 2, Encoder.Stride, bestv);
                    score += CalcScore8x8(Block.VData8x8, planev);
                    MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, planev, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, ref NrBits);
                    scores.Add(new BlockScore() { BlockConfigId = 2 | ((bestu << 8) & 0xFFF) | ((bestv << 20) & 0xFFF), Score = score, NrBits = NrBits });
                    score = 0;
                    NrBits = BitWriter.GetNrBitsRequiredVarIntSigned(bestu) + BitWriter.GetNrBitsRequiredVarIntSigned(bestv);
                    int b2 = 0;
                    for (int y2 = 0; y2 < 8; y2 += 4)
                    {
                        for (int x2 = 0; x2 < 8; x2 += 4)
                        {
                            score += CalcScore4x4(Block.UData4x4[b2], FrameUtil.GetBlockPixels4x4(planeu, x2, y2, 8, 0));
                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.UData4x4[b2], Encoder.UVDec, FrameUtil.GetBlockPixels4x4(planeu, x2, y2, 8, 0), Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0, ref NrBits);
                            score += CalcScore4x4(Block.VData4x4[b2], FrameUtil.GetBlockPixels4x4(planev, x2, y2, 8, 0));
                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.VData4x4[b2], Encoder.UVDec, FrameUtil.GetBlockPixels4x4(planev, x2, y2, 8, 0), Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2, ref NrBits);
                            b2++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = 2 | ((bestu << 8) & 0xFFF) | ((bestv << 20) & 0xFFF) | (1 << 5), Score = score, NrBits = NrBits });
                }
            }
            BlockScore best = null;
            foreach (BlockScore s in scores)
            {
                if (
                    best == null ||
                    s.Score + labda * s.NrBits < best.Score + labda * best.NrBits)
                    //s.NrBits < best.NrBits ||
                    // (s.NrBits == best.NrBits && s.Score < best.Score))
                    //s.Score < best.Score ||
                    //(s.Score == best.Score && s.NrBits < best.NrBits))
                    //(s.Score == best.Score && (s.BlockConfigId & 0x20) == 0 && (best.BlockConfigId & 0x20) != 0))
                    best = s;
            }
            Block.UVPredictionMode = best.BlockConfigId & 0x1F;
            if (Block.UVPredictionMode == 2)
            {
                Block.UVPredict8x8ArgU = ((int)(((best.BlockConfigId >> 8) & 0xFFF) << 4)) >> 4;
                Block.UVPredict8x8ArgV = ((int)best.BlockConfigId >> 20);
                //Block.UVPredict8x8ArgU = Block.UVPredict8x8ArgV = ((int)best.BlockConfigId >> 8);
            }
            Block.UVUseComplex8x8[0] = true;
            Block.UVUseComplex8x8[1] = true;
            if (((best.BlockConfigId >> 5) & 1) == 1)
            {
                Block.UVUse4x4[0] = true;
                Block.UVUse4x4[1] = true;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Block.UVUseDCT4x4[i][j] = true;
                    }
                }
            }
            return best.NrBits;
        }

        public static int ConfigureBlockUV_old(MobiEncoder Encoder, MacroBlock Block)
        {
            float labda = 0.85f * (float)Math.Pow(2, (Encoder.Quantizer - 12) / 3f);
            List<BlockScore> scores = new List<BlockScore>();
            //8x8
            if (Block.Y >= 8)
            {
                int score = 0;
                int NrBits = 0;
                score += GetScore8x8(Block.UData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, 0, ref NrBits));
                score += GetScore8x8(Block.VData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, 0, ref NrBits));
                scores.Add(new BlockScore() { BlockConfigId = 0, Score = score, NrBits = NrBits });
                score = 0;
                NrBits = BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                int b2 = 0;
                for (int y2 = 0; y2 < 8; y2 += 4)
                {
                    for (int x2 = 0; x2 < 8; x2 += 4)
                    {
                        score += GetScore4x4(Block.UData4x4[b2],
                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.UData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0, 10 + 0, ref NrBits));
                        score += GetScore4x4(Block.VData4x4[b2],
                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.VData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2, 10 + 0, ref NrBits));
                        b2++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 0 | (1 << 5), Score = score, NrBits = NrBits });
            }
            if (Block.X >= 8)
            {
                int score = 0;
                int NrBits = 0;
                score += GetScore8x8(Block.UData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, 1, ref NrBits));
                score += GetScore8x8(Block.VData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, 1, ref NrBits));
                scores.Add(new BlockScore() { BlockConfigId = 1, Score = score, NrBits = NrBits });
                score = 0;
                NrBits = BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                int b2 = 0;
                for (int y2 = 0; y2 < 8; y2 += 4)
                {
                    for (int x2 = 0; x2 < 8; x2 += 4)
                    {
                        score += GetScore4x4(Block.UData4x4[b2],
                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.UData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0, 10 + 1, ref NrBits));
                        score += GetScore4x4(Block.VData4x4[b2],
                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.VData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2, 10 + 1, ref NrBits));
                        b2++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 1 | (1 << 5), Score = score, NrBits = NrBits });
            }
            //Block type 3
            {
                int score = 0;
                int NrBits = 0;
                score += GetScore8x8(Block.UData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, 3, ref NrBits));
                score += GetScore8x8(Block.VData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, 3, ref NrBits));
                scores.Add(new BlockScore() { BlockConfigId = 3, Score = score, NrBits = NrBits });
                score = 0;
                NrBits = BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                int b2 = 0;
                for (int y2 = 0; y2 < 8; y2 += 4)
                {
                    for (int x2 = 0; x2 < 8; x2 += 4)
                    {
                        score += GetScore4x4(Block.UData4x4[b2],
                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.UData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0, 10 + 3, ref NrBits));
                        score += GetScore4x4(Block.VData4x4[b2],
                            MacroBlock.EncodeDecode4x4Block(Encoder, Block.VData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2, 10 + 3, ref NrBits));
                        b2++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 3 | (1 << 5), Score = score, NrBits = NrBits });
            }
            if (Block.X >= 8 && Block.Y >= 8)
            {
                //Try type 2 with param 0 for now
                {
                    /*byte[] planeu = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2), Encoder.Stride, 0);
                    int score = 0;
                    int NrBits = 2 * BitWriter.GetNrBitsRequiredVarIntSigned(0);
                    score += GetScore8x8(Block.UData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, planeu, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, ref NrBits));
                    byte[] planev = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2) + Encoder.Stride / 2, Encoder.Stride, 0);
                    score += GetScore8x8(Block.UData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, planev, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, ref NrBits));
                    scores.Add(new BlockScore() { BlockConfigId = 2 | (0 << 8), Score = score, NrBits = NrBits });
                    score = 0;
                    NrBits = 2 * BitWriter.GetNrBitsRequiredVarIntSigned(0);
                    int b2 = 0;
                    for (int y2 = 0; y2 < 8; y2 += 4)
                    {
                        for (int x2 = 0; x2 < 8; x2 += 4)
                        {
                            score += GetScore4x4(Block.UData4x4[b2],
                                MacroBlock.EncodeDecode4x4Block(Encoder, Block.UData4x4[b2], Encoder.UVDec, FrameUtil.GetBlockPixels4x4(planeu, x2, y2, 8, 0), Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0, ref NrBits));
                            score += GetScore4x4(Block.VData4x4[b2],
                                MacroBlock.EncodeDecode4x4Block(Encoder, Block.VData4x4[b2], Encoder.UVDec, FrameUtil.GetBlockPixels4x4(planev, x2, y2, 8, 0), Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2, ref NrBits));
                            b2++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = 2 | (0 << 8) | (1 << 5), Score = score, NrBits = NrBits });*/
                    int bestu;
                    //u
                    {
                        byte[] pixels = Block.UData8x8;

                        byte[] plane0 = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2), Encoder.Stride, 0);

                        int[] bestps = new int[64];
                        // Dictionary<int, int> counts = new Dictionary<int, int>();
                        for (int i = 0; i < 64; i++)
                        {
                            int x = i % 8;
                            int y = i / 8;
                            int coef = (x + 1) * 4 * (y + 1);
                            bestps[i] = (pixels[i] - plane0[i]) * 128 / coef;
                        }

                        int bestp2 = 0;
                        for (int y = 8 - 3; y < 8; y++)
                        {
                            for (int x = 8 - 3; x < 8; x++)
                            {
                                bestp2 += bestps[x + y * 8];
                            }
                        }
                        bestp2 /= 9;
                        bestu = bestp2;
                    }

                    int bestv;
                    //v
                    {
                        byte[] pixels = Block.VData8x8;

                        byte[] plane0 = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2) + Encoder.Stride / 2, Encoder.Stride, 0);

                        int[] bestps = new int[64];
                        // Dictionary<int, int> counts = new Dictionary<int, int>();
                        for (int i = 0; i < 64; i++)
                        {
                            int x = i % 8;
                            int y = i / 8;
                            int coef = (x + 1) * 4 * (y + 1);
                            bestps[i] = (pixels[i] - plane0[i]) * 128 / coef;
                        }

                        int bestp2 = 0;
                        for (int y = 8 - 3; y < 8; y++)
                        {
                            for (int x = 8 - 3; x < 8; x++)
                            {
                                bestp2 += bestps[x + y * 8];
                            }
                        }
                        bestp2 /= 9;
                        bestv = bestp2;
                    }

                    byte[] planeu = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2), Encoder.Stride, bestu);
                    int score = 0;
                    int NrBits = BitWriter.GetNrBitsRequiredVarIntSigned(bestu) + BitWriter.GetNrBitsRequiredVarIntSigned(bestv);
                    score += GetScore8x8(Block.UData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, planeu, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, ref NrBits));
                    byte[] planev = MacroBlock.PredictIntraPlane8x8(Encoder.UVDec, (Block.Y / 2) * Encoder.Stride + (Block.X / 2) + Encoder.Stride / 2, Encoder.Stride, bestv);
                    score += GetScore8x8(Block.VData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, planev, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, ref NrBits));
                    scores.Add(new BlockScore() { BlockConfigId = 2 | ((bestu << 8) & 0xFFF) | ((bestv << 20) & 0xFFF), Score = score, NrBits = NrBits });
                    score = 0;
                    NrBits = BitWriter.GetNrBitsRequiredVarIntSigned(bestu) + BitWriter.GetNrBitsRequiredVarIntSigned(bestv);
                    int b2 = 0;
                    for (int y2 = 0; y2 < 8; y2 += 4)
                    {
                        for (int x2 = 0; x2 < 8; x2 += 4)
                        {
                            score += GetScore4x4(Block.UData4x4[b2],
                                MacroBlock.EncodeDecode4x4Block(Encoder, Block.UData4x4[b2], Encoder.UVDec, FrameUtil.GetBlockPixels4x4(planeu, x2, y2, 8, 0), Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0, ref NrBits));
                            score += GetScore4x4(Block.VData4x4[b2],
                                MacroBlock.EncodeDecode4x4Block(Encoder, Block.VData4x4[b2], Encoder.UVDec, FrameUtil.GetBlockPixels4x4(planev, x2, y2, 8, 0), Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2, ref NrBits));
                            b2++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = 2 | ((bestu << 8) & 0xFFF) | ((bestv << 20) & 0xFFF) | (1 << 5), Score = score, NrBits = NrBits });
                }
                for (int i = 4; i <= 7; i++)
                {
                    int score = 0;
                    int NrBits = 0;
                    score += GetScore8x8(Block.UData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.UData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, i, ref NrBits));
                    score += GetScore8x8(Block.VData8x8, MacroBlock.EncodeDecode8x8Block(Encoder, Block.VData8x8, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, i, ref NrBits));
                    scores.Add(new BlockScore() { BlockConfigId = i, Score = score, NrBits = NrBits });
                    score = 0;
                    NrBits = BitWriter.GetNrBitsRequiredVarIntUnsigned(15);
                    int b2 = 0;
                    for (int y2 = 0; y2 < 8; y2 += 4)
                    {
                        for (int x2 = 0; x2 < 8; x2 += 4)
                        {
                            score += GetScore4x4(Block.UData4x4[b2],
                                MacroBlock.EncodeDecode4x4Block(Encoder, Block.UData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, 0, 10 + i, ref NrBits));
                            score += GetScore4x4(Block.VData4x4[b2],
                                MacroBlock.EncodeDecode4x4Block(Encoder, Block.VData4x4[b2], Encoder.UVDec, Block.X / 2 + x2, Block.Y / 2 + y2, Encoder.Stride, Encoder.Stride / 2, 10 + i, ref NrBits));
                            b2++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = i | (1 << 5), Score = score, NrBits = NrBits });
                }
            }
        finalize:
            BlockScore best = null;
            foreach (BlockScore s in scores)
            {
                if (
                    best == null ||
                    s.Score + labda * s.NrBits < best.Score + labda * best.NrBits)
                    // s.NrBits < best.NrBits ||
                    // (s.NrBits == best.NrBits && s.Score < best.Score))
                    //s.Score < best.Score ||
                    //(s.Score == best.Score && s.NrBits < best.NrBits))
                    //(s.Score == best.Score && (s.BlockConfigId & 0x20) == 0 && (best.BlockConfigId & 0x20) != 0))
                    best = s;
            }
            Block.UVPredictionMode = best.BlockConfigId & 0x1F;
            if (Block.UVPredictionMode == 2)
            {
                Block.UVPredict8x8ArgU = ((int)(((best.BlockConfigId >> 8) & 0xFFF) << 4)) >> 4;
                Block.UVPredict8x8ArgV = ((int)best.BlockConfigId >> 20);
                //Block.UVPredict8x8ArgU = Block.UVPredict8x8ArgV = ((int)best.BlockConfigId >> 8);
            }
            Block.UVUseComplex8x8[0] = true;
            Block.UVUseComplex8x8[1] = true;
            if (((best.BlockConfigId >> 5) & 1) == 1)
            {
                Block.UVUse4x4[0] = true;
                Block.UVUse4x4[1] = true;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Block.UVUseDCT4x4[i][j] = true;
                    }
                }
            }
            return best.NrBits;
        }

        private unsafe static int GetScore8x8(byte[] Block, byte[] Result)
        {
            fixed (byte* pfBlock = &Block[0], pfResult = &Result[0])
            {
                byte* pBlock = pfBlock;
                byte* pResult = pfResult;
                int diff = 0;
                for (int i = 0; i < 4; i++)
                {
                    int diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                    diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff -= diff2;
                    else diff += diff2;
                }
                /*for (int i = 0; i < 64; i++)
                {
                    int diff2 = *pBlock++ - *pResult++;
                    if (diff2 < 0) diff2 = -diff2;
                    diff += diff2;
                }*/
                return diff;
            }
        }

        private unsafe static int GetScore4x4(byte[] Block, byte[] Result)
        {
            fixed (byte* pfBlock = &Block[0], pfResult = &Result[0])
            {
                byte* pBlock = pfBlock;
                byte* pResult = pfResult;
                int diff = 0;
                int diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                diff2 = *pBlock++ - *pResult++;
                if (diff2 < 0) diff -= diff2;
                else diff += diff2;
                return diff;
            }
        }

        /*private static int GetScore2x2(byte[] Block, byte[] Result)
        {
            int a = Block[0] - Result[0];
            if (a < 0) a = -a;
            int b = Block[1] - Result[1];
            if (b < 0) b = -b;
            int c = Block[2] - Result[2];
            if (c < 0) c = -c;
            int d = Block[3] - Result[3];
            if (d < 0) d = -d;
            return a + b + c + d;
        }*/
    }
}
