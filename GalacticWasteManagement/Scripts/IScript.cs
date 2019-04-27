using System.Collections.Generic;
using System.Threading.Tasks;
using JellyDust;

namespace GalacticWasteManagement.Scripts
{
    public interface IScript
    {
        string Name { get; }
        string GetHash();
        IScriptType Type { get; }
        Task ApplyAsync(ITransaction transaction, Dictionary<string, string> scriptVariables);
    }
}
