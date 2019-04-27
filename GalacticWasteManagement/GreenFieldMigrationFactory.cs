using JellyDust;

namespace GalacticWasteManagement
{
    public class GreenFieldMigrationFactory : IMigrationFactory
    {
        public string Name { get; } = "GreenField";
        public IMigration Create(GalacticWasteManager gwm)
        {
            return new GreenFieldMigration(gwm, Name);
        }
    }
}
