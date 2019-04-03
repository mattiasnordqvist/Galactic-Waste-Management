using JellyDust;

namespace GalacticWasteManagement
{
    public class LiveFieldMigrationFactory : IMigrationFactory
    {
        public string Name { get; } = "LiveField";
        public IMigration Create(GalacticWasteManager gwm, IConnection c, ITransaction t)
        {
            return new LiveFieldMigration(gwm.ProjectSettings, gwm.Logger, gwm.Output, gwm.Parameters, c, t, Name);
        }
    }
}
