using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Program;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class ProgramServiceTests
    {
        #region fields

        private ProgramService _programService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _programService = new ProgramService(applicationDbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString), mapHelper:new MapHelper());
        }

        [TestMethod]
        public void GetOrganizationRegulatoryProgram_TEST()
        {
            var result = _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:1);
        }

        [TestMethod]
        public void GetUserRegulatoryPrograms_by_users_email_TEST()
        {
            var result = _programService.GetUserRegulatoryPrograms(email:"test@test.com");
        }

        [TestMethod]
        public void GetUserRegulatoryPrograms_by_users_id_TEST()
        {
            var result = _programService.GetUserRegulatoryPrograms(userId:1);
        }

        [TestMethod]
        public void GetTraversedAuthorityList_TEST()
        {
            var result = _programService.GetTraversedAuthorityList(orgRegProgramId:13);
        }
    }
}