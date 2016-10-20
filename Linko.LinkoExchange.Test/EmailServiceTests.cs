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
using Linko.LinkoExchange.Services.RequestCache;
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

            var emailAuditLogService = new EmailAuditLogService(dbContext);

            var orpu = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                p => p.IsEnabled == true &&
                     p.IsRegistrationApproved == true &&
                     p.IsRemoved == false &&
                     p.OragnizationRegulatoryProgramId == 1 &&
                     p.UserProfileDto  == new UserDto
                     {
                         FirstName = "test first name",
                         LastName = "test last name"
                     }
            );
 
            var psMockObject = new Mock<IProgramService>();
            psMockObject.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>()))
                .Returns(new List<OrganizationRegulatoryProgramUserDto> {orpu});
 

            var globalSetting = new Dictionary<string, string>();
            globalSetting.Add("PasswordRequireLength", "6");
            globalSetting.Add("PasswordRequireDigit", "true");
            globalSetting.Add("PasswordExpiredDays", "90");
            globalSetting.Add("NumberOfPasswordsInHistory", "10");
            globalSetting.Add("supportPhoneNumber", "+1-604-418-3201");
            globalSetting.Add("supportEmail", "support@linkoExchange.com");
            globalSetting.Add("SystemEmailEmailAddress", "shuhao.wang@watertrax.com");
            globalSetting.Add("SystemEmailFirstName", "LinkoExchange ");
            globalSetting.Add("SystemEmailLastName", "System");
            globalSetting.Add("EmailServer", "wtraxadc2.watertrax.local"); 

            var settingService = Mock.Of<ISettingService>(
                s => s.GetGlobalSettings() == globalSetting
            );

            var requestCache = Mock.Of<IRequestCache>(i => 
               i.GetValue("EmailRecipientRegulatoryProgramId") == "1" && 
               i.GetValue("EmailRecipientOrganizationId") == "2" &&
               i.GetValue("EmailRecipientOrganizationId") == "3"
            );

            _emailService = new LinkoExchangeEmailService(dbContext,emailAuditLogService, psMockObject.Object, settingService, requestCache);
        } 

        [TestMethod]
        public void Test_Signature_SignatoryGranted()
        {
            var contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"},
                {"stateName", "BC"}
            };

            var receivers = new List<string> {"shuhao.wang@watertrax.com"};
            

            _emailService.SendEmail(receivers, EmailType.Signature_SignatoryGranted, contentReplacements);

        }
    }
}
