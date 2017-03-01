using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using O5M.Helper;
using OSMDataPrimitives;

namespace O5M
{
	public class O5MWriter : O5MBase, IDisposable
	{
		private bool _nodesStarted = false;
		private bool _waysStarted = false;
		private bool _relationsStarted = false;

		public O5MWriter(string filename, O5MHeader header = O5MHeader.O5M2)
		{
			this.Init(File.OpenWrite(filename), header);
		}

		public O5MWriter(Stream stream, O5MHeader header = O5MHeader.O5M2)
		{
			this.Init(stream, header);
		}

		private void Init(Stream stream, O5MHeader header = O5MHeader.O5M2)
		{
			this._stream = stream;
			this._stream.WriteByte((byte)O5MFileByteMarker.StartByte);
			this._stream.WriteByte((byte)O5MFileByteMarker.Header);

			var headerLengthBytes = VarintBitConverter.GetVarintBytes(4U);
			var headerBuffer = Encoding.UTF8.GetBytes(header.ToString().ToLower());
			this._stream.Write(headerLengthBytes, 0, headerLengthBytes.Length);
			this._stream.Write(headerBuffer, 0, headerBuffer.Length);
		}

		~O5MWriter()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if(this._stream != null) {
				this._stream.WriteByte((byte)O5MFileByteMarker.EndByte);
				this._stream.Dispose();
				this._stream = null;
			}
		}

		public void WriteTimestamp(DateTime timestamp)
		{
			this._stream.WriteByte((byte)O5MFileByteMarker.FileTimestamp);
			var unixTimestamp = (long)timestamp.Subtract(UNIX_START).TotalSeconds;
			var timestampBytes = VarintBitConverter.GetVarintBytes(unixTimestamp);
			var timestampBytesLength = VarintBitConverter.GetVarintBytes((ulong)timestampBytes.Length);
			this._stream.Write(timestampBytesLength, 0, timestampBytesLength.Length);
			this._stream.Write(timestampBytes, 0, timestampBytes.Length);
		}

		public void WriteBoundings(double latitudeMin, double latitudeMax, double longitudeMin, double longitudeMax)
		{
			this._stream.WriteByte((byte)O5MFileByteMarker.BoundingBox);

			var longitudeMinLongBytes = VarintBitConverter.GetVarintBytes((long)(longitudeMin * POINT_DIVIDER));
			var latitudeMinLongBytes = VarintBitConverter.GetVarintBytes((long)(latitudeMin * POINT_DIVIDER));
			var longitudeMaxLongBytes = VarintBitConverter.GetVarintBytes((long)(longitudeMax * POINT_DIVIDER));
			var latitudeMaxLongBytes = VarintBitConverter.GetVarintBytes((long)(latitudeMax * POINT_DIVIDER));
			var boundingBytesLength = VarintBitConverter.GetVarintBytes((ulong)(longitudeMinLongBytes.Length + latitudeMinLongBytes.Length + longitudeMaxLongBytes.Length + latitudeMaxLongBytes.Length));
			this._stream.Write(boundingBytesLength, 0, boundingBytesLength.Length);
			this._stream.Write(longitudeMinLongBytes, 0, longitudeMinLongBytes.Length);
			this._stream.Write(latitudeMinLongBytes, 0, latitudeMinLongBytes.Length);
			this._stream.Write(longitudeMaxLongBytes, 0, longitudeMaxLongBytes.Length);
			this._stream.Write(latitudeMaxLongBytes, 0, latitudeMaxLongBytes.Length);
		}

		public void WriteElement(IOSMElement element)
		{
			if(element is OSMNode) {
				this.WriteNode((OSMNode)element);
			} else if(element is OSMWay) {
				this.WriteWay((OSMWay)element);
			} else if(element is OSMRelation) {
				this.WriteRelation((OSMRelation)element);
			}
		}

#if DEBUG
		public void WriteElement(IOSMElement element, out byte[] writtenData)
		{
			writtenData = null;
			if(element is OSMNode) {
				this.WriteNode((OSMNode)element, out writtenData);
			} else if(element is OSMWay) {
				this.WriteWay((OSMWay)element, out writtenData);
			} else if(element is OSMRelation) {
				this.WriteRelation((OSMRelation)element, out writtenData);
			}
		}
#endif

		private void WriteNode(OSMNode node)
		{
			byte[] writtenData = null;
			this.WriteNode(node, out writtenData);
		}

		private void WriteNode(OSMNode node, out byte[] writtenData)
		{
			if(!this._nodesStarted) {
				this.Reset();
				this._stream.WriteByte((byte)O5MFileByteMarker.Reset);
				this._nodesStarted = true;
			}

			this._stream.WriteByte((byte)O5MFileByteMarker.Node);

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
			this._stream.Write(length, 0, length.Length);
			writtenData = bytes.ToArray();
			this._stream.Write(writtenData, 0, bytes.Count);
		}

		private void WriteWay(OSMWay way)
		{
			byte[] writtenData = null;
			this.WriteWay(way, out writtenData);
		}

		private void WriteWay(OSMWay way, out byte[] writtenData)
		{
			if(!this._waysStarted) {
				this.Reset();
				this._stream.WriteByte((byte)O5MFileByteMarker.Reset);
				this._waysStarted = true;
			}

			this._stream.WriteByte((byte)O5MFileByteMarker.Way);

			var bytes = new List<byte>();
			var diffId = (long)way.Id - this._lastWayId;
			var idBytes = VarintBitConverter.GetVarintBytes(diffId);
			bytes.AddRange(idBytes);
			this._lastWayId = (long)way.Id;

			this.WriteVersionData(way, bytes);

			if(way.NodeRefs.Count == 0) {
				bytes.Add(0x00);
			} else {
				var nodeRefsBytes = new List<byte>();
				foreach(var nodeRef in way.NodeRefs) {
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
			this._stream.Write(length, 0, length.Length);
			writtenData = bytes.ToArray();
			this._stream.Write(writtenData, 0, bytes.Count);
		}

		private void WriteRelation(OSMRelation relation)
		{
			byte[] writtenData = null;
			this.WriteRelation(relation, out writtenData);
		}

		private void WriteRelation(OSMRelation relation, out byte[] writtenData)
		{
			if(!this._relationsStarted) {
				this.Reset();
				this._stream.WriteByte((byte)O5MFileByteMarker.Reset);
				this._relationsStarted = true;
			}

			this._stream.WriteByte((byte)O5MFileByteMarker.Relation);

			var bytes = new List<byte>();
			var diffId = (long)relation.Id - this._lastRelationId;
			var idBytes = VarintBitConverter.GetVarintBytes(diffId);
			bytes.AddRange(idBytes);
			this._lastRelationId = (long)relation.Id;

			this.WriteVersionData(relation, bytes);

			if(relation.Members.Count == 0) {
				bytes.Add(0x00);
			} else {
				var membersBytes = new List<byte>();
				foreach(var member in relation.Members) {
					var nextReferenceDiff = (long)member.Ref - this._lastReferenceId;
					var memberRefBytes = VarintBitConverter.GetVarintBytes(nextReferenceDiff);
					this._lastReferenceId = (long)member.Ref;
					membersBytes.AddRange(memberRefBytes);

					var typeBytes = new byte[] { (byte)((int)member.Type + 0x30) };
					var roleBytes = Encoding.UTF8.GetBytes(member.Role);
					var position = this._storedStringPairs.GetElementPosition(typeBytes, roleBytes);
					if(position == -1) {
						membersBytes.Add(0x00);
						membersBytes.AddRange(typeBytes);
						membersBytes.AddRange(roleBytes);
						membersBytes.Add(0x00);
						if(1 + roleBytes.Length <= 250) {
							var typeAndRole = new KeyValuePair<byte[], byte[]>(typeBytes, roleBytes);
							this._storedStringPairs.Insert(0, typeAndRole);
						}
					} else {
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
			this._stream.Write(length, 0, length.Length);
			writtenData = bytes.ToArray();
			this._stream.Write(writtenData, 0, bytes.Count);
		}

		private void WriteVersionData(IOSMElement element, List<byte> bytes)
		{
			if(element.Version == 0) {
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

			if(timestamp == 0) {
				return;
			}

			var changesetDiff = (long)element.Changeset - this._lastChangeset;
			var changesetBytes = VarintBitConverter.GetVarintBytes(changesetDiff);
			this._lastChangeset = (long)element.Changeset;
			bytes.AddRange(changesetBytes);

			var uidBytes = VarintBitConverter.GetVarintBytes(element.UserId);
			if(element.UserId == 0) {
				uidBytes = new byte[0];
			}
			var userBytes = Encoding.UTF8.GetBytes(element.UserName);
			var elementPosition = this._storedStringPairs.GetElementPosition(uidBytes, userBytes);
			if(elementPosition != -1) {
				var positionBytes = VarintBitConverter.GetVarintBytes((uint)elementPosition);
				bytes.AddRange(positionBytes);
			} else {
				bytes.Add(0x00);
				bytes.AddRange(uidBytes);
				bytes.Add(0x00);
				bytes.AddRange(userBytes);
				bytes.Add(0x00);
				var uidUserPair = new KeyValuePair<byte[], byte[]>(uidBytes, userBytes);
				if(uidUserPair.Key.Length + uidUserPair.Value.Length <= 250) {
					this._storedStringPairs.Insert(0, uidUserPair);
				}
			}
		}

		private void WriteTags(IOSMElement element, List<byte> bytes)
		{
			if(element.Tags.Count == 0) {
				return;
			}

			foreach(string tagKey in element.Tags) {
				var tagKeyBytes = Encoding.UTF8.GetBytes(tagKey);
				var tagValueBytes = Encoding.UTF8.GetBytes(element.Tags[tagKey]);
				var elementPosition = this._storedStringPairs.GetElementPosition(tagKeyBytes, tagValueBytes);
				if(elementPosition != -1) {
					var positionBytes = VarintBitConverter.GetVarintBytes((uint)elementPosition);
					bytes.AddRange(positionBytes);
				} else {
					var stringPair = new KeyValuePair<byte[], byte[]>(tagKeyBytes, tagValueBytes);
					bytes.Add(0x00);
					bytes.AddRange(tagKeyBytes);
					bytes.Add(0x00);
					bytes.AddRange(tagValueBytes);
					bytes.Add(0x00);
					if(stringPair.Key.Length + stringPair.Value.Length <= 250) {
						this._storedStringPairs.Insert(0, stringPair);
					}
				}
			}
		}
	}
}
