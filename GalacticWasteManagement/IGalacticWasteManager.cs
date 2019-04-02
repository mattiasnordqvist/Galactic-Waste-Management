using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public interface IGalacticWasteManager
    {
        Task Update(string mode, Dictionary<string, string> scriptVariables = null);
    }
}
