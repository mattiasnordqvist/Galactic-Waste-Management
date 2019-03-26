using System.Text.RegularExpressions;

namespace GalacticWasteManagement.Scripts
{

    public class DefaultMigrationVersioning : MigrationVersioning<DefaultVersion>
    {
        private Regex embeddedScriptNameVersionRegexp = new Regex(@"\._(?<maj>\d{1,})\._(?<min>\d{1,})\.");
        private Regex versionRegex = new Regex(@"(?<maj>\d{1,})\.(?<min>\d{1,})");

        public override DefaultVersion DetermineVersion(IScript script)
        {
            var match = embeddedScriptNameVersionRegexp.Match(script.Name);
            return new DefaultVersion
            {
                Major = int.Parse(match.Groups["maj"].Value),
                Minor = int.Parse(match.Groups["min"].Value)
            };
        }

        public override int Compare(DefaultVersion version, DefaultVersion otherVersion)
        {
            return version.CompareTo(otherVersion);
        }

        public override string ToString(DefaultVersion version)
        {
            return $"{version.Major}.{version.Minor}";
        }

        public override DefaultVersion FromString(string version)
        {
            var match = versionRegex.Match(version);
            return new DefaultVersion
            {
                Major = int.Parse(match.Groups["maj"].Value),
                Minor = int.Parse(match.Groups["min"].Value)
            };
        }
    }
}
