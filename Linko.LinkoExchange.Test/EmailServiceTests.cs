using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting; 
using Moq;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class EmailServiceTests
    {
        private IEmailService _emailService;

        public EmailServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
            });

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            var dbContext = new LinkoExchangeContext(connectionString);

            var requestCacheT = Mock.Of<IRequestCache>(i => i.GetValue("Token") == "Fake token");

            var emailAuditLogService = new EmailAuditLogService(dbContext, requestCacheT);

            var orpu = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                p => p.IsEnabled == true &&
                     p.IsRegistrationApproved == true &&
                     p.IsRemoved == false &&
                     p.OrganizationRegulatoryProgramId == 1 &&
                     p.UserProfileDto == new UserDto
                     {
                         FirstName = "test first name",
                         LastName = "test last name"
                     }
            );

            var psMockObject = new Mock<IProgramService>();
            psMockObject.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>()))
                .Returns(new List<OrganizationRegulatoryProgramUserDto> {orpu});


            var globalSetting = new Dictionary<SettingType, string>();
            globalSetting.Add(SettingType.PasswordRequireLength, "6");
            globalSetting.Add(SettingType.PasswordRequireDigit, "true");
            globalSetting.Add(SettingType.PasswordExpiredDays, "90");
            globalSetting.Add(SettingType.PasswordHistoryCount, "10");
            globalSetting.Add(SettingType.SupportPhoneNumber, "+1-604-418-3201");
            globalSetting.Add(SettingType.SupportEmail, "support@linkoExchange.com");
            globalSetting.Add(SettingType.SystemEmailEmailAddress, "shuhao.wang@watertrax.com");
            globalSetting.Add(SettingType.SystemEmailFirstName, "LinkoExchange ");
            globalSetting.Add(SettingType.SystemEmailLastName, "System");
            globalSetting.Add(SettingType.EmailServer, "wtraxadc2.watertrax.local");

            var settingService = Mock.Of<ISettingService>(
                s => s.GetGlobalSettings() == globalSetting
            );

            var requestCache = Mock.Of<IRequestCache>(i =>
                    i.GetValue("EmailRecipientRegulatoryProgramId") == "1" &&
                    i.GetValue("EmailRecipientOrganizationId") == "2" &&
                    i.GetValue("EmailRecipientOrganizationId") == "3" && 
                    i.GetValue("Token") == "Fake token ..."
            );

            _emailService = new LinkoExchangeEmailService(dbContext, emailAuditLogService, psMockObject.Object,
                settingService, requestCache);
        }
 
        [TestMethod]
        public void Test_Registration_AuthorityRegistrationDenied()
        {
            SendEmail(EmailType.Registration_AuthorityRegistrationDenied);
        }


        [TestMethod]
        public void Test_Registration_IndustryRegistrationDenied()
        {
            SendEmail(EmailType.Registration_IndustryRegistrationDenied);
        }

        [TestMethod]
        public void Test_Registration_IndustryRegistrationApproved()
        {  
            SendEmail(EmailType.Registration_IndustryRegistrationApproved);
        }

        [TestMethod]
        public void Test_Registration_AuthorityRegistrationApproved()
        { 
            SendEmail(EmailType.Registration_AuthorityRegistrationApproved);
        }

        [TestMethod]
        public void Test_Registration_AuthorityInviteIndustryUser()
        {   
            SendEmail(EmailType.Registration_AuthorityInviteIndustryUser);
        }

        [TestMethod]
        public void Test_Signature_SignatoryGranted()
        {
            SendEmail(EmailType.Signature_SignatoryGranted);
        }


        [TestMethod]
        public void Test_Signature_SignatoryRevoked()
        {
            SendEmail(EmailType.Signature_SignatoryRevoked);
        }


        [TestMethod]
        public void Test_Registration_AuthorityUserRegistrationPendingToApprovers()
        {  

            SendEmail(EmailType.Registration_AuthorityUserRegistrationPendingToApprovers);
        }


        [TestMethod]
        public void Test_Registration_IndustryUserRegistrationPendingToApprovers()
        {   
            SendEmail(EmailType.Registration_IndustryUserRegistrationPendingToApprovers);
        }

        [TestMethod]
        public void Test_Registration_IndustryInviteIndustryUser()
        {
           
            SendEmail(EmailType.Registration_IndustryInviteIndustryUser);
        }
         

        private void SendEmail(EmailType emailType)
        {
            List<string> receivers = new List<string> {"shuhao.wang@watertrax.com"};

            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"},
                {"stateName", "BC"},
                {"link", "http://localhost:71" },
                {"emailAddress", "support@linkoexchange.com" },
                {"phoneNumber","Linko_Support_PhoneNumber" },
                {"firstName", "Registrant_firstName" },
                {"lastName", "Registrant_lastName" }
            };

            _emailService.SendEmail(receivers, emailType, contentReplacements);

        }
    }
}
