using System;
using System.Collections.Generic;
using System.Linq;

namespace O5M
{
	public class O5MStringPairList : List<KeyValuePair<byte[], byte[]>>
	{
		private const int MAX_ELEMENTS = 15000;

		new public KeyValuePair<byte[], byte[]> this[int index] {
			get {
				if(index >= MAX_ELEMENTS) {
					throw new ArgumentOutOfRangeException(nameof(index), index, "The maximum allowed index is " + (MAX_ELEMENTS - 1));
				}
				if(!this.ElementExistsAtPosition(index + 1)) {
					throw new ArgumentOutOfRangeException(nameof(index), index, "The index is out of Range. Max index is " + (this.Count - 1));
				}

				return base[index];
			}
			set {
				if(index >= MAX_ELEMENTS) {
					throw new ArgumentOutOfRangeException(nameof(index), index, "The maximum allowed index is " + (MAX_ELEMENTS - 1));
				}

				base[index] = value;
			}
		}

		new public void Insert(int index, KeyValuePair<byte[], byte[]> item)
		{
			base.Insert(index, item);
			if(this.Count > MAX_ELEMENTS) {
				base.RemoveAt(this.Count - 1);
			}
		}

		public bool ElementExistsAtPosition(int position)
		{
			return (position <= this.Count);
		}

		public int GetElementPosition(byte[] key, byte[] value)
		{
			for(var i = 0; i < this.Count; i++) {
				if(!this[i].Key.SequenceEqual(key)) {
					continue;
				}
				if(!this[i].Value.SequenceEqual(value)) {
					continue;
				}
				return i + 1;
			}

			return -1;
		}
	}
}
