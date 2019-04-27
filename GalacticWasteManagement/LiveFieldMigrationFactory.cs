using JellyDust;

namespace GalacticWasteManagement
{
    public class LiveFieldMigrationFactory : IMigrationFactory
    {
        public string Name { get; } = "LiveField";
        public IMigration Create(GalacticWasteManager gwm)
        {
            return new LiveFieldMigration(gwm, Name);
        }
    }
}
