﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Invitation;
using AutoMapper;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Email;
using Moq;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class InvitationServiceTests
    {
        private InvitationService invitationService;
        IEmailService _emailService = Mock.Of<IEmailService>();

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
            invitationService = new InvitationService(
                new LinkoExchangeContext(connectionString),
                Mapper.Instance,
                new SettingService(new LinkoExchangeContext(connectionString), Mapper.Instance),
                new UserService(new LinkoExchangeContext(connectionString), new EmailAuditLogEntryDto(), new PasswordHasher(), Mapper.Instance, new HttpContextService()),
                new RequestCache(),
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
