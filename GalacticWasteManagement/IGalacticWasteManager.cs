using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public interface IGalacticWasteManager
    {
        Task Update(string mode, bool clean = false, Dictionary<string, string> scriptVariables = null);
    }
}
