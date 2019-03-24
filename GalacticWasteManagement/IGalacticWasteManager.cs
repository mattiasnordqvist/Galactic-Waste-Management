using System.Threading.Tasks;

namespace GalacticWasteManagement
{
    public interface IGalacticWasteManager
    {
        Task Update(WasteManagerConfiguration wasteManagerConfiguration, IField field);
    }
}
