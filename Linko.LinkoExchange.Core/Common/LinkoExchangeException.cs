using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Common
{
    public class LinkoExchangeException : Exception 
    {
        public IEnumerable<string> Errors { get; set; } 
        public LinkoExchangeError ErrorType { get; set; } 
    }
}
