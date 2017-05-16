using System.Web.Mvc;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class CommonController:Controller
    {
        private readonly ILogger _logger;

        #region constructor

        public CommonController(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region error handling

        /// <summary>
        ///     Custom error handler for 404 page not found.
        /// </summary>
        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;

            return View();
        }

        /// <summary>
        ///     Default error handler.
        /// </summary>
        public ActionResult DefaultError()
        {
            var errorInfo = ViewData.Model as HandleErrorInfo;
            if (errorInfo != null)
            {
                _logger.Error(value:errorInfo.Exception);
            }

            return View(viewName:"Error");
        }

        #endregion
    }
}