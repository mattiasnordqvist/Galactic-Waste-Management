using System;
using System.Collections.Generic;
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
            var text = MiniProfiler.Root.Children
                ?.SelectMany(x =>
                 new List<string> { Environment.NewLine, $"-- {x.Name}" }
                 .Union(
                    x.CustomTimings.SelectMany(c =>
                       c.Value.Select(y => y.CommandString)
                       .Intersperse("GO"))
                        .ToList()));
            if (text != null)
            {
                File.WriteAllLines(_filePath, text);
            }
        }
    }
}

