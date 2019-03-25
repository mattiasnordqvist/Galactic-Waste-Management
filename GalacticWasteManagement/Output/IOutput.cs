using StackExchange.Profiling;

namespace GalacticWasteManagement.Output
{
    public interface IOutput
    {
        MiniProfiler MiniProfiler { set; get; }
        void Dump();
    }
}

