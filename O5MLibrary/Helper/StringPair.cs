using System.Text;

namespace O5M.Helper
{
    /// <summary>
    /// String pair.
    /// </summary>
    public static class StringPair
    {
        /// <summary>
        /// Parse the specified stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        public static string[] Parse(Stream stream)
        {
            var pair = new string[] { string.Empty, string.Empty };

            var readByte = stream.ReadByte();
            if (readByte == -1)
            {
                throw new IOException("Stream has preliminated end.");
            }
            if (readByte != 0)
            {
                throw new FormatException("String-Pair doesn't start with 0-byte.");
            }
            for (var i = 0; i < pair.Length; i++)
            {
                var bytes = new List<byte>();
                while (true)
                {
                    readByte = stream.ReadByte();
                    if (readByte < 1)
                    {
                        break;
                    }
                    bytes.Add((byte)readByte);
                }
                pair[i] = Encoding.UTF8.GetString(bytes.ToArray());
            }

            return pair;
        }

        /// <summary>
        /// Parse the specified bytes.
        /// </summary>
        /// <param name="bytes">Bytes.</param>
        public static KeyValuePair<string, string>? Parse(byte[] bytes)
        {
            int offset = 0;
            return Parse(bytes, ref offset);
        }

        /// <summary>
        /// Parse the specified bytes and offset.
        /// </summary>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static KeyValuePair<string, string>? Parse(byte[] bytes, ref int offset)
        {
            var keyValuePair = ParseToByteArrayPair(bytes, ref offset);

            if (keyValuePair.HasValue)
            {
                var key = Encoding.UTF8.GetString(keyValuePair.Value.Key);
                var value = Encoding.UTF8.GetString(keyValuePair.Value.Value);
                return new KeyValuePair<string, string>(key, value);
            }

            return null;
        }

        /// <summary>
        /// Parses to byte array pair.
        /// </summary>
        /// <returns>The to byte array pair.</returns>
        /// <param name="stream">Stream.</param>
        public static KeyValuePair<byte[], byte[]>? ParseToByteArrayPair(Stream stream)
        {
            var readByte = stream.ReadByte();
            if (readByte == -1)
            {
                throw new IOException("Stream has preliminated end.");
            }
            if (readByte != 0)
            {
                throw new FormatException("String-Pair-Stream doesn't start with 0-byte.");
            }

            var key = Array.Empty<byte>();
            var value = Array.Empty<byte>();
            for (var i = 0; i < 2; i++)
            {
                var bytes = new List<byte>();
                while (true)
                {
                    readByte = stream.ReadByte();
                    if (readByte < 1)
                    {
                        break;
                    }
                    bytes.Add((byte)readByte);
                }

                if (i == 0)
                {
                    key = bytes.ToArray();
                }
                else if (i == 1)
                {
                    value = bytes.ToArray();
                }
            }

            if (key.Length == 0)
            {
                return null;
            }

            return new KeyValuePair<byte[], byte[]>(key, value);
        }

        /// <summary>
        /// Parses to byte array pair.
        /// </summary>
        /// <returns>The to byte array pair.</returns>
        /// <param name="bytes">Bytes.</param>
        public static KeyValuePair<byte[], byte[]>? ParseToByteArrayPair(byte[] bytes)
        {
            int offset = 0;
            return ParseToByteArrayPair(bytes, ref offset);
        }

        /// <summary>
        /// Parses to byte array pair.
        /// </summary>
        /// <returns>The to byte array pair.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static KeyValuePair<byte[], byte[]>? ParseToByteArrayPair(byte[] bytes, ref int offset)
        {
            var newBytes = new byte[bytes.Length - offset];
            if (newBytes.Length < 3)
            {
                throw new FormatException("Byte-Array must have at least 3 byte.");
            }
            if (newBytes[0] != 0)
            {
                throw new FormatException("Byte-Array must start with 0-byte.");
            }

            Array.Copy(bytes, offset, newBytes, 0, bytes.Length - offset);

            var firstStopIndex = -1;
            var secondStopIndex = -1;
            for (var i = 1; i < newBytes.Length; i++)
            {
                if (newBytes[i] == 0)
                {
                    firstStopIndex = i;
                    break;
                }
            }
            if (firstStopIndex == -1)
            {
                throw new FormatException("Byte-Array must have a second 0-byte.");
            }
            for (var i = firstStopIndex + 1; i < newBytes.Length; i++)
            {
                if (newBytes[i] == 0)
                {
                    secondStopIndex = i;
                    break;
                }
            }
            if (secondStopIndex == -1)
            {
                throw new FormatException("Byte-Array must have a third 0-byte.");
            }

            var keyLength = firstStopIndex - 1;
            if (keyLength == 0)
            {
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
        /// <summary>
        /// Parses to UInt/String pair.
        /// </summary>
        /// <returns>The to user interface nt string.</returns>
        /// <param name="stream">Stream.</param>
        public static KeyValuePair<ulong, string>? ParseToUIntString(Stream stream)
        {
            var readByte = stream.ReadByte();
            if (readByte == -1)
            {
                throw new IOException("Stream has preliminated end.");
            }
            if (readByte != 0)
            {
                throw new FormatException("String-Pair-Stream doesn't start with 0-byte.");
            }

            var uid = 0UL;
            var name = string.Empty;
            for (var i = 0; i < 2; i++)
            {
                var bytes = new List<byte>();
                while (true)
                {
                    readByte = stream.ReadByte();
                    if (readByte < 1)
                    {
                        break;
                    }
                    bytes.Add((byte)readByte);
                }

                if (i == 0)
                {
                    uid = VarInt.ParseUInt64(bytes.ToArray());
                }
                else if (i == 1)
                {
                    name = Encoding.UTF8.GetString(bytes.ToArray());
                }
            }

            return new KeyValuePair<ulong, string>(uid, name);
        }

        /// <summary>
        /// Parses to UInt/String pair.
        /// </summary>
        /// <returns>The to user interface nt string.</returns>
        /// <param name="bytes">Bytes.</param>
        public static KeyValuePair<ulong, string>? ParseToUIntString(byte[] bytes)
        {
            int offset = 0;
            return ParseToUIntString(bytes, ref offset);
        }

        /// <summary>
        /// Parses to UInt/String pair.
        /// </summary>
        /// <returns>The to user interface nt string.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static KeyValuePair<ulong, string>? ParseToUIntString(byte[] bytes, ref int offset)
        {
            var keyValuePair = ParseToByteArrayPair(bytes, ref offset);

            if (keyValuePair.HasValue)
            {
                var uid = VarInt.ParseUInt64(keyValuePair.Value.Key);
                var name = Encoding.UTF8.GetString(keyValuePair.Value.Value);
                return new KeyValuePair<ulong, string>(uid, name);
            }

            return null;
        }
        /// <summary>
        /// Parses to type role byte array.
        /// </summary>
        /// <returns>The to type role byte array.</returns>
        /// <param name="bytes">Bytes.</param>
        public static KeyValuePair<byte[], byte[]>? ParseToTypeRoleByteArray(byte[] bytes)
        {
            int offset = 0;
            return ParseToTypeRoleByteArray(bytes, ref offset);
        }

        /// <summary>
        /// Parses to type role byte array.
        /// </summary>
        /// <returns>The to type role byte array.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static KeyValuePair<byte[], byte[]>? ParseToTypeRoleByteArray(byte[] bytes, ref int offset)
        {
            if (bytes[offset] != 0)
            {
                throw new FormatException("String-Pair-Bytes doesn't start with 0-byte.");
            }
            offset++;
            var typeBytes = new byte[] { bytes[offset] };
            offset++;
            var roleBytes = new List<byte>();
            for (var i = offset; i < bytes.Length; i++)
            {
                offset++;
                if (bytes[i] == 0)
                {
                    break;
                }
                roleBytes.Add(bytes[i]);
            }

            return new KeyValuePair<byte[], byte[]>(typeBytes, roleBytes.ToArray());
        }
    }
}
