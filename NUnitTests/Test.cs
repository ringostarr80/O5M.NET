using System.IO;
using NUnit.Framework;
using O5M.Helper;

namespace NUnit
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void TestVarIntParseInt64()
        {
            using var memoryStream = new MemoryStream();
            memoryStream.Write(new byte[] { 0xAC, 0x02 }, 0, 2);
            memoryStream.Position = 0;
            var result = VarInt.ParseUInt64(memoryStream);
            Assert.That(result, Is.EqualTo(300));

            memoryStream.Position = 0;
            memoryStream.Write(new byte[] { 0x96, 0x01 }, 0, 2);
            memoryStream.Position = 0;
            result = VarInt.ParseUInt64(memoryStream);
            Assert.That(result, Is.EqualTo(150));
        }

        [Test]
        public void TestUIntString()
        {
			var data = new byte[] { 0x00, 0x85, 0xe3, 0x02, 0x00, 0x55, 0x53, 0x63, 0x68, 0x61, 0x00 };
            using var memoryStream = new MemoryStream();
            memoryStream.Write(data, 0, data.Length);
            memoryStream.Position = 0;
            var result = StringPair.ParseToUIntString(memoryStream);
            Assert.That(result?.Key, Is.EqualTo(45445));
            Assert.That(result?.Value, Is.EqualTo("UScha"));
        }
    }
}
