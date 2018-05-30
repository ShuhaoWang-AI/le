using System;
using System.Net;

namespace Linko.LinkoExchange.Core.Validation
{
    [Serializable]
    public class BadRequest : RuleViolationException
    {
        #region static fields and constants

        private const HttpStatusCode StatusCode = HttpStatusCode.BadRequest;

        #endregion

        #region constructors and destructor

        public BadRequest(string message) : base(StatusCode.ToString(),
                                                 new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message)) { }

        public BadRequest(string propertyName, object propertyValue, string message)
            : base(StatusCode.ToString(),
                   new RuleViolation(propertyName:propertyName, propertyValue:propertyValue, errorMessage:message)) { }

        #endregion

        #region public properties

        public HttpStatusCode HttpStatusCode => StatusCode;

        #endregion
    }
}