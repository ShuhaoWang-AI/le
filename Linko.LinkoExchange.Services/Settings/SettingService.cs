﻿using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using AutoMapper;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Services.Settings
{
	public class SettingService : ISettingService
	{
        #region private members

        private readonly LinkoExchangeContext _dbContext;
        //private readonly IAuditLogEntry _logger;
        private readonly IMapper _mapper;

        #endregion

        public SettingService(LinkoExchangeContext dbContext
            //, IAuditLogEntry logger
            , IMapper mapper)
        {
            _dbContext = dbContext;
         //   _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get organization settings for a collection of organization Ids
        /// </summary>
        /// <param name="organizationIds">The organization Ids.</param>
        /// <returns>Collection of organization settings</returns>
        public IEnumerable<OrganizationSettingDto> GetOrganizationSettingsByIds(IEnumerable<int> organizationIds)
		{
			return new[]
			{
				new OrganizationSettingDto
				{
					OrganizationId = 100,
					Settings = GetMockData()
				}
			};
		}

        public ICollection<OrganizationSettingDto> GetOrganizationSettingsByIds_actual(IEnumerable<int> organizationIds)
        {
            var orgSettingsDtoList = new List<OrganizationSettingDto>();
            if (organizationIds != null)
            {
                foreach (var orgId in organizationIds)
                {
                    var orgSettingDto = GetOrganizationSettingsById_actual(orgId);
                    orgSettingsDtoList.Add(orgSettingDto);
                }
            }
            return orgSettingsDtoList;
        }

        /// <summary>
        /// Get the organization settings by organization Id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns>OrganizationSettingDto object</returns>
        public OrganizationSettingDto GetOrganizationSettingsById(int organizationId)
		{
			return new OrganizationSettingDto
			{
				OrganizationId = 100,
				Settings = GetMockData()
			};
		}
        public OrganizationSettingDto GetOrganizationSettingsById_actual(int organizationId)
        {
            var orgSettingDto = new OrganizationSettingDto() { OrganizationId = organizationId };
            orgSettingDto.Settings = new List<SettingDto>();
            //Get Organization settings first
            var org = _dbContext.Organizations.Single(o => o.OrganizationId == organizationId);
            foreach (var orgSetting in org.OrganizationSettings)
            {
                orgSettingDto.Settings.Add(_mapper.Map<OrganizationSetting, SettingDto>(orgSetting));
            }

            //Get settings for each program this Organization is involved with
            orgSettingDto.ProgramSettings = new List<ProgramSettingDto>();
            var OrganizationRegulatoryPrograms = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.OrganizationId == organizationId);
            foreach (var OrganizationRegulatoryProgram in OrganizationRegulatoryPrograms)
            {
                //Get setting for this Regulatory Program
                var programSettingDto = new ProgramSettingDto() { ProgramId = OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId };
                programSettingDto.Settings = new List<SettingDto>();
                foreach (var OrganizationRegulatoryProgramSetting in OrganizationRegulatoryProgram.OrganizationRegulatoryProgramSettings)
                {
                    var settingDto = _mapper.Map<OrganizationRegulatoryProgramSetting, SettingDto>(OrganizationRegulatoryProgramSetting);
                    programSettingDto.Settings.Add(settingDto);
                }
                orgSettingDto.ProgramSettings.Add(programSettingDto);
            }

            return orgSettingDto;
        }


		/// <summary>
		/// Get settings for one program
		/// </summary>
		/// <param name="programId">The program Id to get for</param>
		/// <returns>The PrrogramSetting object</returns>
		public ProgramSettingDto GetProgramSettingsById(int programId)
		{
            var progSettingDto = new ProgramSettingDto() { ProgramId = programId };
            progSettingDto.Settings = new List<SettingDto>();
            var program = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == programId);
            foreach (var setting in program.OrganizationRegulatoryProgramSettings)
            {
                progSettingDto.Settings.Add(_mapper.Map<OrganizationRegulatoryProgramSetting, SettingDto>(setting));
            }
            return progSettingDto;
		}

		public IEnumerable<ProgramSettingDto> GetProgramSettingsByIds(IEnumerable<int> programIds)
		{
			return new[]
			{
				new ProgramSettingDto
				{
					ProgramId = 100,
					Settings = GetMockData()
				}
			};
		}
        public ICollection<ProgramSettingDto> GetProgramSettingsByIds_actual(IEnumerable<int> programIds)
        {
            var settingDtoList = new List<ProgramSettingDto>();
            if (programIds != null)
            {
                foreach (var programId in programIds)
                {
                    settingDtoList.Add(GetProgramSettingsById(programId));
                }
            }
            return settingDtoList;
        }

        /// <summary>
        /// Get application global settings.
        /// </summary>
        /// <returns>The settings for dictionary object</returns>
        public IDictionary<string, string> GetGlobalSettings()
		{
			//TODO: get system global settings
			var globalSetting = new Dictionary<string, string>();
			globalSetting.Add("PasswordRequireLength", "6");
			globalSetting.Add("PasswordRequireDigit", "true");
			globalSetting.Add("PasswordExpiredDays", "90");
            globalSetting.Add("NumberOfPasswordsInHistory", "10");

            globalSetting.Add("supportPhoneNumber", "+1-604-418-3201");
            globalSetting.Add("supportEmail", "support@linkoExchange.com");

            return globalSetting;
		}

		/// <summary>
		/// Get all organization settings for one user identified by userId.
		/// </summary>
		/// <param name="userId">The user id to get organization setting for.</param>
		/// <returns>A collection of organization settings</returns>
		public IEnumerable<OrganizationSettingDto> GetOrganizationSettingsByUserId(int userId)
		{
			return new[]
			{
				 new OrganizationSettingDto
				 {
					 OrganizationId= 100,
					 Settings =  GetMockData()
				 }
			 };
		}

		private SettingDto[] GetMockData()
		{
			return new[] {
						 new SettingDto
						 {
							 Name="PasswordRequireLength",
							 Value="6"
						 },

						 new SettingDto
						 {
							 Name="PasswordRequireDigit",
							 Value = "true"
						 },

						 new SettingDto
						 {
							 Name="PasswordRequireLowerCase",
							 Value = "true"
						 },

						 new SettingDto
						 {
							 Name="PasswordRequireUpperCase",
							 Value = "true"
						 },
						 new SettingDto
						 {
							 Name="UserLockoutEnabledByDefault",
							 Value = "true"
						 },

						 new SettingDto
						 {
							 Name = "DefaultAccountLockoutTimeSpan",   // lock out days
							 Value = "1"
						 },

						 new SettingDto
						 {
							 Name = "MaxFailedAccessAttemptsBeforeLockout",
							 Value = "2"
						 },

						 new SettingDto
						 {
							 Name="PasswordExpiredDays",
							 Value="90"
						 },

						 new SettingDto
						 {
							 Name="DaysBeforeRequirePasswordChanging",
							 Value="10"
						 },

                         new SettingDto
                         {
                             Name ="NumberOfPasswordsInHistory",
                             Value = "10"
                         }
				};
		}
	}
}
