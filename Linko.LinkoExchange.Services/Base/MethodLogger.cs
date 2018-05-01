using System;
using System.Reflection;

using NLog;

namespace Linko.LinkoExchange.Services.Base
{
    public class MethodLogger : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _methodName;

        public MethodLogger(ILogger logger, MethodBase methodBase, string descripition = "")
        {
            _logger = logger;
            _methodName = methodBase.DeclaringType.Name + "." + methodBase.Name;

            _logger.Info(message: $"Start: ${_methodName}. ${descripition}");
        }

        public void Dispose()
        {
            _logger.Info(message: $"End: ${_methodName}.");
        }
    }
}
