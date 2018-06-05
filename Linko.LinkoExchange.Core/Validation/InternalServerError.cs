using System;
using System.Collections.Generic;
using System.Net;

namespace Linko.LinkoExchange.Core.Validation
{
    [Serializable]
    public class InternalServerError : RuleViolationException
    {
        #region static fields and constants

        private const HttpStatusCode StatusCode = HttpStatusCode.InternalServerError;

        #endregion

        #region constructors and destructor

        public InternalServerError(string message, List<RuleViolation> validationIssues) : base(message:message, validationIssues:validationIssues) { }

        public InternalServerError(string propertyName, object propertyValue, string message)
            : this(message:StatusCode.ToString(),
                   validationIssues:new List<RuleViolation>
                                    {
                                        new RuleViolation(propertyName:propertyName, propertyValue:propertyValue, errorMessage:message)
                                    }) { }

        public InternalServerError(string message) : this(propertyName:string.Empty, propertyValue:null, message:message) { }

        #endregion

        #region public properties

        public HttpStatusCode HttpStatusCode => StatusCode;

        #endregion
    }
}