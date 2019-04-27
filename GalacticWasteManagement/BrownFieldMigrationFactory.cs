using JellyDust;

namespace GalacticWasteManagement
{
    public class BrownFieldMigrationFactory : IMigrationFactory
    {
        public string Name { get; } = "BrownField";
        public IMigration Create(GalacticWasteManager gwm)
        {
            return new BrownFieldMigration(gwm, Name);
        }
    }
}
