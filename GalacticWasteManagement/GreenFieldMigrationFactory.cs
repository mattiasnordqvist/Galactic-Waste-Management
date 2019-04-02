using JellyDust;

namespace GalacticWasteManagement
{
    public class GreenFieldMigrationFactory : IMigrationFactory
    {
        public string Name { get; } = "GreenField";
        public IMigration Create(GalacticWasteManager gwm, IConnection c, ITransaction t)
        {
            return new GreenFieldMigration(gwm.ProjectSettings, gwm.Logger, gwm.Output, gwm.Input, c, t, Name);
        }
    }
}
