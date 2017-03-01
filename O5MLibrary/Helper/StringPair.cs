using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace O5M.Helper
{
	public static class StringPair
	{
		public static string[] Parse(Stream stream)
		{
			var pair = new string[] { string.Empty, string.Empty };

			var readByte = stream.ReadByte();
			if(readByte == -1) {
				throw new IOException("Stream has preliminated end.");
			}
			if(readByte != 0) {
				throw new FormatException("String-Pair doesn't start with 0-byte.");
			}
			for(var i = 0; i < pair.Length; i++) {
				var repeat = true;
				var bytes = new List<byte>();
				while(repeat) {
					readByte = stream.ReadByte();
					if(readByte < 1) {
						repeat = false;
						break;
					}
					bytes.Add((byte)readByte);
				}
				pair[i] = Encoding.UTF8.GetString(bytes.ToArray());
			}

			return pair;
		}

		public static KeyValuePair<string, string>? Parse(byte[] bytes)
		{
			int offset = 0;
			return Parse(bytes, ref offset);
		}

		public static KeyValuePair<string, string>? Parse(byte[] bytes, ref int offset)
		{
			var keyValuePair = ParseToByteArrayPair(bytes, ref offset);

			var key = Encoding.UTF8.GetString(keyValuePair?.Key);
			var value = Encoding.UTF8.GetString(keyValuePair?.Value);
			return new KeyValuePair<string, string>(key, value);
		}

		public static KeyValuePair<byte[], byte[]>? ParseToByteArrayPair(Stream stream)
		{
			var readByte = stream.ReadByte();
			if(readByte == -1) {
				throw new IOException("Stream has preliminated end.");
			}
			if(readByte != 0) {
				throw new FormatException("String-Pair-Stream doesn't start with 0-byte.");
			}

			var key = new byte[0];
			var value = new byte[0];
			for(var i = 0; i < 2; i++) {
				var repeat = true;
				var bytes = new List<byte>();
				while(repeat) {
					readByte = stream.ReadByte();
					if(readByte < 1) {
						repeat = false;
						break;
					}
					bytes.Add((byte)readByte);
				}

				if(i == 0) {
					key = bytes.ToArray();
				} else if(i == 1) {
					value = bytes.ToArray();
				}
			}

			if(key.Length == 0) {
				return null;
			}

			return new KeyValuePair<byte[], byte[]>(key, value);
		}

		public static KeyValuePair<byte[], byte[]>? ParseToByteArrayPair(byte[] bytes)
		{
			int offset = 0;
			return ParseToByteArrayPair(bytes, ref offset);
		}

		public static KeyValuePair<byte[], byte[]>? ParseToByteArrayPair(byte[] bytes, ref int offset)
		{
			var newBytes = new byte[bytes.Length - offset];
			if(newBytes.Length < 3) {
				throw new FormatException("Byte-Array must have at least 3 byte.");
			}
			if(newBytes[0] != 0) {
				throw new FormatException("Byte-Array must start with 0-byte.");
			}

			Array.Copy(bytes, offset, newBytes, 0, bytes.Length - offset);

			var firstStopIndex = -1;
			var secondStopIndex = -1;
			for(var i = 1; i < newBytes.Length; i++) {
				if(newBytes[i] == 0) {
					firstStopIndex = i;
					break;
				}
			}
			if(firstStopIndex == -1) {
				throw new FormatException("Byte-Array must have a second 0-byte.");
			}
			for(var i = firstStopIndex + 1; i < newBytes.Length; i++) {
				if(newBytes[i] == 0) {
					secondStopIndex = i;
					break;
				}
			}
			if(secondStopIndex == -1) {
				throw new FormatException("Byte-Array must have a third 0-byte.");
			}

			var keyLength = firstStopIndex - 1;
			if(keyLength == 0) {
				offset += secondStopIndex + 1;
				return null;
			}

			var keyBytes = new byte[keyLength];
			var valueLength = secondStopIndex - firstStopIndex - 1;
			var valueBytes = new byte[valueLength];
			Array.Copy(newBytes, 1, keyBytes, 0, keyLength);
			Array.Copy(newBytes, firstStopIndex + 1, valueBytes, 0, valueLength);

			offset += secondStopIndex + 1;

			return new KeyValuePair<byte[], byte[]>(keyBytes, valueBytes);
		}

		public static KeyValuePair<ulong, string>? ParseToUIntString(Stream stream)
		{
			var readByte = stream.ReadByte();
			if(readByte == -1) {
				throw new IOException("Stream has preliminated end.");
			}
			if(readByte != 0) {
				throw new FormatException("String-Pair-Stream doesn't start with 0-byte.");
			}

			var uid = 0UL;
			var name = string.Empty;
			for(var i = 0; i < 2; i++) {
				var repeat = true;
				var bytes = new List<byte>();
				while(repeat) {
					readByte = stream.ReadByte();
					if(readByte < 1) {
						repeat = false;
						break;
					}
					bytes.Add((byte)readByte);
				}

				if(i == 0) {
					uid = VarInt.ParseUInt64(bytes.ToArray());
				} else if(i == 1) {
					name = Encoding.UTF8.GetString(bytes.ToArray());
				}
			}

			return new KeyValuePair<ulong, string>(uid, name);
		}

		public static KeyValuePair<ulong, string>? ParseToUIntString(byte[] bytes)
		{
			int offset = 0;
			return ParseToUIntString(bytes, ref offset);
		}

		public static KeyValuePair<ulong, string>? ParseToUIntString(byte[] bytes, ref int offset)
		{
			var keyValuePair = ParseToByteArrayPair(bytes, ref offset);

			var uid = VarInt.ParseUInt64(keyValuePair?.Key);
			var name = Encoding.UTF8.GetString(keyValuePair?.Value);
			return new KeyValuePair<ulong, string>(uid, name);
		}

		public static KeyValuePair<byte[], byte[]>? ParseToTypeRoleByteArray(byte[] bytes)
		{
			int offset = 0;
			return ParseToTypeRoleByteArray(bytes, ref offset);
		}

		public static KeyValuePair<byte[], byte[]>? ParseToTypeRoleByteArray(byte[] bytes, ref int offset)
		{
			if(bytes[offset] != 0) {
				throw new FormatException("String-Pair-Bytes doesn't start with 0-byte.");
			}
			offset++;
			var typeBytes = new byte[] { bytes[offset] };
			offset++;
			var roleBytes = new List<byte>();
			for(var i = offset; i < bytes.Length; i++) {
				offset++;
				if(bytes[i] == 0) {
					break;
				}
				roleBytes.Add(bytes[i]);
			}

			return new KeyValuePair<byte[], byte[]>(typeBytes, roleBytes.ToArray());
		}
	}
}
