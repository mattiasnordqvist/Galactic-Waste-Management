using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public interface IMigration
    {
        string Name { get; }
        string DatabaseName { get; set; }
        Dictionary<string, string> ScriptVariables { get; set; }
        Task ManageWaste();
    }
}