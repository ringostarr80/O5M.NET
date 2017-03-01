namespace O5M
{
	public enum O5MFileByteMarker
	{
		StartByte = 0xff,
		EndByte = 0xfe,
		Node = 0x10,
		Way = 0x11,
		Relation = 0x12,
		BoundingBox = 0xdb,
		FileTimestamp = 0xdc,
		Header = 0xe0,
		Sync = 0xee,
		Jump = 0xef,
		Reset = 0xff
	}
}
