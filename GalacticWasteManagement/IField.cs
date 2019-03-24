using System.Threading.Tasks;
using GalacticWasteManagement.Scripts;
using JellyDust;

namespace GalacticWasteManagement
{
    public interface IField
    {
        Task ManageWasteInField(IConnection connection, WasteManagerConfiguration configuration, IScriptProvider scriptProvider);
    }
}