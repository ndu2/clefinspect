namespace clef_inspect.Model
{
    public class ClefLockedList
    {
        private object _mutexLines = new object();
        private List<ClefLine> _lines;

        public ClefLockedList()
        {
            _lines = new List<ClefLine>(100000);
        }
        public IList<ClefLine> ViewFrom(int first)
        {
            lock (_mutexLines)
            {
                return new List<ClefLine>(this._lines.GetRange(first, this.Count-first));
            }
        }

        internal void Clear()
        {
            lock (_mutexLines)
            {
                _lines.Clear();
            }
        }

        internal void ReplaceLast(ClefLine clefLine)
        {
            lock (_mutexLines)
            {
                _lines[_lines.Count - 1] = clefLine;
            }
        }

        internal void Add(ClefLine clefLine)
        {
            lock (_mutexLines)
            {
                _lines.Add(clefLine);
            }
        }

        public int Count
        {
            get
            {
                lock (_mutexLines)
                {
                    return _lines.Count;
                }
            }
        }
    }
}
