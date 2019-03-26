namespace GalacticWasteManagement.Scripts
{

    public interface IMigrationVersioning
    {
        string DetermineVersion(IScript script);

        int Compare(string versionX, string versionY);

        int Compare(IScript versionX, string versionY);
    }
}
