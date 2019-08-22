using GalacticWasteManagement.Scripts;
using System.Text.RegularExpressions;
using Version = GalacticWasteManagement.Scripts.Version;

namespace GalacticWasteManagement.Tests.Versioning
{
    public class Semver2MigrationVersioning : CustomVersionMigrationVersioningBase<Semver2Version>
    {
        private Regex embeddedScriptNameVersionRegexp = new Regex(@"\.(?<maj>\d{1,})\.(?<min>\d{1,})\.(?<patch>\d{1,})(?<pre>rc|beta|alpha)?\.(?<build>\d{1,})?");
        private Regex versionRegex = new Regex(@"(?<maj>\d{1,})\.(?<min>\d{1,})\.(?<patch>\d{1,})?(?<pre>-rc|-beta|-alpha)?\.?(?<build>\d{1,})?");

        public override Semver2Version ToCustomVersion(IScript script)
        {
            var match = embeddedScriptNameVersionRegexp.Match(script.Name.Replace("_", string.Empty));
            var major = match.Groups["maj"].Value;
            var minor = match.Groups["min"].Value;
            var patch = match.Groups["patch"].Value;
            var pre = match.Groups["pre"].Value;
            var build = match.Groups["build"].Value;
            return new Semver2Version(major, minor, patch, pre, build);
        }

        public override Version ToVersion(Semver2Version version)
        {
            var preRelease = version.PreRelease == PreReleaseType.None ? string.Empty : $"-{version.PreRelease.ToString().ToLower()}";
            return new Version($"{version.Major}.{version.Minor}.{version.Patch}{preRelease}.{version.Build}");
        }

        public override Semver2Version FromVersion(Version version)
        {
            var match = versionRegex.Match(version.Value);
            var major = match.Groups["maj"].Value;
            var minor = match.Groups["min"].Value;
            var patch = match.Groups["patch"].Value;
            var pre = match.Groups["pre"].Value;
            var build = match.Groups["build"].Value;
            return new Semver2Version(major, minor, patch, pre, build);
        }
    }
}
