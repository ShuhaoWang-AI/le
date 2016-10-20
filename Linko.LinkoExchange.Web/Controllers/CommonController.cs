using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Mvc;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class CommonController : Controller
    {
        private readonly ILogger _logger;

        private IEnumerable<Claim> claims;

        #region constructor

        public CommonController(ILogger logger)
        {
            this._logger = logger;
        }

        #endregion


        #region error handling

        /// <summary>
        /// Custom error handler for 404 page not found.
        /// </summary>
        public ActionResult PageNotFound()
        {
            this.Response.StatusCode = 404;
            this.Response.TrySkipIisCustomErrors = true;

            return View ();
        }

        /// <summary>
        /// Default error handler.
        /// </summary>
        public ActionResult DefaultError()
        {
            HandleErrorInfo errorInfo = ViewData.Model as HandleErrorInfo;
            _logger.Error (errorInfo.Exception, errorInfo.Exception.Message);

            return View (viewName: "Error");
        }
        #endregion
    }
}