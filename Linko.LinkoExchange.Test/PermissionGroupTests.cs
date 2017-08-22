using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Program;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class PermissionGroupTests
    {
        #region fields

        private PermissionService _pService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _pService = new PermissionService(
                                              programService:new ProgramService(
                                                                                applicationDbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                                mapHelper:new MapHelper()
                                                                               ),
                                              dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                              mapHelper:new MapHelper()
                                             );
        }

        [TestMethod]
        public void GetRoles()
        {
            var roles = _pService.GetRoles(orgRegProgramId:1);
        }

        [TestMethod]
        public void GetApprovalPeople()
        {
            var dto = new OrganizationRegulatoryProgramDto
                      {
                          OrganizationId = 1000,
                          OrganizationRegulatoryProgramId = 1001
                      };

            var approvers = _pService.GetApprovalPeople(approverOrganizationRegulatoryProgram:dto, isInvitedToIndustry:true);
        }
    }
}