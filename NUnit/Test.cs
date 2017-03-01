using System.IO;
using O5M.Helper;
using NUnit.Framework;

namespace NUnit
{
	[TestFixture]
	public class Test
	{
		[Test]
		public void TestVarIntParseInt64()
		{
			using(var memoryStream = new MemoryStream()) {
				memoryStream.WriteByte(0xAC);
				memoryStream.WriteByte(0x02);
				memoryStream.Position = 0;
				var result = VarInt.ParseUInt64(memoryStream);
				Assert.AreEqual(300, result);

				memoryStream.Position = 0;
				memoryStream.WriteByte(0x96);
				memoryStream.WriteByte(0x01);
				memoryStream.Position = 0;
				result = VarInt.ParseUInt64(memoryStream);
				Assert.AreEqual(150, result);
			}
		}

		[Test]
		public void TestUIntString()
		{
			using(var memoryStream = new MemoryStream()) {
				memoryStream.WriteByte(0x00);
				memoryStream.WriteByte(0x85);
				memoryStream.WriteByte(0xe3);
				memoryStream.WriteByte(0x02);
				memoryStream.WriteByte(0x00);
				memoryStream.WriteByte(0x55);
				memoryStream.WriteByte(0x53);
				memoryStream.WriteByte(0x63);
				memoryStream.WriteByte(0x68);
				memoryStream.WriteByte(0x61);
				memoryStream.WriteByte(0x00);
				memoryStream.Position = 0;
				var result = StringPair.ParseToUIntString(memoryStream);
				Assert.AreEqual(45445, result?.Key);
				Assert.AreEqual("UScha", result?.Value);
			}
		}
	}
}
