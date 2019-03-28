using JellyDust;

namespace GalacticWasteManagement
{
    public interface IMigrationFactory
    {
        string Name { get; }
        IMigration Create(GalacticWasteManager gwm, IConnection c, ITransaction t);
    }
}
