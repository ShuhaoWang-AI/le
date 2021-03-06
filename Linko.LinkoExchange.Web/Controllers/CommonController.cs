﻿using System.Web.Mvc;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class CommonController : Controller
    {
        #region fields

        private readonly ILogger _logger;

        #endregion

        #region constructors and destructor

        public CommonController(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region error handling

        /// <summary>
        ///     Custom error handler for unauthorized access
        /// </summary>
        public ActionResult Unauthorized()
        {
            Response.TrySkipIisCustomErrors = true;

            return View();
        }

        /// <summary>
        ///     Custom error handler for 404 page not found.
        /// </summary>
        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            _logger.Warn($"Page not found. Url: {Request.Url}");
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