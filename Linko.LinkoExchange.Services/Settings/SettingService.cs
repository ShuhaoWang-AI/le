using System.Collections.Generic;
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

        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogEntry _logger;
        private readonly IMapper _mapper;

        #endregion

        public SettingService(ApplicationDbContext dbContext, IAuditLogEntry logger, IMapper mapper)
        {
            _dbContext = dbContext;
            _logger = logger;
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

        public IEnumerable<OrganizationSettingDto> GetOrganizationSettingsByIds_actual(IEnumerable<int> organizationIds)
        {
            var orgSettingsDtoList = new List<OrganizationSettingDto>();
            if (organizationIds != null)
            {
                foreach (var orgId in organizationIds)
                {
                    var orgSettingDto = new OrganizationSettingDto() { OrganizationId = orgId };
                    var orgSettingDtoProgramSettings = new List<ProgramSettingDto>();
                    //Get Organization settings first
                    var org = _dbContext.Organizations.Single(o => o.OrganizationId == orgId);
                    foreach (var orgSetting in org.OrganizationSettings)
                    {
                        orgSettingDto = _mapper.Map<OrganizationSetting, OrganizationSettingDto>(orgSetting); 
                    }
                    orgSettingsDtoList.Add(orgSettingDto);

                    //Get settings for each program this Organization is involved with

                    var OrganizationRegulatoryPrograms = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.OrganizationId == orgId);
                    foreach (var OrganizationRegulatoryProgram in OrganizationRegulatoryPrograms)
                    {
                        //Get setting for this Regulatory Program
                        var programSettingDto = new ProgramSettingDto() { ProgramId = OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId };
                        var programSettingDtoSettings = new List<SettingDto>();
                        foreach (var OrganizationRegulatoryProgramSetting in OrganizationRegulatoryProgram.OrganizationRegulatoryProgramSettings)
                        {
                            var settingDto = _mapper.Map<OrganizationRegulatoryProgramSetting, SettingDto>(OrganizationRegulatoryProgramSetting);
                            programSettingDtoSettings.Add(settingDto);
                        }
                        programSettingDto.Settings = programSettingDtoSettings;
                        orgSettingDtoProgramSettings.Add(programSettingDto);
                    }
                    orgSettingDto.ProgramSettings = orgSettingDtoProgramSettings;
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

		/// <summary>
		/// Get settings for one program
		/// </summary>
		/// <param name="programId">The program Id to get for</param>
		/// <returns>The PrrogramSetting object</returns>
		public ProgramSettingDto GetProgramSettingsById(int programId)
		{
			throw new System.NotImplementedException();
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
