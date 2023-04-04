using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using O5M.Helper;
using OSMDataPrimitives;

namespace O5M
{
    /// <summary>
    /// O5M Writer.
    /// </summary>
    public class O5MWriter : O5MBase, IDisposable
    {
        private bool _nodesStarted = false;
        private bool _waysStarted = false;
        private bool _relationsStarted = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:O5M.O5MWriter"/> class.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="header">Header.</param>
        public O5MWriter(string filename, O5MHeader header = O5MHeader.O5M2)
        {
            this.Init(File.OpenWrite(filename), header);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:O5M.O5MWriter"/> class.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="header">Header.</param>
        public O5MWriter(Stream stream, O5MHeader header = O5MHeader.O5M2)
        {
            this.Init(stream, header);
        }

        private void Init(Stream stream, O5MHeader header = O5MHeader.O5M2)
        {
            this._stream = stream;
            this._stream.Write(new byte[] { (byte)O5MFileByteMarker.StartByte, (byte)O5MFileByteMarker.Header }, 0, 2);

            var headerLengthBytes = VarintBitConverter.GetVarintBytes(4U);
            var headerBuffer = Encoding.UTF8.GetBytes(header.ToString().ToLower());
            this._stream.Write(headerLengthBytes, 0, headerLengthBytes.Length);
            this._stream.Write(headerBuffer, 0, headerBuffer.Length);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the <see cref="T:O5M.O5MWriter"/> is
        /// reclaimed by garbage collection.
        /// </summary>
        ~O5MWriter()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:O5M.O5MWriter"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="T:O5M.O5MWriter"/>. The
        /// <see cref="Dispose()"/> method leaves the <see cref="T:O5M.O5MWriter"/> in an unusable state. After calling
        /// <see cref="Dispose()"/>, you must release all references to the <see cref="T:O5M.O5MWriter"/> so the garbage
        /// collector can reclaim the memory that the <see cref="T:O5M.O5MWriter"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Should only be called by the public Dispose() method.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._stream != null)
                {
                    this._stream.Write(new byte[] { (byte)O5MFileByteMarker.EndByte}, 0, 1);
                    this._stream.Dispose();
                    this._stream = null;
                }
            }
        }

        /// <summary>
        /// Writes the timestamp.
        /// </summary>
        /// <param name="timestamp">Timestamp.</param>
        public void WriteTimestamp(DateTime timestamp)
        {
            this._stream?.Write(new byte[] { (byte)O5MFileByteMarker.FileTimestamp }, 0, 1);
            var unixTimestamp = (long)timestamp.Subtract(UNIX_START).TotalSeconds;
            var timestampBytes = VarintBitConverter.GetVarintBytes(unixTimestamp);
            var timestampBytesLength = VarintBitConverter.GetVarintBytes((ulong)timestampBytes.Length);
            this._stream?.Write(timestampBytesLength, 0, timestampBytesLength.Length);
            this._stream?.Write(timestampBytes, 0, timestampBytes.Length);
        }

        /// <summary>
        /// Writes the boundings.
        /// </summary>
        /// <param name="latitudeMin">Minimum Latitude.</param>
        /// <param name="latitudeMax">Maximum Latitude.</param>
        /// <param name="longitudeMin">Minimum Longitude.</param>
        /// <param name="longitudeMax">Maximum Longitude.</param>
        public void WriteBoundings(double latitudeMin, double latitudeMax, double longitudeMin, double longitudeMax)
        {
            this._stream?.Write(new byte[] { (byte)O5MFileByteMarker.BoundingBox }, 0, 1);

            var longitudeMinLongBytes = VarintBitConverter.GetVarintBytes((long)(longitudeMin * POINT_DIVIDER));
            var latitudeMinLongBytes = VarintBitConverter.GetVarintBytes((long)(latitudeMin * POINT_DIVIDER));
            var longitudeMaxLongBytes = VarintBitConverter.GetVarintBytes((long)(longitudeMax * POINT_DIVIDER));
            var latitudeMaxLongBytes = VarintBitConverter.GetVarintBytes((long)(latitudeMax * POINT_DIVIDER));
            var boundingBytesLength = VarintBitConverter.GetVarintBytes((ulong)(longitudeMinLongBytes.Length + latitudeMinLongBytes.Length + longitudeMaxLongBytes.Length + latitudeMaxLongBytes.Length));
            this._stream?.Write(boundingBytesLength, 0, boundingBytesLength.Length);
            this._stream?.Write(longitudeMinLongBytes, 0, longitudeMinLongBytes.Length);
            this._stream?.Write(latitudeMinLongBytes, 0, latitudeMinLongBytes.Length);
            this._stream?.Write(longitudeMaxLongBytes, 0, longitudeMaxLongBytes.Length);
            this._stream?.Write(latitudeMaxLongBytes, 0, latitudeMaxLongBytes.Length);
        }

        /// <summary>
        /// Writes the element.
        /// </summary>
        /// <param name="element">Element.</param>
        public void WriteElement(IOSMElement element)
        {
            if (element is OSMNode node)
            {
                this.WriteNode(node);
            }
            else if (element is OSMWay way)
            {
                this.WriteWay(way);
            }
            else if (element is OSMRelation relation)
            {
                this.WriteRelation(relation);
            }
        }

#if DEBUG
        /// <summary>
        /// Writes the element.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <param name="writtenData">Written data.</param>
        public void WriteElement(IOSMElement element, out byte[] writtenData)
        {
            writtenData = Array.Empty<byte>();
            if (element is OSMNode node)
            {
                this.WriteNode(node, out writtenData);
            }
            else if (element is OSMWay way)
            {
                this.WriteWay(way, out writtenData);
            }
            else if (element is OSMRelation relation)
            {
                this.WriteRelation(relation, out writtenData);
            }
        }
#endif

        private void WriteNode(OSMNode node)
        {
            var _ = Array.Empty<byte>();
            this.WriteNode(node, out _);
        }

        private void WriteNode(OSMNode node, out byte[] writtenData)
        {
            if (!this._nodesStarted)
            {
                this.Reset();
                this._stream?.Write(new byte[] { (byte)O5MFileByteMarker.Reset }, 0, 1);
                this._nodesStarted = true;
            }

            this._stream?.Write(new byte[] { (byte)O5MFileByteMarker.Node }, 0, 1);

            var bytes = new List<byte>();
            var diffId = (long)node.Id - this._lastNodeId;
            var idBytes = VarintBitConverter.GetVarintBytes(diffId);
            bytes.AddRange(idBytes);
            this._lastNodeId = (long)node.Id;

            this.WriteVersionData(node, bytes);

            var varIntLongitude = (int)Math.Round(node.Longitude * POINT_DIVIDER);
            var diffLongitude = varIntLongitude - this._lastLongitude;
            var longitudeBytes = VarintBitConverter.GetVarintBytes(diffLongitude);
            bytes.AddRange(longitudeBytes);
            this._lastLongitude = varIntLongitude;

            var varIntLatitude = (int)Math.Round(node.Latitude * POINT_DIVIDER);
            var diffLatitude = varIntLatitude - this._lastLatitude;
            var latitudeBytes = VarintBitConverter.GetVarintBytes(diffLatitude);
            bytes.AddRange(latitudeBytes);
            this._lastLatitude = varIntLatitude;

            this.WriteTags(node, bytes);

            var length = VarintBitConverter.GetVarintBytes((ulong)bytes.Count);
            this._stream?.Write(length, 0, length.Length);
            writtenData = bytes.ToArray();
            this._stream?.Write(writtenData, 0, bytes.Count);
        }

        private void WriteWay(OSMWay way)
        {
            byte[] _ = Array.Empty<byte>();
            this.WriteWay(way, out _);
        }

        private void WriteWay(OSMWay way, out byte[] writtenData)
        {
            if (!this._waysStarted)
            {
                this.Reset();
                this._stream?.Write(new byte[] { (byte)O5MFileByteMarker.Reset }, 0, 1);
                this._waysStarted = true;
            }

            this._stream?.Write(new byte[] { (byte)O5MFileByteMarker.Way }, 0, 1);

            var bytes = new List<byte>();
            var diffId = (long)way.Id - this._lastWayId;
            var idBytes = VarintBitConverter.GetVarintBytes(diffId);
            bytes.AddRange(idBytes);
            this._lastWayId = (long)way.Id;

            this.WriteVersionData(way, bytes);

            if (way.NodeRefs.Count == 0)
            {
                bytes.Add(0x00);
            }
            else
            {
                var nodeRefsBytes = new List<byte>();
                foreach (var nodeRef in way.NodeRefs)
                {
                    var nextReferenceDiff = (long)nodeRef - this._lastReferenceId;
                    var nodeRefBytes = VarintBitConverter.GetVarintBytes(nextReferenceDiff);
                    this._lastReferenceId = (long)nodeRef;
                    nodeRefsBytes.AddRange(nodeRefBytes);
                }
                var nodeRefsBytesCountBytes = VarintBitConverter.GetVarintBytes((ulong)nodeRefsBytes.Count);
                bytes.AddRange(nodeRefsBytesCountBytes);
                bytes.AddRange(nodeRefsBytes);
            }

            this.WriteTags(way, bytes);

            var length = VarintBitConverter.GetVarintBytes((ulong)bytes.Count);
            this._stream?.Write(length, 0, length.Length);
            writtenData = bytes.ToArray();
            this._stream?.Write(writtenData, 0, bytes.Count);
        }

        private void WriteRelation(OSMRelation relation)
        {
            byte[] _ = Array.Empty<byte>();
            this.WriteRelation(relation, out _);
        }

        private void WriteRelation(OSMRelation relation, out byte[] writtenData)
        {
            if (!this._relationsStarted)
            {
                this.Reset();
                this._stream?.Write(new byte[] { (byte)O5MFileByteMarker.Reset }, 0, 1);
                this._relationsStarted = true;
            }

            this._stream?.Write(new byte[] { (byte)O5MFileByteMarker.Relation }, 0, 1);

            var bytes = new List<byte>();
            var diffId = (long)relation.Id - this._lastRelationId;
            var idBytes = VarintBitConverter.GetVarintBytes(diffId);
            bytes.AddRange(idBytes);
            this._lastRelationId = (long)relation.Id;

            this.WriteVersionData(relation, bytes);

            if (relation.Members.Count == 0)
            {
                bytes.Add(0x00);
            }
            else
            {
                var membersBytes = new List<byte>();
                foreach (var member in relation.Members)
                {
                    var nextReferenceDiff = (long)member.Ref - this._lastReferenceId;
                    var memberRefBytes = VarintBitConverter.GetVarintBytes(nextReferenceDiff);
                    this._lastReferenceId = (long)member.Ref;
                    membersBytes.AddRange(memberRefBytes);

                    var typeBytes = new byte[] { (byte)((int)member.Type + 0x30) };
                    var roleBytes = Encoding.UTF8.GetBytes(member.Role);
                    var position = this._storedStringPairs.GetElementPosition(typeBytes, roleBytes);
                    if (position == -1)
                    {
                        membersBytes.Add(0x00);
                        membersBytes.AddRange(typeBytes);
                        membersBytes.AddRange(roleBytes);
                        membersBytes.Add(0x00);
                        if (1 + roleBytes.Length <= 250)
                        {
                            var typeAndRole = new KeyValuePair<byte[], byte[]>(typeBytes, roleBytes);
                            this._storedStringPairs.Insert(0, typeAndRole);
                        }
                    }
                    else
                    {
                        var positionBytes = VarintBitConverter.GetVarintBytes((uint)position);
                        membersBytes.AddRange(positionBytes);
                    }
                }
                var membersBytesCountBytes = VarintBitConverter.GetVarintBytes((ulong)membersBytes.Count);
                bytes.AddRange(membersBytesCountBytes);
                bytes.AddRange(membersBytes);
            }

            this.WriteTags(relation, bytes);

            var length = VarintBitConverter.GetVarintBytes((ulong)bytes.Count);
            this._stream?.Write(length, 0, length.Length);
            writtenData = bytes.ToArray();
            this._stream?.Write(writtenData, 0, bytes.Count);
        }

        private void WriteVersionData(IOSMElement element, List<byte> bytes)
        {
            if (element.Version == 0)
            {
                bytes.Add(0x00);
                return;
            }

            var versionBytes = VarintBitConverter.GetVarintBytes(element.Version);
            bytes.AddRange(versionBytes);

            var timestamp = (long)element.Timestamp.Subtract(UNIX_START).TotalSeconds;
            var timestampDiff = timestamp - this._lastTimestamp;
            var timestampBytes = VarintBitConverter.GetVarintBytes(timestampDiff);
            this._lastTimestamp = timestamp;
            bytes.AddRange(timestampBytes);

            if (timestamp == 0)
            {
                return;
            }

            var changesetDiff = (long)element.Changeset - this._lastChangeset;
            var changesetBytes = VarintBitConverter.GetVarintBytes(changesetDiff);
            this._lastChangeset = (long)element.Changeset;
            bytes.AddRange(changesetBytes);

            var uidBytes = VarintBitConverter.GetVarintBytes(element.UserId);
            if (element.UserId == 0)
            {
                uidBytes = Array.Empty<byte>();
            }
            var userBytes = Encoding.UTF8.GetBytes(element.UserName);
            var elementPosition = this._storedStringPairs.GetElementPosition(uidBytes, userBytes);
            if (elementPosition != -1)
            {
                var positionBytes = VarintBitConverter.GetVarintBytes((uint)elementPosition);
                bytes.AddRange(positionBytes);
            }
            else
            {
                bytes.Add(0x00);
                bytes.AddRange(uidBytes);
                bytes.Add(0x00);
                bytes.AddRange(userBytes);
                bytes.Add(0x00);
                var uidUserPair = new KeyValuePair<byte[], byte[]>(uidBytes, userBytes);
                if (uidUserPair.Key.Length + uidUserPair.Value.Length <= 250)
                {
                    this._storedStringPairs.Insert(0, uidUserPair);
                }
            }
        }

        private void WriteTags(IOSMElement element, List<byte> bytes)
        {
            if (element.Tags.Count == 0)
            {
                return;
            }

            foreach (string tagKey in element.Tags)
            {
                var tagKeyBytes = Encoding.UTF8.GetBytes(tagKey);
                if (element.Tags[tagKey] == null)
                {
                    continue;
                }
                var tagValueBytes = Encoding.UTF8.GetBytes(element.Tags[tagKey]!);
                var elementPosition = this._storedStringPairs.GetElementPosition(tagKeyBytes, tagValueBytes);
                if (elementPosition != -1)
                {
                    var positionBytes = VarintBitConverter.GetVarintBytes((uint)elementPosition);
                    bytes.AddRange(positionBytes);
                }
                else
                {
                    var stringPair = new KeyValuePair<byte[], byte[]>(tagKeyBytes, tagValueBytes);
                    bytes.Add(0x00);
                    bytes.AddRange(tagKeyBytes);
                    bytes.Add(0x00);
                    bytes.AddRange(tagValueBytes);
                    bytes.Add(0x00);
                    if (stringPair.Key.Length + stringPair.Value.Length <= 250)
                    {
                        this._storedStringPairs.Insert(0, stringPair);
                    }
                }
            }
        }
    }
}
