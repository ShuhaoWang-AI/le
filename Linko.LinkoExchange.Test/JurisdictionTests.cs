using System.Configuration;
using Linko.LinkoExchange.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class JurisdictionTests
    {
        private JurisdictionService _jService;

        public JurisdictionTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _jService = new JurisdictionService(new LinkoExchangeContext(connectionString), new MapHelper());
        }

        [TestMethod]
        public void GetStateProvs_Test()
        {
            var states = _jService.GetStateProvs(2);
        }

        [TestMethod]
        public void GetJurisdictionById_Test()
        {
            var state = _jService.GetJurisdictionById(34);
        }

    }
}
