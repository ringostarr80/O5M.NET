using System.Collections.Generic;

namespace O5M
{
	public class ElementDebugInfos : Dictionary<uint, string>
	{
		public bool InfoExistsForBytePosition(uint position)
		{
			return this.ContainsKey(position);
		}
	}
}
