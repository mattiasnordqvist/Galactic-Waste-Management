using JellyDust;

namespace GalacticWasteManagement
{
    public class BrownFieldMigrationFactory : IMigrationFactory
    {
        public string Name { get; } = "BrownField";
        public IMigration Create(GalacticWasteManager gwm, IConnection c, ITransaction t)
        {
            return new BrownFieldMigration(gwm.ProjectSettings, gwm.Logger, gwm.Output, gwm.Parameters, c, t, Name);
        }
    }
}
