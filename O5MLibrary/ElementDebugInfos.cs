#if DEBUG
namespace O5M
{
    /// <summary>
    /// ElementDebugInfos is a class that provides informations for debugging purposes.
    /// </summary>
    public class ElementDebugInfos : Dictionary<uint, string>
    {
        /// <summary>
        /// Infos the exists for byte position.
        /// </summary>
        /// <returns><c>true</c>, if exists a debug information exists for byte position, <c>false</c> otherwise.</returns>
        /// <param name="position">Position.</param>
        public bool InfoExistsForBytePosition(uint position)
        {
            return this.ContainsKey(position);
        }
    }
}
#endif
