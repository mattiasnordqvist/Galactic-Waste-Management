using StackExchange.Profiling;

namespace GalacticWasteManagement.Output
{
    public class NullOutput : IOutput
    {
        private MiniProfiler _profiler;

        public MiniProfiler MiniProfiler
        {
            set
            {
                value?.Ignore();
                _profiler = value;
            }
            get { return _profiler; }
        }

        public void Dump() { }
    }
}

