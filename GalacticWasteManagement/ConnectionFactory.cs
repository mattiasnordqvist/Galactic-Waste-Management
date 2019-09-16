using System.Data;
using System.Data.Common;
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
            var connection = new StackExchange.Profiling.Data.ProfiledDbConnection(CreteConnection(), mp);
            connection.Disposed += (_, __) => { mp.Stop(); };

            using (mp?.Ignore())
            {
                connection.Open();
            }

            return connection;
        }

        protected internal virtual DbConnection CreteConnection() => new SqlConnection(connectionString);
    }
}
