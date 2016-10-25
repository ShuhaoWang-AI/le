using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Invitation;
using AutoMapper;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.AspNet.Identity;
using System.Configuration;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class InvitationServiceTests
    {
        private InvitationService invitationService;
       
        public InvitationServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                //cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramUserDtoMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
                cfg.AddProfile(new SettingMapProfile());
            });

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            invitationService = new InvitationService(new LinkoExchangeContext(connectionString), Mapper.Instance);
        }

        [TestMethod]
        public void CreateInvitation()
        {
            invitationService.CreateInvitation(new InvitationDto()
                                                        { EmailAddress = "test@test.com",
                                                         FirstName = "Ryan",
                                                         LastName = "Lee",
                                                         InvitationId = Guid.NewGuid().ToString(),
                                                         InvitationDateTimeUtc = DateTime.Now,
                                                         SenderOrganizationRegulatoryProgramId = 1,
                                                         RecipientOrganizationRegulatoryProgramId = 1});
        }

        [TestMethod]
        public void GetInvitationsForOrgRegProgram()
        {
            var dto = invitationService.GetInvitationsForOrgRegProgram(1);
        }
    }
}
