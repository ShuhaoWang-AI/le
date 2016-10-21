using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using AutoMapper;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using System;
using Linko.LinkoExchange.Core.Common;

namespace Linko.LinkoExchange.Services.Settings
{
	public class SettingService : ISettingService
	{
        #region private members

        private readonly LinkoExchangeContext _dbContext;
        //private readonly IAuditLogEntry _logger;
        private readonly IMapper _mapper;

	    private  Dictionary<SettingType, string> _globalSettings = new Dictionary<SettingType, string>();


        #endregion

        public SettingService(LinkoExchangeContext dbContext
            //, IAuditLogEntry logger
            , IMapper mapper)
        {
            _dbContext = dbContext;
         //   _logger = logger;
            _mapper = mapper;


            //TODO: get system global settings 

            _globalSettings.Add(SettingType.PasswordRequireLength, "6");
            _globalSettings.Add(SettingType.PasswordRequireDigit, "true");
            _globalSettings.Add(SettingType.PasswordExpiredDays, "90");

            _globalSettings.Add(SettingType.PasswordHistoryCount, "10");
            _globalSettings.Add(SettingType.EmailServer, "6");
            _globalSettings.Add(SettingType.SupportPhoneNumber, "+1-604-418-3201");
            _globalSettings.Add(SettingType.SupportEmail, "support@linkoExchange.com");
            _globalSettings.Add(SettingType.SystemEmailEmailAddress, "shuhao.wang@watertrax.com");
            _globalSettings.Add(SettingType.SystemEmailFirstName, "LinkoExchange");
            _globalSettings.Add(SettingType.SystemEmailLastName, "System"); 

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

	    public bool PasswordRequireDigital()
	    {
            if (!_globalSettings.ContainsKey(SettingType.PasswordRequireDigit))
            {
                return false;
            }

            return _globalSettings[SettingType.PasswordRequireDigit] == "true";
        }

	    public bool PassowrdRequireLowerCase()
	    {
            if (!_globalSettings.ContainsKey(SettingType.PasswordRequireLowerCase))
            {
                return false;
            }

            return _globalSettings[SettingType.PasswordRequireLowerCase] == "true";
        }

	    public bool PasswordRequireUpperCase()
	    {
            if (!_globalSettings.ContainsKey(SettingType.PasswordRequireUpperCase))
            {
                return false;
            }

            return _globalSettings[SettingType.PasswordRequireLowerCase] == "true";
        }

	    public int PasswordRequireLength()
	    {
            var settingKey = SettingType.PasswordRequireLength;
            if (!_globalSettings.ContainsKey(settingKey))
            {
                return 6;
            }

            return ValueParser.TryParseInt(_globalSettings[settingKey], 6);
        }

	    public int PasswordLockoutHours()
	    {
            var settingType = SettingType.DefaultAccountLockoutTimeSpan;
            if (!_globalSettings.ContainsKey(settingType))
            {
                return 24;
            }

            return int.Parse(_globalSettings[settingType]);
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
        public IDictionary<SettingType, string> GetGlobalSettings()
		{
            return _globalSettings;
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
                             Type = SettingType.MaxFailedPasswordAttempts,
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
                             Type =SettingType.PasswordHistoryCount,
                             Value = "10"
                         }
				};
		}
	}
}
