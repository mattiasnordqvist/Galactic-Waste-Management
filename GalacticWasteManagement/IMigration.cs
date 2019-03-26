using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public interface IMigration
    {
        string DatabaseName { get; set; }
        Dictionary<string, string> ScriptVariables { get; set; }
        Task ManageWaste(bool clean);
    }
}