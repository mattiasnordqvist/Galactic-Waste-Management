using System.Collections.Generic;
using System.Threading.Tasks;
using JellyDust;

namespace GalacticWasteManagement.Scripts
{
    public interface IScript
    {
        string Name { get; }
        string Content { get; }
        string Hashed { get; }
        ScriptType Type { get; }
        Task Apply(IConnection connection, Dictionary<string, string> scriptVariables);
    }
}
