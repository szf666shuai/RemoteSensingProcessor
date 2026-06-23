namespace RemoteSensingProcessor.Model
{
    public class ImageCache
    {
        private readonly List<Bitmap> _history = new();
        private int _index = -1;
        private const int MaxHistory = 10;

        public void Reset(Bitmap initialState)
        {
            Clear();
            Record(initialState);
        }

        public void Record(Bitmap state)
        {
            while (_history.Count > _index + 1)
            {
                _history[^1].Dispose();
                _history.RemoveAt(_history.Count - 1);
            }

            while (_history.Count >= MaxHistory)
            {
                _history[0].Dispose();
                _history.RemoveAt(0);
                _index--;
            }

            _history.Add((Bitmap)state.Clone());
            _index = _history.Count - 1;
        }

        public Bitmap? Undo()
        {
            if (!CanUndo) return null;
            _index--;
            return (Bitmap)_history[_index].Clone();
        }

        public Bitmap? Redo()
        {
            if (!CanRedo) return null;
            _index++;
            return (Bitmap)_history[_index].Clone();
        }

        public bool CanUndo => _index > 0;
        public bool CanRedo => _index >= 0 && _index < _history.Count - 1;

        public void Clear()
        {
            foreach (Bitmap bmp in _history)
                bmp.Dispose();
            _history.Clear();
            _index = -1;
        }
    }
}
