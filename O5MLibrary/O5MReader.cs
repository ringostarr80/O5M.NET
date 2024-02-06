using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using O5M.Helper;
using OSMDataPrimitives;

namespace O5M
{
    /// <summary>
    /// O5M Reader.
    /// </summary>
    public class O5MReader : O5MBase, IDisposable
    {
        private string _header = string.Empty;
        private DateTime? _fileTimestamp = null;
        private double _latitudeMin = 0.0;
        private double _latitudeMax = 0.0;
        private double _longitudeMin = 0.0;
        private double _longitudeMax = 0.0;
        private bool _skipNodes = false;
        private bool _skipWays = false;
        private bool _skipRelations = false;
        private bool _stop = false;

        /// <summary>
        /// Gets the O5M header string.
        /// </summary>
        /// <value>The header.</value>
        public string Header { get { return this._header; } }
        /// <summary>
        /// Gets the O5M file timestamp.
        /// </summary>
        /// <value>The file timestamp.</value>
        public DateTime? FileTimestamp { get { return this._fileTimestamp; } }
        /// <summary>
        /// Gets the minimum latitude of the O5M data.
        /// </summary>
        /// <value>The latitude minimum.</value>
        public double LatitudeMin { get { return this._latitudeMin; } }
        /// <summary>
        /// Gets the maximum latitude of the O5M data.
        /// </summary>
        /// <value>The latitude max.</value>
        public double LatitudeMax { get { return this._latitudeMax; } }
        /// <summary>
        /// Gets the minimum longitude of the O5M data.
        /// </summary>
        /// <value>The longitude minimum.</value>
        public double LongitudeMin { get { return this._longitudeMin; } }
        /// <summary>
        /// Gets the maximum longitude of the O5M data.
        /// </summary>
        /// <value>The longitude max.</value>
        public double LongitudeMax { get { return this._longitudeMax; } }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:O5M.O5MReader"/> skips all nodes.
        /// </summary>
        /// <value><c>true</c> if skip nodes; otherwise, <c>false</c>.</value>
        public bool SkipNodes { get { return this._skipNodes; } set { this._skipNodes = value; } }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:O5M.O5MReader"/> skips all ways.
        /// </summary>
        /// <value><c>true</c> if skip ways; otherwise, <c>false</c>.</value>
        public bool SkipWays { get { return this._skipWays; } set { this._skipWays = value; } }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:O5M.O5MReader"/> skips all relations.
        /// </summary>
        /// <value><c>true</c> if skip relations; otherwise, <c>false</c>.</value>
        public bool SkipRelations { get { return this._skipRelations; } set { this._skipRelations = value; } }

        /// <summary>
        /// The found node action.
        /// </summary>
        public Action<OSMNode> FoundNode;
        /// <summary>
        /// The found way action.
        /// </summary>
        public Action<OSMWay> FoundWay;
        /// <summary>
        /// The found relation action.
        /// </summary>
        public Action<OSMRelation> FoundRelation;
#if DEBUG
        /// <summary>
        /// The found node raw action.
        /// </summary>
        public Action<OSMNode, byte[], ElementDebugInfos> FoundNodeRaw;
        /// <summary>
        /// The found way raw action.
        /// </summary>
        public Action<OSMWay, byte[], ElementDebugInfos> FoundWayRaw;
        /// <summary>
        /// The found relation raw action.
        /// </summary>
        public Action<OSMRelation, byte[], ElementDebugInfos> FoundRelationRaw;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="T:O5M.O5MReader"/> class.
        /// </summary>
        /// <param name="filename">Filename.</param>
        public O5MReader(string filename)
        {
            if (!filename.EndsWith(".o5m", StringComparison.InvariantCulture))
            {
                throw new ArgumentException("The given filename is not a .o5m file!", nameof(filename));
            }
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("file (" + filename + ") not found!", filename);
            }

            this.Init(File.OpenRead(filename));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:O5M.O5MReader"/> class.
        /// </summary>
        /// <param name="stream">Stream.</param>
        public O5MReader(Stream stream)
        {
            this.Init(stream);
        }

        private void Init(Stream stream)
        {
            this._stream = stream;
            var markerByte = this._stream.ReadByte();
            if (markerByte != (int)O5MFileByteMarker.StartByte)
            {
                throw new FormatException("The .o5m-file has an invalid first-byte!");
            }

            markerByte = this._stream.ReadByte();
            if (markerByte == -1)
            {
                throw new FormatException("Cannot read further to the o5m-Header Content!");
            }
            var marker = (O5MFileByteMarker)markerByte;
            if (marker != O5MFileByteMarker.Header)
            {
                throw new FormatException("o5m-Header-Marker expected!");
            }

            var length = VarInt.ParseUInt64(this._stream);
            var headerBuffer = new byte[length];
            var headerBytesRead = this._stream.Read(headerBuffer, 0, headerBuffer.Length);
            if (headerBytesRead != headerBuffer.Length)
            {
                throw new FormatException("The .o5m-file has an invalid header-length!");
            }
            this._header = Encoding.UTF8.GetString(headerBuffer, 0, headerBuffer.Length);

            this.TryParseFileTimestamp(this._stream);
            this.TryParseBoundingBox(this._stream);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the <see cref="T:O5M.O5MReader"/> is
        /// reclaimed by garbage collection.
        /// </summary>
        ~O5MReader()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:O5M.O5MReader"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="T:O5M.O5MReader"/>. The
        /// <see cref="Dispose()"/> method leaves the <see cref="T:O5M.O5MReader"/> in an unusable state. After calling
        /// <see cref="Dispose()"/>, you must release all references to the <see cref="T:O5M.O5MReader"/> so the garbage
        /// collector can reclaim the memory that the <see cref="T:O5M.O5MReader"/> was occupying.</remarks>
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
					this._stream.Dispose();
					this._stream = null;
				}
			}
		}

        /// <summary>
        /// On node found.
        /// </summary>
        /// <param name="node">Node.</param>
        protected void OnNodeFound(OSMNode node)
        {
            this.FoundNode?.Invoke(node);
        }

#if DEBUG
        /// <summary>
        /// On node found.
        /// </summary>
        /// <param name="node">Node.</param>
        /// <param name="rawData">Raw data.</param>
        /// <param name="infos">Infos.</param>
        protected void OnNodeFound(OSMNode node, byte[] rawData, ElementDebugInfos infos)
        {
            this.FoundNodeRaw?.Invoke(node, rawData, infos);
        }
#endif

        /// <summary>
        /// On way found.
        /// </summary>
        /// <param name="way">Way.</param>
        protected void OnWayFound(OSMWay way)
        {
            this.FoundWay?.Invoke(way);
        }

#if DEBUG
        /// <summary>
        /// On way found.
        /// </summary>
        /// <param name="way">Way.</param>
        /// <param name="rawData">Raw data.</param>
        /// <param name="infos">Infos.</param>
        protected void OnWayFound(OSMWay way, byte[] rawData, ElementDebugInfos infos)
        {
            this.FoundWayRaw?.Invoke(way, rawData, infos);
        }
#endif

        /// <summary>
        /// On relation found.
        /// </summary>
        /// <param name="relation">Relation.</param>
        protected void OnRelationFound(OSMRelation relation)
        {
            this.FoundRelation?.Invoke(relation);
        }

#if DEBUG
        /// <summary>
        /// On relation found.
        /// </summary>
        /// <param name="relation">Relation.</param>
        /// <param name="rawData">Raw data.</param>
        /// <param name="infos">Infos.</param>
        protected void OnRelationFound(OSMRelation relation, byte[] rawData, ElementDebugInfos infos)
        {
            this.FoundRelationRaw?.Invoke(relation, rawData, infos);
        }
#endif

        private void TryParseFileTimestamp(Stream stream)
        {
            var markerByte = this._stream!.ReadByte();
            if (markerByte == -1)
            {
                throw new FormatException("Cannot read further to the o5m-file!");
            }
            var marker = (O5MFileByteMarker)markerByte;
            if (marker != O5MFileByteMarker.FileTimestamp)
            {
                stream.Position -= 1;
                return;
            }

            var length = VarInt.ParseUInt64(stream);
            var buffer = new byte[length];
            var readBytes = this._stream!.Read(buffer, 0, buffer.Length);
            if (readBytes != buffer.Length)
            {
                throw new FormatException("The .o5m-file has an invalid timestamp-length!");
            }
            var timestamp = VarInt.ParseInt64(buffer);
            this._fileTimestamp = UNIX_START.AddSeconds(timestamp);
        }

        private void TryParseBoundingBox(Stream stream)
        {
            var markerByte = this._stream!.ReadByte();
            if (markerByte == -1)
            {
                throw new FormatException("Cannot read further to the o5m-file!");
            }
            var marker = (O5MFileByteMarker)markerByte;
            if (marker != O5MFileByteMarker.BoundingBox)
            {
                stream.Position -= 1;
                return;
            }

            var length = VarInt.ParseUInt64(stream);
            var buffer = new byte[length];
            var readBytes = this._stream!.Read(buffer, 0, buffer.Length);
            if (readBytes != buffer.Length)
            {
                throw new FormatException("The .o5m-file has an invalid bounding-box-length!");
            }

            var offset = 0;
            this._longitudeMin = VarInt.ParseInt64(buffer, ref offset) / POINT_DIVIDER;
            this._latitudeMin = VarInt.ParseInt64(buffer, ref offset) / POINT_DIVIDER;
            this._longitudeMax = VarInt.ParseInt64(buffer, ref offset) / POINT_DIVIDER;
            this._latitudeMax = VarInt.ParseInt64(buffer, ref offset) / POINT_DIVIDER;
        }

        /// <summary>
        /// Start reading and processing O5M data.
        /// </summary>
        public void Start()
        {
            this.Reset();

            var nodesStarted = false;
            var waysStarted = false;
            var relationsStarted = false;

            var streamLength = this._stream!.Length;
            var length = 0UL;
            var elementBuffer = Array.Empty<byte>();
            var elementBufferReadBytes = 0;
            O5MFileByteMarker marker;
            var nextByte = 0;
            do
            {
                nextByte = this._stream!.ReadByte();
                if (nextByte == -1)
                {
                    break;
                }
                marker = (O5MFileByteMarker)nextByte;
                if (marker == O5MFileByteMarker.StartByte)
                { // O5MFileByteMarker.Reset
                    this.Reset();
                    continue;
                }
                if (marker == O5MFileByteMarker.EndByte)
                {
                    break;
                }
                length = VarInt.ParseUInt64(this._stream);

                switch (marker)
                {
                    case O5MFileByteMarker.Node:
                        if (!nodesStarted)
                        {
                            nodesStarted = true;
                        }
                        elementBuffer = new byte[length];
                        elementBufferReadBytes = this._stream.Read(elementBuffer, 0, elementBuffer.Length);
                        if (elementBufferReadBytes != elementBuffer.Length)
                        {
                            throw new FormatException("The .o5m-file has an invalid node-length! expected: " + length + " bytes, but was " + elementBufferReadBytes + " bytes.");
                        }
                        if (this._skipNodes)
                        {
                            break;
                        }

#if DEBUG
                        var nodeDebugInfos = new ElementDebugInfos();
                        var node = this.ParseNodeData(elementBuffer, nodeDebugInfos);
                        this.OnNodeFound(node, elementBuffer, nodeDebugInfos);
#else
						var node = this.ParseNodeData(elementBuffer);
						this.OnNodeFound(node);
#endif
                        break;

                    case O5MFileByteMarker.Way:
                        if (!waysStarted)
                        {
                            this._lastTimestamp = 0;
                            this._lastChangeset = 0;
                            waysStarted = true;
                        }
                        elementBuffer = new byte[length];
                        elementBufferReadBytes = this._stream.Read(elementBuffer, 0, elementBuffer.Length);
                        if (elementBufferReadBytes != elementBuffer.Length)
                        {
                            throw new FormatException("The .o5m-file has an invalid way-length! expected: " + length + " bytes, but was " + elementBufferReadBytes + " bytes.");
                        }
                        if (this._skipWays)
                        {
                            break;
                        }

#if DEBUG
                        var wayDebugInfos = new ElementDebugInfos();
                        var way = this.ParseWayData(elementBuffer, wayDebugInfos);
                        this.OnWayFound(way, elementBuffer, wayDebugInfos);
#else
						var way = this.ParseWayData(elementBuffer);
						this.OnWayFound(way);
#endif
                        break;

                    case O5MFileByteMarker.Relation:
                        if (!relationsStarted)
                        {
                            this._lastTimestamp = 0;
                            this._lastChangeset = 0;
                            this._lastReferenceId = 0;
                            relationsStarted = true;
                            this.Reset();
                        }
                        elementBuffer = new byte[length];
                        elementBufferReadBytes = this._stream.Read(elementBuffer, 0, elementBuffer.Length);
                        if (elementBufferReadBytes != elementBuffer.Length)
                        {
                            throw new FormatException("The .o5m-file has an invalid relation-length! expected: " + length + " bytes, but was " + elementBufferReadBytes + " bytes.");
                        }
                        if (this._skipRelations)
                        {
                            break;
                        }

#if DEBUG
                        var relationDebugInfos = new ElementDebugInfos();
                        var relation = this.ParseRelationData(elementBuffer, relationDebugInfos);
                        this.OnRelationFound(relation, elementBuffer, relationDebugInfos);
#else
						var relation = this.ParseRelationData(elementBuffer);
						this.OnRelationFound(relation);
#endif
                        break;

                    default:
                        throw new Exception("unhandled marker (Unknown): " + marker);
                        //break;
                }
            } while (this._stream.Position < streamLength && !this._stop);
            this._stop = false;
        }

        /// <summary>
        /// Stop reading and processing O5M data.
        /// </summary>
        public void Stop()
        {
            this._stop = true;
        }

        private OSMNode ParseNodeData(
            byte[] data
#if DEBUG
            , ElementDebugInfos debugInfos
#endif
                                     )
        {
            var bufferOffset = 0;

            var nodeId = VarInt.ParseInt64(data, ref bufferOffset) + this._lastNodeId;
            var o5mNode = new OSMNode((ulong)nodeId);
            this._lastNodeId = (long)o5mNode.Id;

#if DEBUG
            this.ParseVersionData(data, o5mNode, ref bufferOffset, debugInfos);
#else
			this.ParseVersionData(data, o5mNode, ref bufferOffset);
#endif

            var parsedInt = VarInt.ParseInt32(data, ref bufferOffset);
            var longLongitude = parsedInt + this._lastLongitude;
            var longLatitude = VarInt.ParseInt32(data, ref bufferOffset) + this._lastLatitude;
            this._lastLongitude = longLongitude;
            this._lastLatitude = longLatitude;
            o5mNode.Longitude = longLongitude / POINT_DIVIDER;
            o5mNode.Latitude = longLatitude / POINT_DIVIDER;

#if DEBUG
            this.ParseTagsData(data, o5mNode, ref bufferOffset, debugInfos);
#else
			this.ParseTagsData(data, o5mNode, ref bufferOffset);
#endif

            return o5mNode;
        }

        private OSMWay ParseWayData(
            byte[] data
#if DEBUG
            , ElementDebugInfos debugInfos
#endif
                                   )
        {
            var bufferOffset = 0;

            var wayId = VarInt.ParseInt64(data, ref bufferOffset) + this._lastWayId;
            var o5mWay = new OSMWay((ulong)wayId);
            this._lastWayId = (long)o5mWay.Id;

#if DEBUG
            this.ParseVersionData(data, o5mWay, ref bufferOffset, debugInfos);
#else
			this.ParseVersionData(data, o5mWay, ref bufferOffset);
#endif

            if (data[bufferOffset] > 0)
            {
                var referenceLength = VarInt.ParseUInt64(data, ref bufferOffset);
                var referenceNodesBuffer = new byte[referenceLength];
                Array.Copy(data, bufferOffset, referenceNodesBuffer, 0, referenceNodesBuffer.Length);
                var referenceBufferOffset = 0;
                var bytesUnread = referenceNodesBuffer.Length;
                while (bytesUnread > 0)
                {
                    var currentReferenceId = VarInt.ParseInt64(referenceNodesBuffer, ref referenceBufferOffset) + this._lastReferenceId;
                    o5mWay.NodeRefs.Add(currentReferenceId);
                    this._lastReferenceId = currentReferenceId;
                    bytesUnread = referenceNodesBuffer.Length - referenceBufferOffset;
                }
                bufferOffset += (int)referenceLength;
            }

#if DEBUG
            this.ParseTagsData(data, o5mWay, ref bufferOffset, debugInfos);
#else
			this.ParseTagsData(data, o5mWay, ref bufferOffset);
#endif

            return o5mWay;
        }

        private OSMRelation ParseRelationData(
            byte[] data
#if DEBUG
            , ElementDebugInfos debugInfos
#endif
            )
        {
            var bufferOffset = 0;

#if DEBUG
            var idPosition = (uint)bufferOffset + 1;
#endif
            var relationId = VarInt.ParseInt64(data, ref bufferOffset) + this._lastRelationId;
#if DEBUG
            debugInfos.Add(idPosition, "start of 'id' (" + relationId + ").");
#endif
            var o5mRelation = new OSMRelation((ulong)relationId);
            this._lastRelationId = (long)o5mRelation.Id;

#if DEBUG
            this.ParseVersionData(data, o5mRelation, ref bufferOffset, debugInfos);
#else
			this.ParseVersionData(data, o5mRelation, ref bufferOffset);
#endif

            if (data[bufferOffset] > 0)
            {
                KeyValuePair<byte[], byte[]>? typeAndRole;
#if DEBUG
                var referenceLengthPosition = (uint)bufferOffset + 1;
#endif
                var referenceLength = VarInt.ParseUInt64(data, ref bufferOffset);
#if DEBUG
                debugInfos.Add(referenceLengthPosition, "start of 'length of reference section' (" + referenceLength + ").");
#endif
                var startOffset = bufferOffset;
                var bytesUnread = referenceLength;
                while (bytesUnread > 0)
                {
#if DEBUG
                    var referenceIdPosition = (uint)bufferOffset + 1;
#endif
                    var currentReferenceId = VarInt.ParseInt64(data, ref bufferOffset) + this._lastReferenceId;
#if DEBUG
                    debugInfos.Add(referenceIdPosition, "start of 'reference-id' (" + currentReferenceId + ").");
#endif
                    this._lastReferenceId = currentReferenceId;
                    if (data[bufferOffset] == 0)
                    {
#if DEBUG
                        debugInfos.Add((uint)bufferOffset + 1, "start of 'type/role'-pair (raw).");
#endif
                        typeAndRole = StringPair.ParseToTypeRoleByteArray(data, ref bufferOffset);
                        if (typeAndRole.HasValue)
                        {
                            var typeAndRoleValue = typeAndRole.Value;
                            if (typeAndRoleValue.Key.Length + typeAndRoleValue.Value.Length <= 250)
                            {
                                this._storedStringPairs.Insert(0, typeAndRoleValue);
                            }
                        }
                    }
                    else
                    {
#if DEBUG
                        var typeRolePosition = (uint)bufferOffset + 1;
#endif
                        var storedPosition = VarInt.ParseUInt32(data, ref bufferOffset);
#if DEBUG
                        debugInfos.Add(typeRolePosition, "start of stored 'type/role'-pair (" + storedPosition + ").");
#endif
                        typeAndRole = this._storedStringPairs[(int)storedPosition - 1];
                    }
                    if (typeAndRole?.Key.Length > 0)
                    {
                        var typeValue = (MemberType)(typeAndRole.Value.Key[0] - 0x30);
                        var roleValue = Encoding.UTF8.GetString(typeAndRole.Value.Value);
                        o5mRelation.Members.Add(new OSMMember(typeValue, (ulong)currentReferenceId, roleValue));
                    }
                    bytesUnread = referenceLength - ((ulong)bufferOffset - (ulong)startOffset);
                }
            }

#if DEBUG
            this.ParseTagsData(data, o5mRelation, ref bufferOffset, debugInfos);
#else
			this.ParseTagsData(data, o5mRelation, ref bufferOffset);
#endif

            return o5mRelation;
        }

        private void ParseVersionData(
            byte[] data,
            OSMElement element,
            ref int bufferOffset
#if DEBUG
            , ElementDebugInfos debugInfos
#endif
                                     )
        {
#if DEBUG
            var versionPosition = (uint)bufferOffset + 1;
#endif
            element.Version = VarInt.ParseUInt64(data, ref bufferOffset);
#if DEBUG
            debugInfos.Add(versionPosition, "start of 'version' (" + element.Version + ").");
#endif
            if (element.Version == 0)
            {
                return;
            }

#if DEBUG
            var timestampOffset = (uint)bufferOffset + 1;
#endif
            var timestampDiff = VarInt.ParseInt64(data, ref bufferOffset);
#if DEBUG
            debugInfos.Add(timestampOffset, "start of 'timestamp' (diff: " + timestampDiff + ").");
#endif
            var unixTimestamp = timestampDiff + this._lastTimestamp;
            this._lastTimestamp = unixTimestamp;
            element.Timestamp = UNIX_START.AddSeconds(unixTimestamp);
            if (unixTimestamp == 0)
            {
                return;
            }

#if DEBUG
            var changesetPosition = (uint)bufferOffset + 1;
#endif
            element.Changeset = (ulong)(VarInt.ParseInt64(data, ref bufferOffset) + this._lastChangeset);
#if DEBUG
            debugInfos.Add(changesetPosition, "start of 'changeset' (" + element.Changeset + ").");
#endif
            this._lastChangeset = (long)element.Changeset;

            KeyValuePair<byte[], byte[]>? keyValuePair;
            if (data[bufferOffset] == 0)
            {
#if DEBUG
                debugInfos.Add((uint)bufferOffset + 1, "start of 'uid/user'-pair (raw).");
#endif
                keyValuePair = StringPair.ParseToByteArrayPair(data, ref bufferOffset);
                if (keyValuePair.HasValue)
                {
                    var keyValuePairValue = keyValuePair.Value;
                    element.UserId = VarInt.ParseUInt64(keyValuePairValue.Key);
                    element.UserName = Encoding.UTF8.GetString(keyValuePairValue.Value);
                    if (element.UserId != 0)
                    {
                        if (keyValuePairValue.Key.Length + keyValuePairValue.Value.Length <= 250)
                        {
                            this._storedStringPairs.Insert(0, keyValuePairValue);
                        }
                    }
                }
                else
                {
                    this._storedStringPairs.Insert(0, new KeyValuePair<byte[], byte[]>(Array.Empty<byte>(), Array.Empty<byte>()));
                }
            }
            else
            {
#if DEBUG
                debugInfos.Add((uint)bufferOffset + 1, "start of 'uid/user'-pair (stored).");
#endif
                var storedPosition = VarInt.ParseUInt32(data, ref bufferOffset);
                if (this._storedStringPairs.ElementExistsAtPosition((int)storedPosition))
                {
                    keyValuePair = this._storedStringPairs[(int)storedPosition - 1];
                    if (keyValuePair.HasValue)
                    {
                        element.UserId = VarInt.ParseUInt64(keyValuePair.Value.Key);
                        element.UserName = Encoding.UTF8.GetString(keyValuePair.Value.Value);
                    }
                }
            }
        }

        private void ParseTagsData(
            byte[] data,
            OSMElement element,
            ref int bufferOffset
#if DEBUG
            , ElementDebugInfos debugInfos
#endif
                                  )
        {
            var unreadBytes = data.Length - bufferOffset;
            while (unreadBytes > 0)
            {
                if (data[bufferOffset] != 0)
                {
#if DEBUG
                    var keyValuePosition = (uint)bufferOffset + 1;
#endif
                    var storedPosition = VarInt.ParseUInt32(data, ref bufferOffset);
#if DEBUG
                    debugInfos.Add(keyValuePosition, "start of stored 'key/value'-pair (" + storedPosition + ").");
#endif
                    var tag = this._storedStringPairs[(int)storedPosition - 1];
                    element.Tags.Add(Encoding.UTF8.GetString(tag.Key), Encoding.UTF8.GetString(tag.Value));
                }
                else
                {
#if DEBUG
                    var keyValuePosition = (uint)bufferOffset + 1;
#endif
                    var tag = StringPair.ParseToByteArrayPair(data, ref bufferOffset);
#if DEBUG
                    debugInfos.Add(keyValuePosition, "start of raw 'key/value'-pair.");
#endif
                    if (tag.HasValue)
                    {
                        var tagValue = tag.Value;
                        element.Tags.Add(Encoding.UTF8.GetString(tagValue.Key), Encoding.UTF8.GetString(tagValue.Value));
                        if (tagValue.Key.Length + tagValue.Value.Length <= 250)
                        {
                            this._storedStringPairs.Insert(0, tagValue);
                        }
                    }
                }
                unreadBytes = data.Length - bufferOffset;
            }
        }
    }
}
