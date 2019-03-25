using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public interface IMigration
    {
        Task ManageWasteInField(WasteManagerConfiguration configuration);
    }
}