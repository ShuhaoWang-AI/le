using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class JurisdictionTests
    {
        #region fields

        private JurisdictionService _jService;
        private Mock<ILogger> _logger;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _logger = new Mock<ILogger>();
            _jService = new JurisdictionService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString), mapHelper:new MapHelper(), logger:_logger.Object);
        }

        [TestMethod]
        public void GetStateProvs_Test()
        {
            var states = _jService.GetStateProvs(countryId:2);
        }

        [TestMethod]
        public void GetJurisdictionById_Test()
        {
            var state = _jService.GetJurisdictionById(jurisdictionId:34);
        }
    }
}