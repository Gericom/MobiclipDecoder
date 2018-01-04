using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Tests
{
    [TestClass]
    public class DecoderTest
    {
        string redirect = "..\\..\\..\\MobiclipDecoder\\bin\\Debug\\";

        [TestMethod]
        public void DecodeTest1()
        {
            FileAssert.AreEqual(redirect + "original\\IFrame0orig.png", redirect + "current\\IFrame0.png");
            FileAssert.AreEqual(redirect + "original\\IFrame2orig.png", redirect + "current\\IFrame2.png");

            FileAssert.AreEqual(redirect + "original\\PFrame0orig.png", redirect + "current\\PFrame0.png");
            FileAssert.AreEqual(redirect + "original\\PFrame1orig.png", redirect + "current\\PFrame1.png");
            FileAssert.AreEqual(redirect + "original\\PFrame132orig.png", redirect + "current\\PFrame132.png");
            FileAssert.AreEqual(redirect + "original\\PFrame133orig.png", redirect + "current\\PFrame133.png");
        }

        public static class FileAssert
        {
            public static void AreEqual(string path1, string path2)
            {
                using (var fs1 = File.OpenRead(path1))
                {
                    using (var fs2 = File.OpenRead(path2))
                    {
                        Assert.AreEqual(fs1.Length, fs2.Length, "The two files have different lengths.");

                        int b1;
                        while ((b1 = fs1.ReadByte()) != -1)
                        {
                            int b2 = fs2.ReadByte();
                            if (b1 == b2) continue; // This makes it run a lot faster
                            Assert.AreEqual(b1, b2, $"Byte mismatch at position 0x{fs1.Position - 1:X}");
                        }
                    }
                }
            }
        }
    }
}
