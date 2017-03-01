using System;
using System.Collections.Generic;
using System.IO;

namespace O5M.Helper
{
	public static class VarInt
	{
		private static byte[] ParseStreamToByteArray(Stream stream)
		{
			var continueParsing = false;
			var bytes = new List<byte>();
			do {
				var currentByte = stream.ReadByte();
				if(currentByte == -1) {
					break;
				}
				continueParsing = ((currentByte & 0x80) > 0);
				bytes.Add((byte)currentByte);
			} while(continueParsing);

			return bytes.ToArray();
		}

		public static short ParseInt16(Stream stream)
		{
			var byteArray = ParseStreamToByteArray(stream);
			return VarintBitConverter.ToInt16(byteArray);
		}

		public static int ParseInt32(Stream stream)
		{
			var byteArray = ParseStreamToByteArray(stream);
			return VarintBitConverter.ToInt32(byteArray);
		}

		public static long ParseInt64(Stream stream)
		{
			var byteArray = ParseStreamToByteArray(stream);
			return VarintBitConverter.ToInt64(byteArray);
		}

		public static short ParseInt16(byte[] bytes)
		{
			return VarintBitConverter.ToInt16(bytes);
		}

		public static int ParseInt32(byte[] bytes)
		{
			return VarintBitConverter.ToInt32(bytes);
		}

		public static long ParseInt64(byte[] bytes)
		{
			return VarintBitConverter.ToInt64(bytes);
		}

		public static short ParseInt16(byte[] bytes, ref int offset)
		{
			var readBytes = 0;
			var newBytes = new byte[bytes.Length - offset];
			Array.Copy(bytes, offset, newBytes, 0, newBytes.Length);
			var shortResult = VarintBitConverter.ToInt16(newBytes, out readBytes);
			offset += readBytes;

			return shortResult;
		}

		public static int ParseInt32(byte[] bytes, ref int offset)
		{
			var readBytes = 0;
			var newBytes = new byte[bytes.Length - offset];
			Array.Copy(bytes, offset, newBytes, 0, newBytes.Length);
			var intResult = VarintBitConverter.ToInt32(newBytes, out readBytes);
			offset += readBytes;

			return intResult;
		}

		public static long ParseInt64(byte[] bytes, ref int offset)
		{
			var readBytes = 0;
			var newBytes = new byte[bytes.Length - offset];
			Array.Copy(bytes, offset, newBytes, 0, newBytes.Length);
			var longResult = VarintBitConverter.ToInt64(newBytes, out readBytes);
			offset += readBytes;

			return longResult;
		}

		public static ushort ParseUInt16(Stream stream)
		{
			var byteArray = ParseStreamToByteArray(stream);
			return VarintBitConverter.ToUInt16(byteArray);
		}

		public static uint ParseUInt32(Stream stream)
		{
			var byteArray = ParseStreamToByteArray(stream);
			return VarintBitConverter.ToUInt32(byteArray);
		}

		public static ulong ParseUInt64(Stream stream)
		{
			var byteArray = ParseStreamToByteArray(stream);
			return VarintBitConverter.ToUInt64(byteArray);
		}

		public static ushort ParseUInt16(byte[] bytes)
		{
			return VarintBitConverter.ToUInt16(bytes);
		}

		public static uint ParseUInt32(byte[] bytes)
		{
			return VarintBitConverter.ToUInt32(bytes);
		}

		public static ulong ParseUInt64(byte[] bytes)
		{
			return VarintBitConverter.ToUInt64(bytes);
		}

		public static ushort ParseUInt16(byte[] bytes, ref int offset)
		{
			var readBytes = 0;
			var newBytes = new byte[bytes.Length - offset];
			Array.Copy(bytes, offset, newBytes, 0, bytes.Length - offset);
			var ushortResult = VarintBitConverter.ToUInt16(newBytes, out readBytes);
			offset += readBytes;

			return ushortResult;
		}

		public static uint ParseUInt32(byte[] bytes, ref int offset)
		{
			var readBytes = 0;
			var newBytes = new byte[bytes.Length - offset];
			Array.Copy(bytes, offset, newBytes, 0, bytes.Length - offset);
			var uintResult = VarintBitConverter.ToUInt32(newBytes, out readBytes);
			offset += readBytes;

			return uintResult;
		}

		public static ulong ParseUInt64(byte[] bytes, ref int offset)
		{
			var readBytes = 0;
			var newBytes = new byte[bytes.Length - offset];
			Array.Copy(bytes, offset, newBytes, 0, bytes.Length - offset);
			var ulongResult = VarintBitConverter.ToUInt64(newBytes, out readBytes);
			offset += readBytes;

			return ulongResult;
		}

		public static double ParseInt64AsDouble(Stream stream, int precision)
		{
			var longResult = ParseInt64(stream);
			return longResult / Math.Pow(10, precision);
		}

		public static double ParseInt64AsDouble(byte[] bytes, int precision)
		{
			var longResult = ParseInt64(bytes);
			return longResult / Math.Pow(10, precision);
		}

		public static double ParseInt64AsDouble(byte[] bytes, int precision, ref int offset)
		{
			var longResult = ParseInt64(bytes, ref offset);
			return longResult / Math.Pow(10, precision);
		}
	}
}
