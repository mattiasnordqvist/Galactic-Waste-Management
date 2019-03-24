using System.Data;
using JellyDust;

namespace GalacticWasteManagement
{
    public class TransactionFactory : IDbTransactionFactory
    {
        public IDbTransaction OpenTransaction(IDbConnection connection)
        {
            return connection.BeginTransaction();
        }
    }
}
