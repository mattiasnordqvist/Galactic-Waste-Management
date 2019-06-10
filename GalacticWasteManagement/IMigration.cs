using SuperNotUnderstandableInputHandling;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public interface IMigration
    {
        GalacticWasteManager GalacticWasteManager { set; }
        string Name { get; set; }
        string DatabaseName { get; set; }
        Dictionary<string, string> ScriptVariables { get; set; }
        Task ManageGalacticWaste();
        IParameters Parameters { get; }
    }
}