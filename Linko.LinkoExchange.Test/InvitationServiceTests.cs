using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Invitation;
using AutoMapper;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Email;
using Moq;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Authentication;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class InvitationServiceTests
    {
        private InvitationService invitationService;
        IEmailService _emailService = Mock.Of<IEmailService>();
        IRequestCache _requestCache = Mock.Of<IRequestCache>();
        ISettingService _settingService = Mock.Of<ISettingService>();
        Mock<ISessionCache> _sessionCache;

        public InvitationServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                //cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
                cfg.AddProfile(new PermissionGroupMapProfile());
                cfg.AddProfile(new RegulatoryProgramMapperProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramUserDtoMapProfile());
                cfg.AddProfile(new SettingMapProfile());
            });

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _sessionCache = new Mock<ISessionCache>();
            _sessionCache.Setup(x => x.GetClaimValue(It.IsAny<string>())).Returns("1");

            invitationService = new InvitationService(
                new LinkoExchangeContext(connectionString),
                Mapper.Instance,
                new SettingService(new LinkoExchangeContext(connectionString), Mapper.Instance),
                new UserService(new LinkoExchangeContext(connectionString), new EmailAuditLogEntryDto(), new PasswordHasher(), Mapper.Instance, new HttpContextService(), _emailService, _settingService, _sessionCache.Object),
                _requestCache,//new RequestCache(),
                _emailService,
                new OrganizationService(new LinkoExchangeContext(connectionString), Mapper.Instance, new SettingService(new LinkoExchangeContext(connectionString), Mapper.Instance), new HttpContextService()),
                new HttpContextService());
        }

        [TestMethod]
        public void CreateInvitation()
        {
            invitationService.CreateInvitation(new InvitationDto()
                                                        { EmailAddress = "test@test.com",
                                                         FirstName = "Ryan",
                                                         LastName = "Lee",
                                                         InvitationId = Guid.NewGuid().ToString(),
                                                         InvitationDateTimeUtc = DateTimeOffset.Now,
                                                         SenderOrganizationRegulatoryProgramId = 1,
                                                         RecipientOrganizationRegulatoryProgramId = 1});
        }

        [TestMethod]
        public void GetInvitationsForOrgRegProgram()
        {
            var dto = invitationService.GetInvitationsForOrgRegProgram(7);
        }

        [TestMethod]
        public void SendUserInvite_AuthorityToAuthority_UnknownEmail()
        {
            var dto = invitationService.SendUserInvite(1, "unknown@test.com", "Bob", "Smith", InvitationType.AuthorityToAuthority);
        }

        [TestMethod]
        public void SendUserInvite_AuthorityToAuthority_EmailExists()
        {
            var dto = invitationService.SendUserInvite(1, "support@linkotechnology.com", "Jen", "Lee", InvitationType.AuthorityToAuthority);
        }

        [TestMethod]
        public void SendUserInvite_AuthorityToAuthority_EmailExistsInDifferentProgram()
        {
            var dto = invitationService.SendUserInvite(1, "jbourne@test.com", "Jen", "Lee", InvitationType.AuthorityToAuthority);
        }

    }
}
