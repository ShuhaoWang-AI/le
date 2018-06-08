using System;
using System.Linq;
using NLog;

namespace Linko.LinkoExchange.Data
{
	public class CustomTransactionScope : IDisposable
	{
		#region fields

		private readonly CustomTransactionWrapper _requestScopedTransaction;
		private readonly ILogger _logger;

		#endregion

		#region constructors and destructor

		public CustomTransactionScope(ILogger logger, CustomTransactionWrapper requestScopedTransaction, string from)
		{
			_logger = logger;
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
				_requestScopedTransaction.DbContext.Commit();
			}
		}

		#endregion
	}
}