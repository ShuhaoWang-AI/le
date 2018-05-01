using System;
using System.Data.Entity;

namespace Linko.LinkoExchange.Data
{
    public class AutoCommitScope : IDisposable
    {
        private readonly DbContextTransaction _transaction;

        public AutoCommitScope(DbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public void Dispose()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
        }
    }
}
