using System;
using System.Collections.Generic;
using System.IO;

namespace O5M.Helper
{
    /// <summary>
    /// Variable int.
    /// </summary>
    public static class VarInt
    {
        private static byte[] ParseStreamToByteArray(Stream stream)
        {
            var bytes = new List<byte>();
            bool continueParsing;
            do
            {
                var currentByte = stream.ReadByte();
                if (currentByte == -1)
                {
                    break;
                }
                continueParsing = ((currentByte & 0x80) > 0);
                bytes.Add((byte)currentByte);
            } while (continueParsing);

            return bytes.ToArray();
        }

        /// <summary>
        /// Parses the stream to Int16.
        /// </summary>
        /// <returns>The int16.</returns>
        /// <param name="stream">Stream.</param>
        public static short ParseInt16(Stream stream)
        {
            var byteArray = ParseStreamToByteArray(stream);
            return VarintBitConverter.ToInt16(byteArray);
        }

        /// <summary>
        /// Parses the stream to Int32.
        /// </summary>
        /// <returns>The int32.</returns>
        /// <param name="stream">Stream.</param>
        public static int ParseInt32(Stream stream)
        {
            var byteArray = ParseStreamToByteArray(stream);
            return VarintBitConverter.ToInt32(byteArray);
        }

        /// <summary>
        /// Parses the stream to Int64.
        /// </summary>
        /// <returns>The int64.</returns>
        /// <param name="stream">Stream.</param>
        public static long ParseInt64(Stream stream)
        {
            var byteArray = ParseStreamToByteArray(stream);
            return VarintBitConverter.ToInt64(byteArray);
        }

        /// <summary>
        /// Parses the byte-array to Int16.
        /// </summary>
        /// <returns>The int16.</returns>
        /// <param name="bytes">Bytes.</param>
        public static short ParseInt16(byte[] bytes)
        {
            return VarintBitConverter.ToInt16(bytes);
        }

        /// <summary>
        /// Parses the byte-array to Int32.
        /// </summary>
        /// <returns>The int32.</returns>
        /// <param name="bytes">Bytes.</param>
        public static int ParseInt32(byte[] bytes)
        {
            return VarintBitConverter.ToInt32(bytes);
        }

        /// <summary>
        /// Parses the byte-array to Int64.
        /// </summary>
        /// <returns>The int64.</returns>
        /// <param name="bytes">Bytes.</param>
        public static long ParseInt64(byte[] bytes)
        {
            return VarintBitConverter.ToInt64(bytes);
        }

        /// <summary>
        /// Parses the byte-array to Int16.
        /// </summary>
        /// <returns>The int16.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static short ParseInt16(byte[] bytes, ref int offset)
        {
            var newBytes = new byte[bytes.Length - offset];
            Array.Copy(bytes, offset, newBytes, 0, newBytes.Length);
            var shortResult = VarintBitConverter.ToInt16(newBytes, out int readBytes);
            offset += readBytes;

            return shortResult;
        }

        /// <summary>
        /// Parses the byte-array to Int32.
        /// </summary>
        /// <returns>The int32.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static int ParseInt32(byte[] bytes, ref int offset)
        {
            var newBytes = new byte[bytes.Length - offset];
            Array.Copy(bytes, offset, newBytes, 0, newBytes.Length);
            var intResult = VarintBitConverter.ToInt32(newBytes, out int readBytes);
            offset += readBytes;

            return intResult;
        }

        /// <summary>
        /// Parses the byte-array to Int64.
        /// </summary>
        /// <returns>The int64.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static long ParseInt64(byte[] bytes, ref int offset)
        {
            var newBytes = new byte[bytes.Length - offset];
            Array.Copy(bytes, offset, newBytes, 0, newBytes.Length);
            var longResult = VarintBitConverter.ToInt64(newBytes, out int readBytes);
            offset += readBytes;

            return longResult;
        }

        /// <summary>
        /// Parses the stream to UInt16.
        /// </summary>
        /// <returns>The user interface nt16.</returns>
        /// <param name="stream">Stream.</param>
        public static ushort ParseUInt16(Stream stream)
        {
            var byteArray = ParseStreamToByteArray(stream);
            return VarintBitConverter.ToUInt16(byteArray);
        }

        /// <summary>
        /// Parses the stream to UInt32.
        /// </summary>
        /// <returns>The user interface nt32.</returns>
        /// <param name="stream">Stream.</param>
        public static uint ParseUInt32(Stream stream)
        {
            var byteArray = ParseStreamToByteArray(stream);
            return VarintBitConverter.ToUInt32(byteArray);
        }

        /// <summary>
        /// Parses the stream to UInt64.
        /// </summary>
        /// <returns>The user interface nt64.</returns>
        /// <param name="stream">Stream.</param>
        public static ulong ParseUInt64(Stream stream)
        {
            var byteArray = ParseStreamToByteArray(stream);
            return VarintBitConverter.ToUInt64(byteArray);
        }

        /// <summary>
        /// Parses the byte-array to UInt16.
        /// </summary>
        /// <returns>The user interface nt16.</returns>
        /// <param name="bytes">Bytes.</param>
        public static ushort ParseUInt16(byte[] bytes)
        {
            return VarintBitConverter.ToUInt16(bytes);
        }

        /// <summary>
        /// Parses the byte-array to Int32.
        /// </summary>
        /// <returns>The user interface nt32.</returns>
        /// <param name="bytes">Bytes.</param>
        public static uint ParseUInt32(byte[] bytes)
        {
            return VarintBitConverter.ToUInt32(bytes);
        }

        /// <summary>
        /// Parses the byte-array to UInt64.
        /// </summary>
        /// <returns>The user interface nt64.</returns>
        /// <param name="bytes">Bytes.</param>
        public static ulong ParseUInt64(byte[] bytes)
        {
            return VarintBitConverter.ToUInt64(bytes);
        }

        /// <summary>
        /// Parses the byte-array to UInt16.
        /// </summary>
        /// <returns>The user interface nt16.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static ushort ParseUInt16(byte[] bytes, ref int offset)
        {
            var newBytes = new byte[bytes.Length - offset];
            Array.Copy(bytes, offset, newBytes, 0, bytes.Length - offset);
            var ushortResult = VarintBitConverter.ToUInt16(newBytes, out int readBytes);
            offset += readBytes;

            return ushortResult;
        }

        /// <summary>
        /// Parses the byte-array to UInt32.
        /// </summary>
        /// <returns>The user interface nt32.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static uint ParseUInt32(byte[] bytes, ref int offset)
        {
            var newBytes = new byte[bytes.Length - offset];
            Array.Copy(bytes, offset, newBytes, 0, bytes.Length - offset);
            var uintResult = VarintBitConverter.ToUInt32(newBytes, out int readBytes);
            offset += readBytes;

            return uintResult;
        }

        /// <summary>
        /// Parses the byte-array to UInt64.
        /// </summary>
        /// <returns>The user interface nt64.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="offset">Offset.</param>
        public static ulong ParseUInt64(byte[] bytes, ref int offset)
        {
            var newBytes = new byte[bytes.Length - offset];
            Array.Copy(bytes, offset, newBytes, 0, bytes.Length - offset);
            var ulongResult = VarintBitConverter.ToUInt64(newBytes, out int readBytes);
            offset += readBytes;

            return ulongResult;
        }

        /// <summary>
        /// Parses the Int64 as Double.
        /// </summary>
        /// <returns>The int64 as double.</returns>
        /// <param name="stream">Stream.</param>
        /// <param name="precision">Precision.</param>
        public static double ParseInt64AsDouble(Stream stream, int precision)
        {
            var longResult = ParseInt64(stream);
            return longResult / Math.Pow(10, precision);
        }

        /// <summary>
        /// Parses the Int64 as Double.
        /// </summary>
        /// <returns>The int64 as double.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="precision">Precision.</param>
        public static double ParseInt64AsDouble(byte[] bytes, int precision)
        {
            var longResult = ParseInt64(bytes);
            return longResult / Math.Pow(10, precision);
        }

        /// <summary>
        /// Parses the Int64 as Double.
        /// </summary>
        /// <returns>The int64 as double.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="precision">Precision.</param>
        /// <param name="offset">Offset.</param>
        public static double ParseInt64AsDouble(byte[] bytes, int precision, ref int offset)
        {
            var longResult = ParseInt64(bytes, ref offset);
            return longResult / Math.Pow(10, precision);
        }
    }
}
