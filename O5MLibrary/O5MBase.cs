using System;
using System.IO;

namespace O5M
{
	/// <summary>
	/// O5MBase.
	/// </summary>
	public abstract class O5MBase
	{
		/// <summary>
		/// The point divider for latitude and longitude values.
		/// </summary>
		protected const double POINT_DIVIDER = 10000000;
		/// <summary>
		/// The unix start date.
		/// </summary>
		protected DateTime UNIX_START = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// The stream.
		/// </summary>
		protected Stream? _stream = null;
		/// <summary>
		/// The stored string pairs.
		/// </summary>
		protected readonly O5MStringPairList _storedStringPairs = new O5MStringPairList();
		/// <summary>
		/// The last node identifier.
		/// </summary>
		protected long _lastNodeId = 0;
		/// <summary>
		/// The last way identifier.
		/// </summary>
		protected long _lastWayId = 0;
		/// <summary>
		/// The last relation identifier.
		/// </summary>
		protected long _lastRelationId = 0;
		/// <summary>
		/// The last reference identifier.
		/// </summary>
		protected long _lastReferenceId = 0;
		/// <summary>
		/// The last timestamp.
		/// </summary>
		protected long _lastTimestamp = 0;
		/// <summary>
		/// The last changeset.
		/// </summary>
		protected long _lastChangeset = 0;
		/// <summary>
		/// The last latitude.
		/// </summary>
		protected int _lastLatitude = 0;
		/// <summary>
		/// The last longitude.
		/// </summary>
		protected int _lastLongitude = 0;

		/// <summary>
		/// Resets this instance.
		/// </summary>
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
