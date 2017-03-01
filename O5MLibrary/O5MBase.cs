using System;
using System.IO;

namespace O5M
{
	public abstract class O5MBase
	{
		protected const double POINT_DIVIDER = 10000000;
		protected DateTime UNIX_START = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		protected Stream _stream = null;
		protected readonly O5MStringPairList _storedStringPairs = new O5MStringPairList();
		protected long _lastNodeId = 0;
		protected long _lastWayId = 0;
		protected long _lastRelationId = 0;
		protected long _lastReferenceId = 0;
		protected long _lastTimestamp = 0;
		protected long _lastChangeset = 0;
		protected int _lastLatitude = 0;
		protected int _lastLongitude = 0;

		protected void Reset()
		{
			this._lastNodeId = 0;
			this._lastWayId = 0;
			this._lastRelationId = 0;
			this._lastTimestamp = 0;
			this._lastChangeset = 0;
			this._lastLatitude = 0;
			this._lastLongitude = 0;
			this._lastReferenceId = 0;
			this._storedStringPairs.Clear();
		}
	}
}
