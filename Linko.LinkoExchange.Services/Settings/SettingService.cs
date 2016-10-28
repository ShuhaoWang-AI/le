using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Data;
using AutoMapper;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using System;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Validation;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using Linko.LinkoExchange.Core.Enum;

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

            _globalSettings.Add(SettingType.FailedPasswordAttemptMaxCount, "3");

            _globalSettings.Add(SettingType.PasswordHistoryMaxCount, "10");
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
        public ICollection<OrganizationSettingDto> GetOrganizationSettingsByIds(IEnumerable<int> organizationIds)
        {
            var orgSettingsDtoList = new List<OrganizationSettingDto>();
            if (organizationIds != null)
            {
                foreach (var orgId in organizationIds)
                {
                    var orgSettingDto = GetOrganizationSettingsById(orgId);
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
  //      public OrganizationSettingDto GetOrganizationSettingsById(int organizationId)
		//{
		//	return new OrganizationSettingDto
		//	{
		//		OrganizationId = 100,
		//		Settings = GetMockData()
		//	};
		//}
        public OrganizationSettingDto GetOrganizationSettingsById(int organizationId)
        //GetOrganizationSettingsById_actual(int organizationId)
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
            foreach (var settingDto in settingDtos.Settings)
            {
                CreateOrUpdateProgramSetting(settingDtos.OrgRegProgId, settingDto);
            }
        }

        public void CreateOrUpdateProgramSettings(int orgRegProgId, IEnumerable<SettingDto> settingDtos)
        {
            foreach (var settingDto in settingDtos)
            {
                CreateOrUpdateProgramSetting(orgRegProgId, settingDto);
            }
        }
        public void CreateOrUpdateProgramSetting(int orgRegProgId, SettingDto settingDto)
        {
            var existingSetting = _dbContext.OrganizationRegulatoryProgramSettings
                .SingleOrDefault(o => o.OrganizationRegulatoryProgramId == orgRegProgId
                 && o.SettingTemplateId == (int)settingDto.Type);

            if (existingSetting != null)
            {
                existingSetting.Value = settingDto.Value;
            }
            else
            {
                var newSetting = _dbContext.OrganizationRegulatoryProgramSettings.Create();
                newSetting.OrganizationRegulatoryProgramId = orgRegProgId;
                newSetting.SettingTemplateId = Convert.ToInt32(settingDto.Type);
                newSetting.Value = settingDto.Value;
                _dbContext.OrganizationRegulatoryProgramSettings.Add(newSetting);
            }

            _dbContext.SaveChanges();
        }
        public void CreateOrUpdateOrganizationSettings(OrganizationSettingDto settingDtos)
        {
            foreach (var settingDto in settingDtos.Settings)
            {
                CreateOrUpdateOrganizationSetting(settingDtos.OrganizationId, settingDto);
            }
        }
        public void CreateOrUpdateOrganizationSettings(int organizationId, IEnumerable<SettingDto> settingDtos)
        {
            foreach (var settingDto in settingDtos)
            {
                CreateOrUpdateOrganizationSetting(organizationId, settingDto);
            }
        }
        public void CreateOrUpdateOrganizationSetting(int organizationId, SettingDto settingDto)
        {
            var existingSetting = _dbContext.OrganizationSettings
                .SingleOrDefault(o => o.OrganizationId == organizationId
                 && o.SettingTemplateId == (int)settingDto.Type);

            if (existingSetting != null)
            {
                existingSetting.Value = settingDto.Value;
            }
            else
            {
                var newSetting = _dbContext.OrganizationSettings.Create();
                newSetting.OrganizationId = organizationId;
                newSetting.SettingTemplateId = Convert.ToInt32(settingDto.Type);
                newSetting.Value = settingDto.Value;
                _dbContext.OrganizationSettings.Add(newSetting);
            }

            _dbContext.SaveChanges();
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

        public string GetOrganizationSettingValueByUserId(int userProfileId, SettingType settingType, bool? isChooseMin, bool? isChooseMax)
        {
            var orgIds = _dbContext.OrganizationRegulatoryProgramUsers.Where(o => o.UserProfileId == userProfileId).Select(o => o.OrganizationRegulatoryProgram.OrganizationId).Distinct();
            if (orgIds != null)
            {
                var orgSettingDtos = this.GetOrganizationSettingsByIds(orgIds);
                var settings = orgSettingDtos.SelectMany(o => o.Settings).Where(s => s.Type == settingType);
                if (settings == null || settings.Count() < 1)
                    throw new Exception(string.Format("ERROR: Could not find organization settings for user profile id={0} and setting type={1}", userProfileId, settingType.ToString()));
                else if (settings.Count() == 1)
                    return settings.ElementAt(0).Value;
                else
                {
                    if (isChooseMin.HasValue && isChooseMin.Value)
                    {
                        //can't use LINQ .Min and .Max b/c values are string! #argh
                        int minValue = int.MaxValue;
                        foreach (var setting in settings)
                        {
                            if (Convert.ToInt32(setting.Value) < minValue)
                            {
                                minValue = Convert.ToInt32(setting.Value);
                                break;
                            }
                        }
                        return minValue.ToString();
                    }
                    else if (isChooseMin.HasValue && isChooseMin.Value)
                    {
                        //can't use LINQ .Min and .Max b/c values are string! #argh
                        int maxValue = int.MinValue;
                        foreach (var setting in settings)
                        {
                            if (Convert.ToInt32(setting.Value) > maxValue)
                            {
                                maxValue = Convert.ToInt32(setting.Value);
                                break;
                            }
                        }
                        return maxValue.ToString();
                    }
                    else
                    {
                        return settings.ElementAt(0).Value; //Return first item by default
                    }

                }
            }

            throw new Exception(string.Format("ERROR: Could not find any associated organizations for user profile id={0}", userProfileId));

        }

        public string GetOrganizationSettingValue(int organizationId, SettingType settingType)
        {
            return _dbContext.OrganizationSettings
               .Single(s => s.OrganizationId == organizationId
               && s.SettingTemplateId == (int)settingType).Value;
        }

        public string GetOrgRegProgramSettingValue(int orgRegProgramId, SettingType settingType)
        {
            try
            {

           return _dbContext.OrganizationRegulatoryProgramSettings
                .Single(s => s.OrganizationRegulatoryProgramId == orgRegProgramId
                && s.SettingTemplateId == (int)settingType).Value;

            }
            catch (DbEntityValidationException ex)
            {
                HandleEntityException(ex);
            }
            catch (Exception e)
            {
                var linkoException = new LinkoExchangeException();
                linkoException.ErrorType = LinkoExchangeError.OrganizationSetting;
                linkoException.Errors = new List<string> { e.Message };
                throw linkoException;
            }
            return null;
        } 

        private void HandleEntityException(DbEntityValidationException ex)
        {
            List<RuleViolation> validationIssues = new List<RuleViolation>();
            foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            {
                DbEntityEntry entry = item.Entry;
                string entityTypeName = entry.Entity.GetType().Name;

                foreach (DbValidationError subItem in item.ValidationErrors)
                {
                    string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                    validationIssues.Add(new RuleViolation(string.Empty, null, message));

                }
            }
            //_logger.Info("???");
            throw new RuleViolationException("Validation errors", validationIssues);
        } 
    }
}
