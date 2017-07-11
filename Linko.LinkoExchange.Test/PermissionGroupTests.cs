using System.Configuration;
using Linko.LinkoExchange.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Mapping;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class PermissionGroupTests
    {
        private PermissionService _pService;

        public PermissionGroupTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _pService = new PermissionService(new ProgramService(new LinkoExchangeContext(connectionString), new MapHelper()), new LinkoExchangeContext(connectionString), new MapHelper());
        }

        [TestMethod]
        public void GetRoles()
        {
            var roles = _pService.GetRoles(1);
        }

        [TestMethod]
        public void GetApprovalPeople()
        {
            var approvers = _pService.GetApprovalPeople(1);
        }
    }
}
