using System.Text.RegularExpressions;

namespace GalacticWasteManagement.Scripts
{
    public class DefaultMigrationVersioning : IMigrationVersioning
    {
        private Regex versionRegex = new Regex(@"\._(?<maj>\d{1,})\._(?<min>\d{1,})\.");

        public string DetermineVersion(IScript script)
        {
            return $"{versionRegex.Match(script.Name).Groups["maj"].Value}.{versionRegex.Match(script.Name).Groups["min"].Value}";
        }
    }
}
