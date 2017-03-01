namespace O5M
{
	/// <summary>
	/// O5M File byte marker.
	/// </summary>
	public enum O5MFileByteMarker
	{
		/// <summary>
		/// The start byte.
		/// </summary>
		StartByte = 0xff,
		/// <summary>
		/// The end byte.
		/// </summary>
		EndByte = 0xfe,
		/// <summary>
		/// The node.
		/// </summary>
		Node = 0x10,
		/// <summary>
		/// The way.
		/// </summary>
		Way = 0x11,
		/// <summary>
		/// The relation.
		/// </summary>
		Relation = 0x12,
		/// <summary>
		/// The bounding box.
		/// </summary>
		BoundingBox = 0xdb,
		/// <summary>
		/// The file timestamp.
		/// </summary>
		FileTimestamp = 0xdc,
		/// <summary>
		/// The header.
		/// </summary>
		Header = 0xe0,
		/// <summary>
		/// The sync.
		/// </summary>
		Sync = 0xee,
		/// <summary>
		/// The jump.
		/// </summary>
		Jump = 0xef,
		/// <summary>
		/// The reset.
		/// </summary>
		Reset = 0xff
	}
}
