using StackExchange.Profiling;

namespace GalacticWasteManagement.Output
{
    public class NullOutput : IOutput
    {
        public MiniProfiler MiniProfiler { set { value?.Ignore(); } }

        public void Dump() { }
    }
}

