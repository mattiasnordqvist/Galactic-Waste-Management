using System.Collections.Generic;
using System.Threading.Tasks;
using JellyDust;

namespace GalacticWasteManagement.Scripts
{
    public interface IScript
    {
        string Name { get; }
        string GetHash();
        ScriptType Type { get; }
        Task ApplyAsync(IConnection connection, Dictionary<string, string> scriptVariables);
    }
}
