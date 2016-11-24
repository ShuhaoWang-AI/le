using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using AutoMapper;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using NLog;

namespace Linko.LinkoExchange.Services.Settings
{
	public class SettingService : ISettingService
	{
        #region private members

        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

	    private  Dictionary<SystemSettingType, string> _globalSettings = new Dictionary<SystemSettingType, string>();


        #endregion

        public SettingService(LinkoExchangeContext dbContext, IMapper mapper, ILogger logger)
        {
            _dbContext = dbContext; 
            _mapper = mapper;
            _logger = logger;

            var systemSettings = _dbContext.SystemSettings.ToList(); 

            _globalSettings.Add(SystemSettingType.EmailServer, systemSettings.First(i => i.Name == SystemSettingType.EmailServer.ToString()).Value);
            _globalSettings.Add(SystemSettingType.PasswordExpiredDays, systemSettings.First(i => i.Name == SystemSettingType.PasswordExpiredDays.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SupportPhoneNumber, systemSettings.First(i => i.Name == SystemSettingType.SupportPhoneNumber.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SupportEmailAddress, systemSettings.First(i => i.Name == SystemSettingType.SupportEmailAddress.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SystemEmailEmailAddress, systemSettings.First(i => i.Name == SystemSettingType.SystemEmailEmailAddress.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SystemEmailFirstName, systemSettings.First(i => i.Name == SystemSettingType.SystemEmailFirstName.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SystemEmailLastName, systemSettings.First(i => i.Name == SystemSettingType.SystemEmailLastName.ToString()).Value);

            _globalSettings.Add(SystemSettingType.PasswordRequireDigit, systemSettings.First(i => i.Name == SystemSettingType.PasswordRequireDigit.ToString()).Value);
            _globalSettings.Add(SystemSettingType.PasswordRequiredLength, systemSettings.First(i => i.Name == SystemSettingType.PasswordRequiredLength.ToString()).Value); 
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

        public OrganizationSettingDto GetOrganizationSettingsById(int organizationId)
        {
            var orgSettingDto = new OrganizationSettingDto() { OrganizationId = organizationId };
            orgSettingDto.Settings = new List<SettingDto>();
            //Get Organization settings first
            var orgSettings = _dbContext.OrganizationSettings.Include(s => s.SettingTemplate.OrganizationType)
                .Where(o => o.OrganizationId == organizationId);
            foreach (var orgSetting in orgSettings)
            {
                orgSettingDto.Settings.Add(_mapper.Map<OrganizationSetting, SettingDto>(orgSetting));
            }

            ////Get settings for each program this Organization is involved with
            //orgSettingDto.ProgramSettings = new List<ProgramSettingDto>();
            //var OrganizationRegulatoryPrograms = _dbContext.OrganizationRegulatoryPrograms.Where(o => o.OrganizationId == organizationId);
            //if (OrganizationRegulatoryPrograms != null)
            //{
            //    foreach (var OrganizationRegulatoryProgram in OrganizationRegulatoryPrograms.ToList())
            //    {
            //        //Get setting for this Regulatory Program
            //        var programSettingDto = new ProgramSettingDto() { OrgRegProgId = OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId };
            //        programSettingDto.Settings = new List<SettingDto>();
            //        var organizationRegulatoryProgramSettings = _dbContext.OrganizationRegulatoryProgramSettings.Where(o => o.OrganizationRegulatoryProgramId == OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId);
            //        if (organizationRegulatoryProgramSettings != null)
            //        {
            //            foreach (var OrganizationRegulatoryProgramSetting in organizationRegulatoryProgramSettings.ToList())
            //            {
            //                var settingDto = _mapper.Map<OrganizationRegulatoryProgramSetting, SettingDto>(OrganizationRegulatoryProgramSetting);
            //                programSettingDto.Settings.Add(settingDto);
            //            }
            //        }
            //        orgSettingDto.ProgramSettings.Add(programSettingDto);
            //    }
            //}

            return orgSettingDto;
        }


		/// <summary>
		/// Get settings for one program
		/// </summary>
		/// <param name="programId">The program Id to get for</param>
		/// <returns>The PrrogramSetting object</returns>
		public ProgramSettingDto GetProgramSettingsById(int orgRegProgramId)
		{
            var progSettingDto = new ProgramSettingDto() { OrgRegProgId = orgRegProgramId };
            progSettingDto.Settings = new List<SettingDto>();
            var settings = _dbContext.OrganizationRegulatoryProgramSettings.Where(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            foreach (var setting in settings)
            {
                progSettingDto.Settings.Add(_mapper.Map<OrganizationRegulatoryProgramSetting, SettingDto>(setting));
            }
            return progSettingDto;
		}

        public void CreateOrUpdateProgramSettings(ProgramSettingDto settingDtos)
        {
            var transaction = _dbContext.BeginTransaction();
            try
            {
                foreach (var settingDto in settingDtos.Settings)
                {
                    CreateOrUpdateProgramSetting(settingDtos.OrgRegProgId, settingDto);
                }

                transaction.Commit();
            }
            catch (RuleViolationException rve)
            {
                transaction.Rollback();
                throw rve;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public void CreateOrUpdateProgramSettings(int orgRegProgId, IEnumerable<SettingDto> settingDtos)
        {
            var transaction = _dbContext.BeginTransaction();
            try
            {
                foreach (var settingDto in settingDtos)
                {
                    CreateOrUpdateProgramSetting(orgRegProgId, settingDto);
                }

                transaction.Commit();
            }
            catch (RuleViolationException rve)
            {
                transaction.Rollback();
                throw rve;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }
        public void CreateOrUpdateProgramSetting(int orgRegProgId, SettingDto settingDto)
        {
            var existingSetting = _dbContext.OrganizationRegulatoryProgramSettings
                .SingleOrDefault(o => o.OrganizationRegulatoryProgramId == orgRegProgId
                 && o.SettingTemplate.Name == settingDto.TemplateName.ToString() 
                 && o.SettingTemplate.OrganizationType.Name == settingDto.OrgTypeName.ToString());

            if (existingSetting != null)
            {
                existingSetting.Value = settingDto.Value;
            }
            else
            {
                var newSetting = _dbContext.OrganizationRegulatoryProgramSettings.Create();
                newSetting.OrganizationRegulatoryProgramId = orgRegProgId;
                newSetting.SettingTemplateId = _dbContext.SettingTemplates
                    .Single(s => s.Name == settingDto.TemplateName.ToString()
                    && s.OrganizationType.Name == settingDto.OrgTypeName.ToString()).SettingTemplateId;
                newSetting.Value = settingDto.Value;
                _dbContext.OrganizationRegulatoryProgramSettings.Add(newSetting);
            }
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                string msg = $"Cannot create/update program setting '{settingDto.TemplateName.ToString()}' to '{settingDto.Value}'" ;
                var violations = new List<RuleViolation>() { new RuleViolation("", settingDto.Value, msg) };
                throw new RuleViolationException(msg, violations);
            }
        }
        public void CreateOrUpdateOrganizationSettings(OrganizationSettingDto settingDtos)
        {
            var transaction = _dbContext.BeginTransaction();
            try
            {
                foreach (var settingDto in settingDtos.Settings)
                {
                    CreateOrUpdateOrganizationSetting(settingDtos.OrganizationId, settingDto);
                }

                transaction.Commit();
            }
            catch (RuleViolationException rve)
            {
                transaction.Rollback();
                throw rve;
            }
            catch
            {
                transaction.Rollback();
                throw ;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public void CreateOrUpdateOrganizationSettings(int organizationId, IEnumerable<SettingDto> settingDtos)
        {
            var transaction = _dbContext.BeginTransaction();
            try
            {
                foreach (var settingDto in settingDtos)
                {
                    CreateOrUpdateOrganizationSetting(organizationId, settingDto);
                }

                transaction.Commit();
            }
            catch (RuleViolationException rve)
            {
                transaction.Rollback();
                throw rve;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public void CreateOrUpdateOrganizationSetting(int organizationId, SettingDto settingDto)
        {
            var existingSetting = _dbContext.OrganizationSettings
                .SingleOrDefault(o => o.OrganizationId == organizationId
                 && o.SettingTemplate.Name == settingDto.TemplateName.ToString()
                 && o.SettingTemplate.OrganizationType.Name == settingDto.OrgTypeName.ToString());

            if (existingSetting != null)
            {
                existingSetting.Value = settingDto.Value;
            }
            else
            {
                var newSetting = _dbContext.OrganizationSettings.Create();
                newSetting.OrganizationId = organizationId;
                newSetting.SettingTemplateId = _dbContext.SettingTemplates
                    .Single(s => s.Name == settingDto.TemplateName.ToString() 
                    && s.OrganizationType.Name == settingDto.OrgTypeName.ToString()).SettingTemplateId;
                newSetting.Value = settingDto.Value;
                _dbContext.OrganizationSettings.Add(newSetting);
            }

            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                string msg = string.Format("Cannot create/update program setting '{0}' to '{1}'", settingDto.TemplateName.ToString(), settingDto.Value);
                var violations = new List<RuleViolation>() { new RuleViolation("", settingDto.Value, msg) };
                throw new RuleViolationException(msg, violations);
            }
        }
  
	    public int PasswordLockoutHours()
	    {
            return 24;
        }
        
        /// <summary>
        /// Get application global settings.
        /// </summary>
        /// <returns>The settings for dictionary object</returns>
        public IDictionary<SystemSettingType, string> GetGlobalSettings()
		{
            return _globalSettings;
		}

        public string GetOrganizationSettingValueByUserId(int userProfileId, SettingType settingType, bool? isChooseMin, bool? isChooseMax)
        {
            var orgIds = _dbContext.OrganizationRegulatoryProgramUsers.Where(o => o.UserProfileId == userProfileId).Select(o => o.OrganizationRegulatoryProgram.OrganizationId).Distinct();
            if (orgIds != null)
            {
                var orgSettingDtos = this.GetOrganizationSettingsByIds(orgIds);
                var settings = orgSettingDtos.SelectMany(o => o.Settings).Where(s => s.TemplateName == settingType);
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
            else
            {
                throw new Exception(string.Format("ERROR: Could not find any associated organizations for user profile id={0}", userProfileId));
            }


        }

        public string GetOrganizationSettingValue(int organizationId, SettingType settingType)
        {
            return _dbContext.OrganizationSettings
               .Single(s => s.OrganizationId == organizationId
               && s.SettingTemplate.Name == settingType.ToString()).Value;
        }

        public string GetOrgRegProgramSettingValue(int orgRegProgramId, SettingType settingType)
        {
            try
            {
                //get org type
                int orgTypeId = _dbContext.OrganizationRegulatoryPrograms.Include("Organization.OrganizationType")
                    .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId).Organization.OrganizationType.OrganizationTypeId;

           return _dbContext.OrganizationRegulatoryProgramSettings
                .Single(s => s.OrganizationRegulatoryProgramId == orgRegProgramId
                && s.SettingTemplate.Name == settingType.ToString() 
                && s.SettingTemplate.OrganizationTypeId == orgTypeId).Value;

            }
            catch (DbEntityValidationException ex)
            {
                HandleEntityException(ex);
            }
            catch
            {
                throw;
            }
            return null;
        } 

        public string GetSettingTemplateValue(SettingType settingType, OrganizationTypeName? orgType)
        {
            var settings = _dbContext.SettingTemplates.Include("OrganizationType")
                .Where(i => i.Name == settingType.ToString());

            if (settings == null || settings.Count() < 1)
                return string.Empty;

            SettingTemplate setting;
            if (orgType.HasValue)
            {
                setting = settings.Single(s => s.OrganizationType.Name == orgType.ToString());
            }
            else
            {
                setting = settings.First();
            }

            return setting.DefaultValue;
        }

        private void HandleEntityException(DbEntityValidationException ex)
        {
            _logger.Error(ex, ex.Message);

            List<RuleViolation> validationIssues = new List<RuleViolation>();
            foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
            {
                DbEntityEntry entry = item.Entry;
                string entityTypeName = entry.Entity.GetType().Name;

                foreach (DbValidationError subItem in item.ValidationErrors)
                {
                    string message = string.Format(format: "Error '{0}' occurred in {1} at {2}", arg0: subItem.ErrorMessage, arg1: entityTypeName, arg2: subItem.PropertyName);
                    validationIssues.Add(new RuleViolation(string.Empty, null, message));

                }
            }
            
            throw new RuleViolationException(message: "", validationIssues: validationIssues);
        }
    }
}
