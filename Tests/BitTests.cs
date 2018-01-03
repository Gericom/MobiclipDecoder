using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using LibMobiclip.Codec.Mobiclip;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void BitTest1()
        {
            //s-4 s-3 s3 u6 (3*0) (3*1) true false false true
            //32 bits
            //short LE
            //0001001 00111 00110 00111 000 111 1 0 0 1
            List<byte> test = new List<byte>() { 0x73, 0x12, 0x79, 0x1c };

            var reader = new BitReader(test.ToArray());

            //Read vals
            var var1 = reader.ReadVarSignedInt();
            var var2 = reader.ReadVarSignedInt();
            var var3 = reader.ReadVarSignedInt();
            var var4 = reader.ReadVarUnsignedInt();
            reader.SkipBits(3);
            var read1 = reader.ReadBits(3);
            bool b1 = reader.ReadBit();
            bool b2 = reader.ReadBit();
            bool b3 = reader.ReadBit();
            bool b4 = reader.ReadBit();

            var writer = new BitWriter();

            writer.WriteVarIntSigned(var1);
            writer.WriteVarIntSigned(var2);
            writer.WriteVarIntSigned(var3);
            writer.WriteVarIntUnsigned(var4);
            writer.WriteBits(0, 3);
            writer.WriteBits(read1, 3);
            writer.WriteBits(Convert.ToUInt32(b1), 1);
            writer.WriteBits(Convert.ToUInt32(b2), 1);
            writer.WriteBits(Convert.ToUInt32(b3), 1);
            writer.WriteBits(Convert.ToUInt32(b4), 1);

            var wa = writer.ToArray();
            for (int i = 0; i < test.Count; i++)
            {
                Assert.AreEqual(test[i], wa[i], $"Byte mismatch at position 0x{i:X}");
            }
        }
    }
}
