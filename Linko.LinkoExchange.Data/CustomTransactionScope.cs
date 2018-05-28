using System;
using System.Linq;

namespace Linko.LinkoExchange.Data
{
	public class CustomTransactionScope : IDisposable
	{
		#region fields

		private readonly CustomTransactionWrapper _requestScopedTransaction;

		#endregion

		#region constructors and destructor

		public CustomTransactionScope(CustomTransactionWrapper requestScopedTransaction, string from)
		{
			_requestScopedTransaction = requestScopedTransaction;
			requestScopedTransaction.CallStacks.Push(item:from);
		}

		#endregion

		#region interface implementations

		public void Dispose()
		{
			if (_requestScopedTransaction.CallStacks == null || !_requestScopedTransaction.CallStacks.Any())
			{
				return;
			}

			_requestScopedTransaction.CallStacks.Pop();
			if (_requestScopedTransaction.CallStacks.Count == 0)
			{
				try
				{
					_requestScopedTransaction.Transaction.Commit();
				}
				catch
				{
					_requestScopedTransaction.Transaction.Rollback();
					throw;
				}
			}
		}

		#endregion
	}
}