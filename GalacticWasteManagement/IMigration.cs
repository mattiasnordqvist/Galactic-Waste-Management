using GalacticWasteManagement.Scripts;
using SuperNotUnderstandableInputHandling;
using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public interface IMigration
    {
        GalacticWasteManager GalacticWasteManager { set; }
        string Name { get; set; }
        string DatabaseName { get; set; }
        IScriptContext ScriptContext { get; set; }
        Task ManageGalacticWaste();
        IParameters Parameters { get; }
    }
}