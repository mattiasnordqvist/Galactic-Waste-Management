using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using GalacticWasteManagement.Output;
using JellyDust;
using StackExchange.Profiling;

namespace GalacticWasteManagement
{
    public class ConnectionFactory : IDbConnectionFactory
    {
        private readonly string connectionString;
        private IOutput output;

        public ConnectionFactory(string connectionString, IOutput output)
        {
            this.connectionString = connectionString;
            this.output = output;
        }

        public IDbConnection CreateOpenConnection()
        {
            MiniProfiler mp = new MiniProfiler($"{Assembly.GetEntryAssembly().GetName()} - DatabaseUpdater", MiniProfiler.DefaultOptions);
            (output ?? (output = new NullOutput())).MiniProfiler = mp;
            var connection = new StackExchange.Profiling.Data.ProfiledDbConnection(new SqlConnection(connectionString), mp);
            connection.Disposed += (_, __) => { mp.Stop(); output.Dump(); };

            using (mp?.Ignore())
            {
                connection.Open();
            }

            return connection;
        }
    }
}
