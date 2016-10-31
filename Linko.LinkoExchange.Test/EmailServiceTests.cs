using System.Collections.Generic;
using System.Configuration;
using AutoMapper;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class EmailServiceTests
    {
        private IEmailService _emailService;
        string connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;

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
          
            var dbContext = new LinkoExchangeContext(connectionString);
   
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


            var globalSetting = new Dictionary<SystemSettingType, string>();
            globalSetting.Add(SystemSettingType.PasswordRequiredLength, "6");
            globalSetting.Add(SystemSettingType.PasswordRequiredDigit, "true");
            globalSetting.Add(SystemSettingType.PasswordExpiredDays, "90");
            //globalSetting.Add(SystemSettingType.PasswordHistoryMaxCount, "10"); //Organization Setting
            globalSetting.Add(SystemSettingType.SupportPhoneNumber, "+1-604-418-3201");
            globalSetting.Add(SystemSettingType.SupportEmailAddress, "support@linkoExchange.com");
            globalSetting.Add(SystemSettingType.SystemEmailEmailAddress, "shuhao.wang@watertrax.com");
            globalSetting.Add(SystemSettingType.SystemEmailFirstName, "LinkoExchange ");
            globalSetting.Add(SystemSettingType.SystemEmailLastName, "System");
            globalSetting.Add(SystemSettingType.EmailServer, "wtraxadc2.watertrax.local");

            var settingService = Mock.Of<ISettingService>(
                s => s.GetGlobalSettings() == globalSetting
            );

            var requestCache = Mock.Of<IRequestCache>(i =>
                    i.GetValue("EmailRecipientRegulatoryProgramId") == "1" &&
                    i.GetValue("EmailRecipientOrganizationId") == "2" &&
                    i.GetValue("EmailRecipientOrganizationId") == "3" && 
                    i.GetValue("Token") == Guid.NewGuid().ToString() && 
                    i.GetValue(CacheKey.EmailRecipientUserProfileId) == "1000" && 
                    i.GetValue(CacheKey.EmailSenderUserProfileId) == "2000"
            );
             
            var emailAuditLogService = new EmailAuditLogService(dbContext, requestCache);


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
