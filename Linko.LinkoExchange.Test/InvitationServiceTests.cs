﻿using System;
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
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.QuestionAnswer;
using NLog;
using Linko.LinkoExchange.Services.Program;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class InvitationServiceTests
    {
        private InvitationService invitationService;
        IEmailService _emailService = Mock.Of<IEmailService>();
        IRequestCache _requestCache = Mock.Of<IRequestCache>();
        ISettingService _settingService = Mock.Of<ISettingService>();
        IProgramService _programService = Mock.Of<IProgramService>();
        Mock<ISessionCache> _sessionCache;
        Mock<IOrganizationService> _orgService;
        Mock<ITimeZoneService> _timeZones;
        Mock<ApplicationUserManager> _userManager;
        Mock<IQuestionAnswerService> _qaService;
        Mock<IHttpContextService> _httpContext;
        
        Mock<ILogger> _logger;

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
                cfg.AddProfile(new OrganizationRegulatoryProgramUserMapProfile());
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
            _orgService = new Mock<IOrganizationService>();
            _timeZones = new Mock<ITimeZoneService>();
            _userManager = new Mock<ApplicationUserManager>();
            _qaService = new Mock<IQuestionAnswerService>();
            _httpContext = new Mock<IHttpContextService>();
             
            _logger = new Mock<ILogger>();

            _httpContext.Setup(x => x.GetRequestBaseUrl()).Returns("http://test.linkoexchange.com/");

            invitationService = new InvitationService(
                new LinkoExchangeContext(connectionString),
                Mapper.Instance,
                new SettingService(new LinkoExchangeContext(connectionString), Mapper.Instance, _logger.Object),
                new UserService(new LinkoExchangeContext(connectionString), new EmailAuditLogEntryDto(), new PasswordHasher(), Mapper.Instance, new HttpContextService(), _emailService, _settingService, _sessionCache.Object, _orgService.Object, _requestCache, _timeZones.Object, _qaService.Object),
                _requestCache,//new RequestCache(),
                _emailService,
                new OrganizationService(new LinkoExchangeContext(connectionString), Mapper.Instance,
                new SettingService(
                    new LinkoExchangeContext(connectionString),
                    Mapper.Instance,
                    _logger.Object),
                new HttpContextService(), new JurisdictionService(new LinkoExchangeContext(connectionString), Mapper.Instance)),
                _httpContext.Object, _timeZones.Object, _logger.Object,
                _programService, new SessionCache(_httpContext.Object)
                );
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
            var dto = invitationService.GetInvitationsForOrgRegProgram(7, 7);
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
