using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;

namespace Linko.LinkoExchange.Data
{
	public class CustomTransactionWrapper
	{
		#region public properties

		public LinkoExchangeContext DbContext { get; set; }
		public Stack<string> CallStacks { get; set; }
		#endregion
	}
}