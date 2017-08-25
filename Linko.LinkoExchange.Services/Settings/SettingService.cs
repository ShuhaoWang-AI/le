using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.Settings
{
    public class SettingService : ISettingService
    {
        #region fields

        private readonly IRequestCache _cache;

        private readonly LinkoExchangeContext _dbContext;
        private readonly IGlobalSettings _globalSettings;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;

        #endregion

        #region constructors and destructor

        public SettingService(LinkoExchangeContext dbContext, ILogger logger, IMapHelper mapHelper, IRequestCache cache, IGlobalSettings globalSettings)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapHelper = mapHelper;
            _cache = cache;
            _globalSettings = globalSettings;
        }

        #endregion

        #region interface implementations

        /// <summary>
        ///     Get organization settings for a collection of organization Ids
        /// </summary>
        /// <param name="organizationIds"> The organization Ids. </param>
        /// <returns> Collection of organization settings </returns>
        public ICollection<OrganizationSettingDto> GetOrganizationSettingsByIds(IEnumerable<int> organizationIds)
        {
            var orgSettingsDtoList = new List<OrganizationSettingDto>();
            if (organizationIds != null)
            {
                foreach (var orgId in organizationIds)
                {
                    var orgSettingDto = GetOrganizationSettingsById(organizationId:orgId);
                    orgSettingsDtoList.Add(item:orgSettingDto);
                }
            }

            return orgSettingsDtoList;
        }

        public OrganizationSettingDto GetOrganizationSettingsById(int organizationId)
        {
            var orgSettingDto = new OrganizationSettingDto {OrganizationId = organizationId};
            orgSettingDto.Settings = new List<SettingDto>();

            //Get Organization settings first
            var orgSettings = _dbContext.OrganizationSettings.Include(s => s.SettingTemplate.OrganizationType)
                                        .Where(o => o.OrganizationId == organizationId);
            foreach (var orgSetting in orgSettings)
            {
                orgSettingDto.Settings.Add(item:_mapHelper.GetSettingDtoFromOrganizationSetting(setting:orgSetting));
            }

            return orgSettingDto;
        }

        /// <summary>
        ///     Get settings for one program
        /// </summary>
        /// <param name="orgRegProgramId"> The program Id to get for </param>
        /// <returns> The ProgramSetting object </returns>
        public ProgramSettingDto GetProgramSettingsById(int orgRegProgramId)
        {
            var progSettingDto = new ProgramSettingDto {OrgRegProgId = orgRegProgramId};
            progSettingDto.Settings = new List<SettingDto>();
            var settings = _dbContext.OrganizationRegulatoryProgramSettings.Where(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            foreach (var setting in settings)
            {
                progSettingDto.Settings.Add(item:_mapHelper.GetSettingDtoFromOrganizationRegulatoryProgramSetting(setting:setting));
            }

            return progSettingDto;
        }

        /// <summary>
        ///     Get settings for one program. If industry it will find the authority first and return the settings for the authority
        /// </summary>
        /// <param name="orgRegProgramId"> The program Id to get for </param>
        /// <returns> The ProgramSetting object </returns>
        public ProgramSettingDto GetAuthorityProgramSettingsById(int orgRegProgramId)
        {
            var progSettingDto = new ProgramSettingDto {OrgRegProgId = orgRegProgramId};
            progSettingDto.Settings = new List<SettingDto>();

            var org = _dbContext.OrganizationRegulatoryPrograms.Include(path:"Organization.Jurisdiction")
                                .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            OrganizationRegulatoryProgram authority;
            if (org.RegulatorOrganization != null)
            {
                authority = _dbContext.OrganizationRegulatoryPrograms.Include(path:"Organization")
                                      .Single(o => o.OrganizationId == org.RegulatorOrganization.OrganizationId
                                                   && o.RegulatoryProgramId == org.RegulatoryProgramId);
            }
            else
            {
                authority = org;
            }

            var settings = _dbContext.OrganizationRegulatoryProgramSettings.Where(o => o.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId);
            foreach (var setting in settings)
            {
                progSettingDto.Settings.Add(item:_mapHelper.GetSettingDtoFromOrganizationRegulatoryProgramSetting(setting:setting));
            }

            return progSettingDto;
        }

        public void CreateOrUpdateProgramSettings(int orgRegProgId, IEnumerable<SettingDto> settingDtos)
        {
            var transaction = _dbContext.BeginTransaction();
            try
            {
                foreach (var settingDto in settingDtos)
                {
                    CreateOrUpdateProgramSetting(orgRegProgId:orgRegProgId, settingDto:settingDto);
                }

                transaction.Commit();
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
                _dbContext.OrganizationRegulatoryProgramSettings.Add(entity:newSetting);
            }
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // ReSharper disable once ArgumentsStyleNamedExpression
                _logger.Error(ex, message:ex.Message);
                var msg = $"Cannot create/update program setting '{settingDto.Description}.'";
                var violations = new List<RuleViolation> {new RuleViolation(propertyName:"", propertyValue:settingDto.Value, errorMessage:msg)};
                throw new RuleViolationException(message:msg, validationIssues:violations);
            }
        }

        public void CreateOrUpdateOrganizationSettings(int organizationId, IEnumerable<SettingDto> settingDtos)
        {
            var transaction = _dbContext.BeginTransaction();
            try
            {
                foreach (var settingDto in settingDtos)
                {
                    CreateOrUpdateOrganizationSetting(organizationId:organizationId, settingDto:settingDto);
                }

                transaction.Commit();
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
                _dbContext.OrganizationSettings.Add(entity:newSetting);
            }

            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // ReSharper disable once ArgumentsStyleNamedExpression
                _logger.Error(ex, message:ex.Message);
                var msg = $"Cannot create/update program setting '{settingDto.Description}.'";
                var violations = new List<RuleViolation> {new RuleViolation(propertyName:"", propertyValue:settingDto.Value, errorMessage:msg)};
                throw new RuleViolationException(message:msg, validationIssues:violations);
            }
        }

        public int PasswordLockoutHours()
        {
            return 24;
        }

        /// <summary>
        ///     Get application global settings.
        /// </summary>
        /// <returns> The settings for dictionary object </returns>
        public IDictionary<SystemSettingType, string> GetGlobalSettings()
        {
            return _globalSettings.GetGlobalSettings();
        }

        public string GetOrganizationSettingValueByUserId(int userProfileId, SettingType settingType, bool? isChooseMin, bool? isChooseMax)
        {
            var orgRegProgramIds = _dbContext.OrganizationRegulatoryProgramUsers
                                             .Where(o => o.UserProfileId == userProfileId)
                                             .Select(o => o.OrganizationRegulatoryProgramId).Distinct();

            var authorityOrgIds = new List<int>();
            foreach (var orgRegProgId in orgRegProgramIds)
            {
                var authority = GetAuthority(orgRegProgramId:orgRegProgId);
                if (!authorityOrgIds.Contains(item:authority.OrganizationId))
                {
                    authorityOrgIds.Add(item:authority.OrganizationId);
                }
            }

            if (authorityOrgIds.Any())
            {
                var orgSettingDtos = GetOrganizationSettingsByIds(organizationIds:authorityOrgIds);
                var settings = orgSettingDtos.SelectMany(o => o.Settings).Where(s => s.TemplateName == settingType).ToList();
                if (!settings.Any())
                {
                    throw new Exception(message:string.Format(format:"ERROR: Could not find organization settings for user profile id={0} and setting type={1}", arg0:userProfileId,
                                                              arg1:settingType));
                }
                else if (settings.Count() == 1)
                {
                    return settings.ElementAt(index:0).Value;
                }
                else
                {
                    if (isChooseMin.HasValue && isChooseMin.Value)
                    {
                        //can't use LINQ .Min and .Max b/c values are string! #argh
                        var minValue = int.MaxValue;
                        foreach (var setting in settings)
                        {
                            if (Convert.ToInt32(value:setting.Value) < minValue)
                            {
                                minValue = Convert.ToInt32(value:setting.Value);
                                break;
                            }
                        }

                        return minValue.ToString();
                    }
                    else if (isChooseMax.HasValue && isChooseMax.Value)
                    {
                        //can't use LINQ .Min and .Max b/c values are string! #argh
                        var maxValue = int.MinValue;
                        foreach (var setting in settings)
                        {
                            if (Convert.ToInt32(value:setting.Value) > maxValue)
                            {
                                maxValue = Convert.ToInt32(value:setting.Value);
                                break;
                            }
                        }

                        return maxValue.ToString();
                    }
                    else
                    {
                        return settings.ElementAt(index:0).Value; //Return first item by default
                    }
                }
            }
            else
            {
                throw new Exception(message:string.Format(format:"ERROR: Could not find any associated organizations for user profile id={0}", arg0:userProfileId));
            }
        }

        public OrganizationRegulatoryProgram GetAuthority(int? organizationId = null, int? regProgramId = null, int? orgRegProgramId = null)
        {
            OrganizationRegulatoryProgram thisOrgRegProgram;
            if (orgRegProgramId.HasValue)
            {
                thisOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                              .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId.Value);
            }
            else
            {
                thisOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                              .Single(o => o.OrganizationId == organizationId.Value && o.RegulatoryProgramId == regProgramId.Value);
            }

            OrganizationRegulatoryProgram authority;
            if (thisOrgRegProgram.RegulatorOrganization != null)
            {
                authority = _dbContext.OrganizationRegulatoryPrograms.Include(path:"Organization.OrganizationType")
                                      .Single(o => o.OrganizationId == thisOrgRegProgram.RegulatorOrganizationId
                                                   && o.RegulatoryProgramId == thisOrgRegProgram.RegulatoryProgramId);
            }
            else
            {
                authority = thisOrgRegProgram;
            }

            return authority;
        }

        public string GetOrganizationSettingValue(int orgRegProgramId, SettingType settingType)
        {
            var cacheKey = $"OrgRegProgramId-{orgRegProgramId}-{settingType}";
            if (_cache.GetValue(key:cacheKey) != null)
            {
                return (string) _cache.GetValue(key:cacheKey);
            }

            var authority = GetAuthority(orgRegProgramId:orgRegProgramId);
            var settingValue = _dbContext.OrganizationSettings
                                         .Single(s => s.OrganizationId == authority.OrganizationId
                                                      && s.SettingTemplate.Name == settingType.ToString()).Value;

            _cache.SetValue(key:cacheKey, value:settingValue);

            return settingValue;
        }

        public string GetOrganizationSettingValue(int organizationId, int regProgramId, SettingType settingType)
        {
            var authority = GetAuthority(organizationId:organizationId, regProgramId:regProgramId);
            return _dbContext.OrganizationSettings
                             .Single(s => s.OrganizationId == authority.OrganizationId
                                          && s.SettingTemplate.Name == settingType.ToString()).Value;
        }

        public string GetOrgRegProgramSettingValue(int orgRegProgramId, SettingType settingType)
        {
            var authority = GetAuthority(orgRegProgramId:orgRegProgramId);
            var orgTypeId = authority.Organization.OrganizationType.OrganizationTypeId;

            return _dbContext.OrganizationRegulatoryProgramSettings
                             .Single(s => s.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId
                                          && s.SettingTemplate.Name == settingType.ToString()
                                          && s.SettingTemplate.OrganizationTypeId == orgTypeId).Value;
        }

        public string GetSettingTemplateValue(SettingType settingType, OrganizationTypeName? orgType)
        {
            var settings = _dbContext.SettingTemplates.Include(path:"OrganizationType")
                                     .Where(i => i.Name == settingType.ToString());

            if (!settings.Any())
            {
                return string.Empty;
            }

            var setting = orgType.HasValue ? settings.Single(s => s.OrganizationType.Name == orgType.ToString()) : settings.First();

            return setting.DefaultValue;
        }

        #endregion

        public void CreateOrUpdateProgramSettings(ProgramSettingDto settingDtos)
        {
            var transaction = _dbContext.BeginTransaction();
            try
            {
                foreach (var settingDto in settingDtos.Settings)
                {
                    CreateOrUpdateProgramSetting(orgRegProgId:settingDtos.OrgRegProgId, settingDto:settingDto);
                }

                transaction.Commit();
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

        public void CreateOrUpdateOrganizationSettings(OrganizationSettingDto settingDtos)
        {
            var transaction = _dbContext.BeginTransaction();
            try
            {
                foreach (var settingDto in settingDtos.Settings)
                {
                    CreateOrUpdateOrganizationSetting(organizationId:settingDtos.OrganizationId, settingDto:settingDto);
                }

                transaction.Commit();
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
    }
}