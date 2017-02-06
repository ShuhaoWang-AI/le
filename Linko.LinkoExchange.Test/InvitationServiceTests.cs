using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Invitation;
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
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.AuditLog;

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
        Mock<ICromerrAuditLogService> _cromerrLogger;

        public InvitationServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _sessionCache = new Mock<ISessionCache>();
            _httpContext.Setup(x => x.GetClaimValue(It.IsAny<string>())).Returns("1");
            _orgService = new Mock<IOrganizationService>();
            _timeZones = new Mock<ITimeZoneService>();
            _userManager = new Mock<ApplicationUserManager>();
            _qaService = new Mock<IQuestionAnswerService>();
            _httpContext = new Mock<IHttpContextService>();

            _logger = new Mock<ILogger>();
            _cromerrLogger = new Mock<ICromerrAuditLogService>();

            _httpContext.Setup(x => x.GetRequestBaseUrl()).Returns("http://test.linkoexchange.com/");

            invitationService = new InvitationService(
                new LinkoExchangeContext(connectionString),
                new SettingService(new LinkoExchangeContext(connectionString), _logger.Object, new MapHelper()),
                new UserService(new LinkoExchangeContext(connectionString),
                                new EmailAuditLogEntryDto(),
                                new PasswordHasher(),
                                new HttpContextService(),
                                _emailService,
                                _settingService,
                                _sessionCache.Object,
                                _orgService.Object,
                                _requestCache,
                                _timeZones.Object,
                                _qaService.Object,
                                _logger.Object,
                                new MapHelper(),
                                _cromerrLogger.Object),
                _requestCache,//new RequestCache(),
                _emailService,
                new OrganizationService(new LinkoExchangeContext(connectionString),
                                        new SettingService(new LinkoExchangeContext(connectionString), _logger.Object, new MapHelper()),
                                        new HttpContextService(),
                                        new JurisdictionService(new LinkoExchangeContext(connectionString), new MapHelper()), new MapHelper()),
                                        _httpContext.Object,
                                        _timeZones.Object,
                                        _logger.Object,
                                        _programService,
                                        new SessionCache(_httpContext.Object),
                                        new MapHelper(),
                _cromerrLogger.Object
                );
        }

        [TestMethod]
        public void CreateInvitation()
        {
            invitationService.CreateInvitation(new InvitationDto()
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
