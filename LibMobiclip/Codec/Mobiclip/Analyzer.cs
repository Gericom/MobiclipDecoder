using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibMobiclip.Codec.Mobiclip.Encoder;

namespace LibMobiclip.Codec.Mobiclip
{
    public class Analyzer
    {
        private class BlockScore
        {
            public int BlockConfigId;
            public int Score;
        }

        //This analyzer should determine the best block configuration for a macro block (mb)
        public static void ConfigureBlockY(MobiEncoder Encoder, MacroBlock Block)
        {
            List<BlockScore> scores = new List<BlockScore>();
            //8x8
            if (Block.Y >= 8)
            {
                int score = 0;
                score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, Encoder.QTable8x8, 0));
                score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, Encoder.QTable8x8, 0));
                score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, Encoder.QTable8x8, 0));
                score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, Encoder.QTable8x8, 0));
                scores.Add(new BlockScore() { BlockConfigId = 0, Score = score });
                score = 0;
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
                                    MacroBlock.EncodeDecode4x4Block(Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, Encoder.QTable4x4, 10 + 0));
                                b2++;
                            }
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 0 | (1 << 5), Score = score });
            }
            if (Block.X >= 8)
            {
                int score = 0;
                score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, Encoder.QTable8x8, 1));
                score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, Encoder.QTable8x8, 1));
                score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, Encoder.QTable8x8, 1));
                score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, Encoder.QTable8x8, 1));
                scores.Add(new BlockScore() { BlockConfigId = 1, Score = score });
                score = 0;
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
                                    MacroBlock.EncodeDecode4x4Block(Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, Encoder.QTable4x4, 10 + 1));
                                b2++;
                            }
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 1 | (1 << 5), Score = score });
            }
            //TODO: block type2
            //Block type 3
            {
                int score = 0;
                score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, Encoder.QTable8x8, 3));
                score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, Encoder.QTable8x8, 3));
                score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, Encoder.QTable8x8, 3));
                score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, Encoder.QTable8x8, 3));
                scores.Add(new BlockScore() { BlockConfigId = 3, Score = score });
                score = 0;
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
                                    MacroBlock.EncodeDecode4x4Block(Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, Encoder.QTable4x4, 10 + 3));
                                b2++;
                            }
                        }
                        b++;
                    }
                }
                scores.Add(new BlockScore() { BlockConfigId = 3 | (1 << 5), Score = score });
            }
            if (Block.X >= 8 && Block.Y >= 8)
            {
                for (int i = 4; i <= 7; i++)
                {
                    int score = 0;
                    score += GetScore8x8(Block.YData8x8[0], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[0], Encoder.YDec, Block.X, Block.Y, Encoder.Stride, 0, Encoder.QTable8x8, i));
                    score += GetScore8x8(Block.YData8x8[1], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[1], Encoder.YDec, Block.X + 8, Block.Y, Encoder.Stride, 0, Encoder.QTable8x8, i));
                    score += GetScore8x8(Block.YData8x8[2], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[2], Encoder.YDec, Block.X, Block.Y + 8, Encoder.Stride, 0, Encoder.QTable8x8, i));
                    score += GetScore8x8(Block.YData8x8[3], MacroBlock.EncodeDecode8x8Block(Block.YData8x8[3], Encoder.YDec, Block.X + 8, Block.Y + 8, Encoder.Stride, 0, Encoder.QTable8x8, i));
                    scores.Add(new BlockScore() { BlockConfigId = i, Score = score });
                    score = 0;
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
                                        MacroBlock.EncodeDecode4x4Block(Block.YData4x4[b][b2], Encoder.YDec, Block.X + x + x2, Block.Y + y + y2, Encoder.Stride, 0, Encoder.QTable4x4, 10 + i));
                                    b2++;
                                }
                            }
                            b++;
                        }
                    }
                    scores.Add(new BlockScore() { BlockConfigId = i | (1 << 5), Score = score });
                }
            }
            BlockScore best = null;
            foreach (BlockScore s in scores)
            {
                if (best == null || s.Score < best.Score || (s.Score == best.Score && (s.BlockConfigId & 0x20) == 0 && (best.BlockConfigId & 0x20) != 0)) best = s;
            }
            Block.YPredictionMode = best.BlockConfigId & 0x1F;
            Block.YUseComplex8x8[0] = true;
            Block.YUseComplex8x8[1] = true;
            Block.YUseComplex8x8[2] = true;
            Block.YUseComplex8x8[3] = true;
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

        public static int AnalyzeBlockUV(MobiEncoder Encoder, MacroBlock Block)
        {
            List<BlockScore> scores = new List<BlockScore>();
            //8x8
            if (Block.Y >= 8)
            {
                int score = 0;
                score += GetScore8x8(Block.UData, MacroBlock.EncodeDecode8x8Block(Block.UData, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, Encoder.QTable8x8, 0));
                score += GetScore8x8(Block.VData, MacroBlock.EncodeDecode8x8Block(Block.VData, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, Encoder.QTable8x8, 0));
                scores.Add(new BlockScore() { BlockConfigId = 0, Score = score });
            }
            if (Block.X >= 8)
            {
                int score = 0;
                score += GetScore8x8(Block.UData, MacroBlock.EncodeDecode8x8Block(Block.UData, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, Encoder.QTable8x8, 1));
                score += GetScore8x8(Block.VData, MacroBlock.EncodeDecode8x8Block(Block.VData, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, Encoder.QTable8x8, 1));
                scores.Add(new BlockScore() { BlockConfigId = 1, Score = score });
            }
            //TODO: block type2
            //Block type 3
            {
                int score = 0;
                score += GetScore8x8(Block.UData, MacroBlock.EncodeDecode8x8Block(Block.UData, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, Encoder.QTable8x8, 3));
                score += GetScore8x8(Block.VData, MacroBlock.EncodeDecode8x8Block(Block.VData, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, Encoder.QTable8x8, 3));
                scores.Add(new BlockScore() { BlockConfigId = 3, Score = score });
            }
            if (Block.X >= 8 && Block.Y >= 8)
            {
                for (int i = 4; i <= 7; i++)
                {
                    int score = 0;
                    score += GetScore8x8(Block.UData, MacroBlock.EncodeDecode8x8Block(Block.UData, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, 0, Encoder.QTable8x8, i));
                    score += GetScore8x8(Block.VData, MacroBlock.EncodeDecode8x8Block(Block.VData, Encoder.UVDec, Block.X / 2, Block.Y / 2, Encoder.Stride, Encoder.Stride / 2, Encoder.QTable8x8, i));
                    scores.Add(new BlockScore() { BlockConfigId = i, Score = score });
                }
            }
            BlockScore best = null;
            foreach (BlockScore s in scores)
            {
                if (best == null || s.Score < best.Score) best = s;
            }
            return best.BlockConfigId;
        }

        private static int GetScore8x8(byte[] Block, byte[] Result)
        {
            int diff = 0;
            for (int i = 0; i < 64; i++)
            {
                int diff2 = Block[i] - Result[i];
                if (diff2 < 0) diff2 = -diff2;
                diff += diff2;
            }
            return diff;
        }

        private static int GetScore4x4(byte[] Block, byte[] Result)
        {
            int diff = 0;
            for (int i = 0; i < 16; i++)
            {
                int diff2 = Block[i] - Result[i];
                if (diff2 < 0) diff2 = -diff2;
                diff += diff2;
            }
            return diff;
        }
    }
}
