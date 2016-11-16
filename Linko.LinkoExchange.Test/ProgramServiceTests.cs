using System.Configuration;
using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Program;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class ProgramServiceTests
    {
        private ProgramService _programService;

        public ProgramServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramUserMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramMapProfile());
                cfg.AddProfile(new RegulatoryProgramMapperProfile());
                cfg.AddProfile(new PermissionGroupMapProfile());
                cfg.AddProfile(new JurisdictionMapProfile());
            }); 

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _programService = new ProgramService(new LinkoExchangeContext(connectionString), Mapper.Instance);
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

    }
}
