using System.Configuration;
using Linko.LinkoExchange.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class ProgramServiceTests
    {
        private ProgramService _programService;

        public ProgramServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _programService = new ProgramService(new LinkoExchangeContext(connectionString), new MapHelper());
        }

        [TestMethod]
        public void GetOrganizationRegulatoryProgram_TEST()
        {
            var result = _programService.GetOrganizationRegulatoryProgram(1);
        }

        [TestMethod]
        public void GetUserRegulatoryPrograms_by_users_email_TEST()
        {
            var result = _programService.GetUserRegulatoryPrograms("test@test.com");
        }

        [TestMethod]
        public void GetUserRegulatoryPrograms_by_users_id_TEST()
        {
            var result = _programService.GetUserRegulatoryPrograms(1);
        }

        [TestMethod]
        public void GetTraversedAuthorityList_TEST()
        {
            var result = _programService.GetTraversedAuthorityList(13);
        }

    }
}
