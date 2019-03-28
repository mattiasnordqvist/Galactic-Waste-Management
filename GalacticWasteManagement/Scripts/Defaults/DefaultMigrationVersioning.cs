using System.Text.RegularExpressions;

namespace GalacticWasteManagement.Scripts
{

    public class DefaultMigrationVersioning : MigrationVersioningBase<DefaultVersion>
    {
        private Regex embeddedScriptNameVersionRegexp = new Regex(@"\._(?<maj>\d{1,})\._(?<min>\d{1,})\.");
        private Regex versionRegex = new Regex(@"(?<maj>\d{1,})\.(?<min>\d{1,})");

        public override DefaultVersion GetVersion(IScript script)
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

        public override Version ToVersionStringForJournaling(DefaultVersion version)
        {
            return new Version($"{version.Major}.{version.Minor}");
        }

        public override DefaultVersion FromVersionStringForJournaling(Version version)
        {
            var match = versionRegex.Match(version.Value);
            return new DefaultVersion
            {
                Major = int.Parse(match.Groups["maj"].Value),
                Minor = int.Parse(match.Groups["min"].Value)
            };
        }
    }
}
