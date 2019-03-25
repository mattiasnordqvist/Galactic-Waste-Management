namespace GalacticWasteManagement.Scripts
{
    public interface IMigrationVersioning
    {
        string DetermineVersion(IScript script);
    }
}
