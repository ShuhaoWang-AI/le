using System;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
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
    public class InvitationServiceTests
    {
        #region fields

        private readonly IProgramService _programService = Mock.Of<IProgramService>();
        private readonly IRequestCache _requestCache = Mock.Of<IRequestCache>();
        private readonly ISettingService _settingService = Mock.Of<ISettingService>();

        private Mock<ICromerrAuditLogService> _cromerrLogger;
        private Mock<IHttpContextService> _httpContext;
        private InvitationService _invitationService;
        private Mock<IJurisdictionService> _jurisdiction;
        private Mock<ILinkoExchangeEmailService> _linkoExchangeEmailService;

        private Mock<ILogger> _logger;
        private Mock<IOrganizationService> _orgService;
        private Mock<IQuestionAnswerService> _qaService;
        private Mock<ISessionCache> _sessionCache;
        private Mock<ITimeZoneService> _timeZones;
        private Mock<ApplicationUserManager> _userManager;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _sessionCache = new Mock<ISessionCache>();

            _orgService = new Mock<IOrganizationService>();
            _timeZones = new Mock<ITimeZoneService>();
            _userManager = new Mock<ApplicationUserManager>();
            _qaService = new Mock<IQuestionAnswerService>();
            _httpContext = new Mock<IHttpContextService>();
            _jurisdiction = new Mock<IJurisdictionService>();

            _logger = new Mock<ILogger>();
            _cromerrLogger = new Mock<ICromerrAuditLogService>();
            _httpContext.Setup(x => x.GetClaimValue(It.IsAny<string>())).Returns(value:"1");
            _httpContext.Setup(x => x.GetRequestBaseUrl()).Returns(value:"http://localhost/");
            _linkoExchangeEmailService = new Mock<ILinkoExchangeEmailService>();

            _invitationService =
                new InvitationService(
                                      dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                      settingService:new SettingService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                        logger:_logger.Object, mapHelper:new MapHelper(), cache:new Mock<IRequestCache>().Object,
                                                                        globalSettings:new Mock<IGlobalSettings>().Object),
                                      userService:new UserService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                  httpContext:new HttpContextService(),
                                                                  settingService:_settingService,
                                                                  orgService:_orgService.Object,
                                                                  requestCache:_requestCache,
                                                                  timeZones:_timeZones.Object,
                                                                  logService:_logger.Object,
                                                                  mapHelper:new MapHelper(),
                                                                  crommerAuditLogService:_cromerrLogger.Object,
                                                                  linkoExchangeEmailService:_linkoExchangeEmailService.Object),
                                      requestCache:_requestCache, //new RequestCache(), 
                                      organizationService:new OrganizationService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                                  settingService:new
                                                                                      SettingService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                                                     logger:_logger.Object, mapHelper:new MapHelper(),
                                                                                                     cache:new Mock<IRequestCache>().Object,
                                                                                                     globalSettings:new Mock<IGlobalSettings>().Object),
                                                                                  httpContext:new HttpContextService(),
                                                                                  jurisdictionService:new
                                                                                      JurisdictionService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                                                          mapHelper:new MapHelper(), logger:_logger.Object),
                                                                                  timeZoneService:_timeZones.Object, mapHelper:new MapHelper()),
                                      httpContext:_httpContext.Object,
                                      timeZones:_timeZones.Object,
                                      logger:_logger.Object,
                                      programService:_programService,
                                      mapHelper:new MapHelper(),
                                      crommerAuditLogService:_cromerrLogger.Object,
                                      linkoExchangeEmailService:_linkoExchangeEmailService.Object,
                                      jurisdictionService:_jurisdiction.Object
                                     );
        }

        [TestMethod]
        public void CreateInvitation()
        {
            _invitationService.CreateInvitation(dto:new InvitationDto
                                                    {
                                                        EmailAddress = "test@test.com",
                                                        FirstName = "Ryan",
                                                        LastName = "Lee",
                                                        InvitationId = Guid.NewGuid().ToString(),
                                                        InvitationDateTimeUtc = DateTimeOffset.Now,
                                                        SenderOrganizationRegulatoryProgramId = 1,
                                                        RecipientOrganizationRegulatoryProgramId = 1
                                                    });
        }

        [TestMethod]
        public void GetInvitationsForOrgRegProgram()
        {
            var dto = _invitationService.GetInvitationsForOrgRegProgram(senderOrgRegProgramId:7, targetOrgRegProgramId:7);
        }

        [TestMethod]
        public void SendUserInvite_AuthorityToAuthority_UnknownEmail()
        {
            _invitationService.SendUserInvite(targetOrgRegProgramId:1, email:"unknown@test.com", firstName:"Bob", lastName:"Smith",
                                              invitationType:InvitationType.AuthorityToAuthority);
        }

        [TestMethod]
        public void SendUserInvite_AuthorityToAuthority_EmailExists()
        {
            _invitationService.SendUserInvite(targetOrgRegProgramId:1, email:"support@linkotechnology.com", firstName:"Jen", lastName:"Lee",
                                              invitationType:InvitationType.AuthorityToAuthority);
        }

        [TestMethod]
        public void SendUserInvite_AuthorityToAuthority_EmailExistsInDifferentProgram()
        {
            _invitationService.SendUserInvite(targetOrgRegProgramId:1, email:"jbourne@test.com", firstName:"Jen", lastName:"Lee",
                                              invitationType:InvitationType.AuthorityToAuthority);
        }
    }
}