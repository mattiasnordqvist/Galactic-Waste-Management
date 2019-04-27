using System.Data.SqlClient;

namespace GalacticWasteManagement.SqlServer
{
    public static class MsSqlExtensions
    {
        public static SqlConnectionStringBuilder For(this SqlConnectionStringBuilder @this, string database)
        {
            var b = new SqlConnectionStringBuilder(@this.ConnectionString);
            b.InitialCatalog = database;
            return b;
        }
    }
}
