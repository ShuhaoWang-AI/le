using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using AutoMapper;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using System;

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
            if (OrganizationRegulatoryPrograms != null)
            {
                foreach (var OrganizationRegulatoryProgram in OrganizationRegulatoryPrograms.ToList())
                {
                    //Get setting for this Regulatory Program
                    var programSettingDto = new ProgramSettingDto() { OrgRegProgId = OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId };
                    programSettingDto.Settings = new List<SettingDto>();
                    var organizationRegulatoryProgramSettings = _dbContext.OrganizationRegulatoryProgramSettings.Where(o => o.OrganizationRegulatoryProgramId == OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId);
                    if (organizationRegulatoryProgramSettings != null)
                    {
                        foreach (var OrganizationRegulatoryProgramSetting in organizationRegulatoryProgramSettings.ToList())
                        {
                            var settingDto = _mapper.Map<OrganizationRegulatoryProgramSetting, SettingDto>(OrganizationRegulatoryProgramSetting);
                            programSettingDto.Settings.Add(settingDto);
                        }
                    }
                    orgSettingDto.ProgramSettings.Add(programSettingDto);
                }
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
            var progSettingDto = new ProgramSettingDto() { OrgRegProgId = programId };
            progSettingDto.Settings = new List<SettingDto>();
            var settings = _dbContext.OrganizationRegulatoryProgramSettings.Where(o => o.OrganizationRegulatoryProgramId == programId);
            foreach (var setting in settings)
            {
                progSettingDto.Settings.Add(_mapper.Map<OrganizationRegulatoryProgramSetting, SettingDto>(setting));
            }
            return progSettingDto;
		}

        public void CreateOrUpdateProgramSettings(ProgramSettingDto settingDtos)
        {
            var existingSettings = _dbContext.OrganizationRegulatoryProgramSettings.Where(o => o.OrganizationRegulatoryProgramId == settingDtos.OrgRegProgId).ToArray();
            foreach (var settingDto in settingDtos.Settings)
            {
                var foundSetting = existingSettings != null ? existingSettings.SingleOrDefault(s => s.SettingTemplate.SettingTemplateId == Convert.ToInt32(settingDto.Type)) : null;
                if (foundSetting != null)
                {
                    foundSetting.Value = settingDto.Value;
                }
                else
                {
                    var newSetting = _dbContext.OrganizationRegulatoryProgramSettings.Create();
                    newSetting.OrganizationRegulatoryProgramId = settingDtos.OrgRegProgId;
                    newSetting.SettingTemplateId = Convert.ToInt32(settingDto.Type);
                    newSetting.Value = settingDto.Value;
                    _dbContext.OrganizationRegulatoryProgramSettings.Add(newSetting);
                }
                
            }
            _dbContext.SaveChanges();
        }

		public IEnumerable<ProgramSettingDto> GetProgramSettingsByIds(IEnumerable<int> orgRegProgIds)
		{
			return new[]
			{
				new ProgramSettingDto
				{
					OrgRegProgId = 100,
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

            globalSetting.Add("EmailServer", "wtraxadc2.watertrax.local");
            globalSetting.Add("supportPhoneNumber", "+1-604-418-3201");
            globalSetting.Add("supportEmail", "support@linkoExchange.com");

            globalSetting.Add("SystemEmailEmailAddress", "shuhao.wang@watertrax.com");
            globalSetting.Add("SystemEmailFirstName", "LinkoExchange ");
            globalSetting.Add("SystemEmailLastName", "System");

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
                             Type=SettingType.PasswordRequireLength,
							 Value="6"
						 },

						 new SettingDto
						 {
                             Type=SettingType.PasswordRequireDigit,
							 Value = "true"
						 },

						 new SettingDto
						 {
                             Type=SettingType.PasswordRequireLowerCase,
							 Value = "true"
						 },

						 new SettingDto
						 {
                             Type=SettingType.PasswordRequireUpperCase,
							 Value = "true"
						 },
						 new SettingDto
						 {
                             Type=SettingType.UserLockoutEnabledByDefault,
							 Value = "true"
						 },

						 new SettingDto
						 {
                             Type = SettingType.DefaultAccountLockoutTimeSpan,   // lock out days
							 Value = "1"
						 },

						 new SettingDto
						 {
                             Type = SettingType.MaxFailedAccessAttemptsBeforeLockout,
							 Value = "2"
						 },

						 new SettingDto
						 {
                             Type=SettingType.PasswordExpiredDays,
							 Value="90"
						 },

						 new SettingDto
						 {
                             Type=SettingType.DaysBeforeRequirePasswordChanging,
							 Value="10"
						 },

                         new SettingDto
                         {
                             Type =SettingType.NumberOfPasswordsInHistory,
                             Value = "10"
                         }
				};
		}
	}
}
