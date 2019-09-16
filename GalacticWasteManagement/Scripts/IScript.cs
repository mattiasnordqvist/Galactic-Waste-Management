using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JellyDust;

namespace GalacticWasteManagement.Scripts
{
    public interface IScript
    {
        string Name { get; }
        Func<string> GetHash { get; set; }
        IScriptType Type { get; }
        Task ApplyAsync(ITransaction transaction, Dictionary<string, string> scriptVariables);
    }
}
