using System;
using System.Collections.Generic;
using System.Linq;

namespace O5M
{
    /// <summary>
    /// O5M String pair list.
    /// </summary>
    public class O5MStringPairList : List<KeyValuePair<byte[], byte[]>>
    {
        private const int MAX_ELEMENTS = 15000;

        /// <summary>
        /// Gets or sets the <see cref="T:O5M.O5MStringPairList"/> at the specified index.
        /// </summary>
        /// <param name="index">Index.</param>
        new public KeyValuePair<byte[], byte[]> this[int index]
        {
            get
            {
                if (index >= MAX_ELEMENTS)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, "The maximum allowed index is " + (MAX_ELEMENTS - 1));
                }
                if (!this.ElementExistsAtPosition(index + 1))
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, "The index is out of Range. Max index is " + (this.Count - 1));
                }

                return base[index];
            }
            set
            {
                if (index >= MAX_ELEMENTS)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, "The maximum allowed index is " + (MAX_ELEMENTS - 1));
                }

                base[index] = value;
            }
        }

        /// <summary>
        /// Insert the specified index and item.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="item">Item.</param>
        new public void Insert(int index, KeyValuePair<byte[], byte[]> item)
        {
            base.Insert(index, item);
            if (this.Count > MAX_ELEMENTS)
            {
                RemoveAt(this.Count - 1);
            }
        }

        /// <summary>
        /// Elements the exists at position.
        /// </summary>
        /// <returns><c>true</c>, if an element exists at position, <c>false</c> otherwise.</returns>
        /// <param name="position">Position.</param>
        public bool ElementExistsAtPosition(int position)
        {
            return (position <= this.Count);
        }

        /// <summary>
        /// Gets the element position.
        /// </summary>
        /// <returns>The element position.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public int GetElementPosition(byte[] key, byte[] value)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (!this[i].Key.SequenceEqual(key))
                {
                    continue;
                }
                if (!this[i].Value.SequenceEqual(value))
                {
                    continue;
                }
                return i + 1;
            }

            return -1;
        }
    }
}
