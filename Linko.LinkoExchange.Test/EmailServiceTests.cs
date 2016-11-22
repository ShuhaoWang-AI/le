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
                     p.OrganizationRegulatoryProgramDto == new OrganizationRegulatoryProgramDto
                     {
                         OrganizationRegulatoryProgramId = 90001,
                         OrganizationId = 90000,
                         RegulatorOrganizationId = 90000
                     }  &&
                     p.InviterOrganizationRegulatoryProgramDto == new OrganizationRegulatoryProgramDto
                     {
                         OrganizationRegulatoryProgramId = 90001,
                         OrganizationId = 90000,
                         RegulatorOrganizationId = 90000
                     } &&

                     p.UserProfileDto == new UserDto
                     {
                         FirstName = "test first name",
                         LastName = "test last name",
                         UserName = "tes user name",
                         UserProfileId = 10000
                     }
            );

            var psMockObject = new Mock<IProgramService>();
            psMockObject.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>()))
                .Returns(new List<OrganizationRegulatoryProgramUserDto> {orpu});


            var globalSetting = new Dictionary<SystemSettingType, string>();
            globalSetting.Add(SystemSettingType.PasswordExpiredDays, "90");
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
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };

            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
           
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Registration_AuthorityRegistrationDenied, contentReplacements); 
        }


        [TestMethod]
        public void Test_Registration_IndustryRegistrationDenied()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                {"userName", "test-username"},
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"}, 
                {"stateName", "BC"},
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Registration_IndustryRegistrationDenied, contentReplacements); 
        }

        [TestMethod]
        public void Test_Registration_IndustryRegistrationApproved()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                {"userName", "test-username"},
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"},
                {"link", "http://localhost:71" },
                {"stateName", "BC"},
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Registration_IndustryRegistrationApproved, contentReplacements); 
        }

        [TestMethod]
        public void Test_Registration_AuthorityRegistrationApproved()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };

            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
                {"link", "http://localhost:71" },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Registration_AuthorityRegistrationApproved, contentReplacements);
             
        }

        [TestMethod]
        public void Test_Registration_AuthorityInviteIndustryUser()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };

            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"},
                {"stateName", "BC"},
                {"link", "http://localhost:71" },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" } 
            };

            _emailService.SendEmail(receivers, EmailType.Registration_AuthorityInviteIndustryUser, contentReplacements); 
        }

        [TestMethod]
        public void Test_UserAccess_AccountLockOut()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" }; 

            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"authorityName", "Grand Rapids"},  
                {"authoritySupportEmail", "linkoexchange@grand-rapids.mi.us" },
                {"authoritySupportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.UserAccess_AccountLockOut, contentReplacements); 
        }

        [TestMethod]
        public void Test_UserAccess_LockOutToSysAdmins()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"firstName", "Registrant_firstName" },
                {"lastName", "Registrant_lastName" },
                {"userName", "test-user-name" },
                {"email","locked-account-email@watertrax.com" },
                {"authorityName", "Grand Rapids"},
                {"link", "http://localhost:71" },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.UserAccess_LockOutToSysAdmins, contentReplacements); 
        }

        [TestMethod]
        public void Test_Registration_resetRequired()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"authorityName", "Grand Rapids"}, 
                {"link", "http://localhost:71" },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Registration_ResetRequired, contentReplacements); 
        }

        [TestMethod]
        public void Test_Registration_InviteAuthorityUser()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
                {"link", "http://localhost:71" },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Registration_InviteAuthorityUser, contentReplacements); 
        }

        [TestMethod]
        public void Test_Signature_SignatoryGranted()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"},
                {"stateName", "BC"},
                {"link", "http://localhost:71" },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Signature_SignatoryGranted, contentReplacements); 
        }

        [TestMethod]
        public void Test_Signature_SignatoryRevoked()
        {

            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"},
                {"stateName", "BC"},
                {"link", "http://localhost:71" },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Signature_SignatoryRevoked, contentReplacements); 
        } 

        [TestMethod]
        public void Test_Profile_KBQFailedLockOut()
        {
            var authorityList = Environment.NewLine +
                     "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 " + Environment.NewLine +
                     "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 ";

            List <string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"authorityList", authorityList},
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Profile_KBQFailedLockOut, contentReplacements); 
        } 

        [TestMethod]
        public void Test_Profile_PrifileChanged()
        {
            var authorityList = Environment.NewLine +
                    "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 " + Environment.NewLine +
                    "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 ";


            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                { "userName", "test-user-name" },
                { "authorityList", authorityList},
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }

            };
            _emailService.SendEmail(receivers, EmailType.Profile_ProfileChanged, contentReplacements); 
        }

        [TestMethod]
        public void Test_Profile_EmailChanged()
        {
            var authorityList = Environment.NewLine +
                     "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 " + Environment.NewLine +
                     "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 ";


            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                { "userName", "test-user-name" },
                { "oldEmail", "oldEmail@test.com" },
                { "newEmail", "newEmail@test.com" },
                { "authorityList", authorityList}, 
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }

            };

            _emailService.SendEmail(receivers, EmailType.Profile_EmailChanged, contentReplacements); 
        }

        [TestMethod]
        public void Test_Profile_KBQChanged()
        {
            var authorityList = Environment.NewLine +
                       "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 " + Environment.NewLine +
                       "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 ";

            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                { "userName", "test-user-name" },
                { "authorityList", authorityList}, 
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" } 
            };

            _emailService.SendEmail(receivers, EmailType.Profile_KBQChanged, contentReplacements); 
        }

        [TestMethod]
        public void Test_Profile_SecurityQuestionsChanged()
        {
            var authorityList = Environment.NewLine +
                                  "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 " + Environment.NewLine +
                                  "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 ";

            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                { "userName", "test-user-name" },
                { "authorityList", authorityList},
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Profile_SecurityQuestionsChanged, contentReplacements); 
        }

        [TestMethod]
        public void Test_Profile_PasswordChanged()
        {
            var authorityList = Environment.NewLine +
                    "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 " + Environment.NewLine +
                    "City of Grand Rapids at linkoexchange@grand-rapids.mi.us Or 616-456-3260 ";

            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                { "userName", "test-user-name" },
                { "authorityList", authorityList},
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Profile_PasswordChanged, contentReplacements);

        }

        [TestMethod]
        public void Test_ForgetPassword_ForgetPassword()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                { "link", "http://localhost:71?token=this-is-the-token"  },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }

            };

            _emailService.SendEmail(receivers, EmailType.ForgotPassword_ForgotPassword, contentReplacements);
        }

        [TestMethod]
        public void Test_Profile_ResetProfileRequired()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                { "link", "http://localhost:71"  },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }

            };

            _emailService.SendEmail(receivers, EmailType.Profile_ResetProfileRequired, contentReplacements); 
        }

        [TestMethod]
        public void Test_Registration_IndustryUserRegistrationPendingToApprovers()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                { "firstName","test-first-name" },
                {"lastName", "test-last-name" },
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"},
                {"stateName", "BC"},
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" } 
            };

            _emailService.SendEmail(receivers, EmailType.Registration_IndustryUserRegistrationPendingToApprovers, contentReplacements); 
        }

        [TestMethod]
        public void Test_Registration_AuthorityUserRegistrationPendingToApprovers()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                { "firstName","test-first-name" },
                {"lastName", "test-last-name" },

                { "link", "http://localhost:71"  },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }

            };

            _emailService.SendEmail(receivers, EmailType.Registration_AuthorityUserRegistrationPendingToApprovers, contentReplacements);
        }

        [TestMethod]
        public void Test_ForgotUserName_ForgotUserName()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                { "userName","test-userName"},
                { "link", "http://localhost:71"  },
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }

            };

            _emailService.SendEmail(receivers, EmailType.ForgotUserName_ForgotUserName, contentReplacements); 
        }
        
        [TestMethod]
        public void Test_Registration_RegistrationResetPending()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"authorityName", "Grand Rapids"},
                { "firstName","test-first-name" },
                {"lastName", "test-last-name" },

                { "supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" }
            };

            _emailService.SendEmail(receivers, EmailType.Registration_RegistrationResetPending, contentReplacements); 
        } 

        [TestMethod]
        public void Test_Registration_IndustryInviteIndustryUser()
        {
            List<string> receivers = new List<string> { "shuhao.wang@watertrax.com" };
            Dictionary<string, string> contentReplacements = new Dictionary<string, string>
            {
                {"organizationName", "Green Vally Plant"},
                {"authorityName", "Grand Rapids"},
                {"userName", "Shuhao Wang"},
                {"addressLine1", "1055 Pender Street"},
                {"cityName", "Vancouver"},
                {"link", "http://localhost:71" },
                {"stateName", "BC"}, 
                {"supportEmail", "support@linkoexchange.com" },
                {"supportPhoneNumber","616-456-3260" } 
            };

            _emailService.SendEmail(receivers, EmailType.Registration_IndustryInviteIndustryUser, contentReplacements); 
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
                {"phoneNumber","616-456-3260" },
                {"firstName", "Registrant_firstName" },
                {"lastName", "Registrant_lastName" }
            };

            _emailService.SendEmail(receivers, emailType, contentReplacements); 
        }
    }
}
