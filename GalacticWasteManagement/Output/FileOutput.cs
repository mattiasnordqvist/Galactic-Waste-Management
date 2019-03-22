using System.IO;
using System.Linq;
using GalacticWasteManagement.Utilities;
using StackExchange.Profiling;

namespace GalacticWasteManagement.Output
{
    public class FileOutput : IOutput
    {
        private readonly string _filePath;
        public MiniProfiler MiniProfiler { get; set; }

        public FileOutput(string filePath)
        {
            _filePath = filePath;
        }

        public void Dump()
        {
            var text = MiniProfiler.Root.CustomTimings?
                .SelectMany(x => x.Value.Select(y => y.CommandString)
                    .Intersperse("GO")
                    .ToList());
            if (text != null)
            {
                File.WriteAllLines(_filePath, text);
            }
        }
    }
}

