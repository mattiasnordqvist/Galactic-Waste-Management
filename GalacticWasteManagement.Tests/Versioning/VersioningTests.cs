using GalacticWasteManagement.Scripts;
using NUnit.Framework;
using Shouldly;
using Version = GalacticWasteManagement.Scripts.Version;

namespace GalacticWasteManagement.Tests.Versioning
{
    public class VersioningTests
    {
        private static Semver2Versioning Versioning = new Semver2Versioning();

        [Test]
        public void TestEmbeddedScriptToCustomVersionComparisons()
        {
            var alpha = FromEmbedded("Scripts.Migrations._1._0._0_alpha.Migration.sql");
            var beta1 = FromEmbedded("Scripts.Migrations._1._0._0_beta._1.Migration.sql");
            var beta = FromEmbedded("Scripts.Migrations._1._0._0_beta.Migration.sql");
            var rc1 = FromEmbedded("Scripts.Migrations._1._0._0_rc._1.Migration.sql");
            var rc2 = FromEmbedded("Scripts.Migrations._1._0._0_rc._2.Migration.sql");
            var v100 = FromEmbedded("Scripts.Migrations._1._0._0.Migration.sql");
            var v1001 = FromEmbedded("Scripts.Migrations._1._0._0._1.Migration.sql");

            beta.ShouldBeGreaterThan(alpha);
            beta1.ShouldBeGreaterThan(beta);
            rc1.ShouldBeGreaterThan(beta1);
            rc2.ShouldBeGreaterThan(rc1);
            v100.ShouldBeGreaterThan(rc2);
            v1001.ShouldBeGreaterThan(v100);
        }

        [Test]
        public void TestVersionToCustomVersionComparisons()
        {
            var alpha = FromDB("1.0.0-alpha");
            var beta1 = FromDB("1.0.0-beta.1");
            var beta = FromDB("1.0.0-beta");
            var rc1 = FromDB("1.0.0-rc.1");
            var rc2 = FromDB("1.0.0-rc.2");
            var v100 = FromDB("1.0.0");
            var v1001 = FromDB("1.0.0.1");

            beta.ShouldBeGreaterThan(alpha);
            beta1.ShouldBeGreaterThan(beta);
            rc1.ShouldBeGreaterThan(beta1);
            rc2.ShouldBeGreaterThan(rc1);
            v100.ShouldBeGreaterThan(rc2);
            v1001.ShouldBeGreaterThan(v100);
        }

        [Test]
        public void TestCustomVersionToVersion()
        {
            var alpha = new Semver2Version("1", "0", "0", "alpha", null);
            var beta = new Semver2Version("1", "0", "0", "beta", null);
            var rc1 = new Semver2Version("1", "0", "0", "rc", "1");
            var v100 = new Semver2Version("1", "0", "0", null, null);
            var v1001 = new Semver2Version("1", "0", "0", null, "1");

            Versioning.ToVersion(alpha).Value.ShouldBe("1.0.0-alpha.0");
            Versioning.ToVersion(beta).Value.ShouldBe("1.0.0-beta.0");
            Versioning.ToVersion(rc1).Value.ShouldBe("1.0.0-rc.1");
            Versioning.ToVersion(v100).Value.ShouldBe("1.0.0.0");
            Versioning.ToVersion(v1001).Value.ShouldBe("1.0.0.1");
        }

        private Semver2Version FromEmbedded(string name) => Versioning.ToCustomVersion((TestScript)name);

        private Semver2Version FromDB(string value) => Versioning.FromVersion(new Version(value));

        private class TestScript : ScriptBase
        {
            public TestScript(string embeddedScriptNamespace)
            {
                Name = embeddedScriptNamespace;
            }
            public override string Sql => string.Empty;

            public override string Name { get; }

            public override IScriptType Type => ScriptType.Migration;

            public static implicit operator TestScript(string value) => new TestScript(value);
        }
    }
}
