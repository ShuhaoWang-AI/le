using System;
using System.Reflection;

namespace Linko.LinkoExchange.Data
{
	public class CustomTransactionScope : IDisposable
	{
		private CustomTransactionWrapper RequestScopedTransaction { get; }
		#region constructors and destructor

		public CustomTransactionScope(CustomTransactionWrapper requestScopedTransaction, string from)
		{
			RequestScopedTransaction = requestScopedTransaction;
			requestScopedTransaction.CallStacks.Push(from);
		}

		#endregion

		#region interface implementations

		public void Dispose()
		{
			RequestScopedTransaction.CallStacks.Pop();
			if (RequestScopedTransaction.CallStacks.Count == 0)
			{
				try
				{
					RequestScopedTransaction.Transaction.Commit();
				}
				catch
				{
					RequestScopedTransaction.Transaction.Rollback();
					throw;
				}
			}
		}

		#endregion

		#region public properties


		#endregion
	}
}