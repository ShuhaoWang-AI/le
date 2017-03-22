using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Parameter
{
    public class ParameterService : IParameterService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly IOrganizationService _orgService;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISettingService _settings;

        public ParameterService(LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IOrganizationService orgService,
            IMapHelper mapHelper,
            ILogger logger,
            ITimeZoneService timeZoneService,
            ISettingService settings)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _orgService = orgService;
            _mapHelper = mapHelper;
            _logger = logger;
            _timeZoneService = timeZoneService;
            _settings = settings;
        }

        /// <summary>
        /// Returns a complete list of parameters associated with this Organization Regulatory Program
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ParameterDto> GetGlobalParameters(string startsWith = null)
        {
            var authOrgRegProgramId = _orgService.GetAuthority(int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId))).OrganizationRegulatoryProgramId;
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var parameterDtos = new List<ParameterDto>();
            var foundParams = _dbContext.Parameters
                .Include(p => p.DefaultUnit)
                .Where(param => param.OrganizationRegulatoryProgramId == authOrgRegProgramId); // need to find authority OrganizationRegulatoryProgramId

            if (!string.IsNullOrEmpty(startsWith))
            {
                startsWith = startsWith.TrimStart();
                foundParams = foundParams.Where(param => param.Name.StartsWith(startsWith));
            }

            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            foreach (var parameter in foundParams.ToList())
            {
                var dto = _mapHelper.GetParameterDtoFromParameter(parameter);
                dto.LastModificationDateTimeLocal = _timeZoneService
                                        .GetLocalizedDateTimeUsingThisTimeZoneId((parameter.LastModificationDateTimeUtc.HasValue ? parameter.LastModificationDateTimeUtc.Value.DateTime
                                         : parameter.CreationDateTimeUtc.DateTime), timeZoneId);

                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == parameter.LastModifierUserId);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                parameterDtos.Add(dto);
            }
            return parameterDtos;
        }

        /// <summary>
        /// Returns all Parameter Groups associated with this Organization Regulatory Program
        /// including children parameters
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ParameterGroupDto> GetStaticParameterGroups()
        {
            var authOrgRegProgramId = _orgService.GetAuthority(int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId))).OrganizationRegulatoryProgramId;
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var parameterGroupDtos = new List<ParameterGroupDto>();
            var foundParamGroups = _dbContext.ParameterGroups
                .Include(param => param.ParameterGroupParameters)
                .Where(param => param.OrganizationRegulatoryProgramId == authOrgRegProgramId)
                .ToList();

            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            foreach (var paramGroup in foundParamGroups)
            {
                var dto = _mapHelper.GetParameterGroupDtoFromParameterGroup(paramGroup);

                //Set LastModificationDateTimeLocal
                dto.LastModificationDateTimeLocal = _timeZoneService
                        .GetLocalizedDateTimeUsingThisTimeZoneId((paramGroup.LastModificationDateTimeUtc.HasValue ? paramGroup.LastModificationDateTimeUtc.Value.DateTime
                         : paramGroup.CreationDateTimeUtc.DateTime), timeZoneId);

                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == paramGroup.LastModifierUserId);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";

                parameterGroupDtos.Add(dto);
            }
            return parameterGroupDtos;
        }

        /// <summary>
        /// Returns single Paramater Group associated with Id
        /// including children parameters
        /// </summary>
        /// <param name="parameterGroupId">Id</param>
        /// <returns></returns>
        public ParameterGroupDto GetParameterGroup(int parameterGroupId)
        {
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var foundParamGroup = _dbContext.ParameterGroups
                .Include(param => param.ParameterGroupParameters)
                .Single(param => param.ParameterGroupId == parameterGroupId);

            var parameterGroupDto = _mapHelper.GetParameterGroupDtoFromParameterGroup(foundParamGroup);
            
            //Set LastModificationDateTimeLocal
            parameterGroupDto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingSettingForThisOrg((foundParamGroup.LastModificationDateTimeUtc.HasValue ? foundParamGroup.LastModificationDateTimeUtc.Value.DateTime
                        : foundParamGroup.CreationDateTimeUtc.DateTime), currentOrgRegProgramId);

            var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == foundParamGroup.LastModifierUserId);
            parameterGroupDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";


            return parameterGroupDto;
        }

        /// <summary>
        /// If ParameterGroupId exists in passed in Dto, finds existing ParameterGroup to update 
        /// OR creates new object to persist.
        /// </summary>
        /// <param name="parameterGroup"></param>
        public void SaveParameterGroup(ParameterGroupDto parameterGroup)
        {
            var authOrgRegProgramId = _orgService.GetAuthority(int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId))).OrganizationRegulatoryProgramId; 
            var currentUserProfileId = int.Parse(_httpContext.GetClaimValue(CacheKey.UserProfileId));
            List<RuleViolation> validationIssues = new List<RuleViolation>();
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    if (string.IsNullOrEmpty(parameterGroup.Name))
                    {
                        string message = "Name is required.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }

                    //Find existing groups with same Name (UC-33-1 7.1)
                    string proposedParamGroupName = parameterGroup.Name.Trim().ToLower();
                    var paramGroupsWithMatchingName = _dbContext.ParameterGroups
                        .Where(param => param.Name.Trim().ToLower() == proposedParamGroupName
                                && param.OrganizationRegulatoryProgramId == authOrgRegProgramId);

                    //Make sure there is at least 1 parameter
                    if (parameterGroup.Parameters == null || parameterGroup.Parameters.Count() < 1)
                    {
                        string message = "At least 1 parameter must be added to the group.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }

                    //Make sure parameters are unique
                    var isDuplicates = parameterGroup.Parameters
                                        .GroupBy(p => p.ParameterId)
                                        .Select(grp => new { Count = grp.Count() })
                                        .Any(grp => grp.Count > 1);
                    if (isDuplicates)
                    {
                        string message = "Parameters added to the group must be unique.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }

                    ParameterGroup paramGroupToPersist = null;
                    if (parameterGroup.ParameterGroupId.HasValue && parameterGroup.ParameterGroupId.Value > 0)
                    {
                        //Ensure there are no other groups with same name
                        foreach (var paramGroupWithMatchingName in paramGroupsWithMatchingName)
                        {
                            if (paramGroupWithMatchingName.ParameterGroupId != parameterGroup.ParameterGroupId.Value)
                            {
                                string message = "A Parameter Group with that name already exists.  Please select another name.";
                                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                            }
                        }
                
                        //Update existing
                        paramGroupToPersist = _dbContext.ParameterGroups.Single(param => param.ParameterGroupId == parameterGroup.ParameterGroupId);

                        //First remove all children (parameter associations)
                        _dbContext.ParameterGroupParameters.RemoveRange(paramGroupToPersist.ParameterGroupParameters);

                        paramGroupToPersist = _mapHelper.GetParameterGroupFromParameterGroupDto(parameterGroup, paramGroupToPersist);
                        paramGroupToPersist.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                        paramGroupToPersist.LastModifierUserId = currentUserProfileId;
                    }
                    else
                    {
                        //Ensure there are no other groups with same name
                        if (paramGroupsWithMatchingName.Count() > 0)
                        {
                            string message = "A Parameter Group with that name already exists.  Please select another name.";
                            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                        }

                        //Get new
                        paramGroupToPersist = _mapHelper.GetParameterGroupFromParameterGroupDto(parameterGroup);
                        paramGroupToPersist.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                        paramGroupToPersist.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                        paramGroupToPersist.LastModifierUserId = currentUserProfileId;
                        _dbContext.ParameterGroups.Add(paramGroupToPersist);
                    }

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (RuleViolationException ex)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }

            }
           
            
            

        }

        /// <summary>
        /// Deletes associated child rows from tParameterGroupParameter before deleting 
        /// from tParamaterGroup
        /// </summary>
        /// <param name="parameterGroupId">Id</param>
        public void DeleteParameterGroup(int parameterGroupId)
        {
            using (var transaction = _dbContext.BeginTransaction())
            {
                try {

                    var childAssociations = _dbContext.ParameterGroupParameters
                        .Where(child => child.ParameterGroupId == parameterGroupId);

                    if (childAssociations.Count() > 0)
                    {
                        _dbContext.ParameterGroupParameters.RemoveRange(childAssociations);
                    }

                    var foundParameterGroup = _dbContext.ParameterGroups
                        .Single(pg => pg.ParameterGroupId == parameterGroupId);

                    _dbContext.ParameterGroups.Remove(foundParameterGroup);

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error occurred {0} ", String.Join("," + Environment.NewLine, errors));

                    transaction.Rollback();
                    throw;
                }
            }

        }

        public IEnumerable<ParameterGroupDto> GetAllParameterGroups(int monitoringPointId)
        {
            string monitoringPointAbbrv = _dbContext.MonitoringPoints
                                        .Single(mp => mp.MonitoringPointId == monitoringPointId).Name; //TO-DO: Is this the same as Abbreviation? Or do we take Id?
            //Static Groups
            var parameterGroupDtos = new List<ParameterGroupDto>();
            parameterGroupDtos = this.GetStaticParameterGroups().ToList();

            //Add Dyanamic Groups
            var uniqueNonNullFrequencies = _dbContext.MonitoringPointParameterLimits
                .Where(x => !string.IsNullOrEmpty(x.IUSampleFrequency) && x.IsRemoved == false)
                .Select(x => x.IUSampleFrequency)
                .Distinct();

            var uniqueCollectionMethods = _dbContext.MonitoringPointParameterLimits
                .Include(x => x.CollectionMethod)
                .Where(x => x.IsRemoved == false && x.CollectionMethod.IsRemoved == false)
                .Select(x => x.CollectionMethod.Name)
                .Distinct();

            foreach (var collectMethod in uniqueCollectionMethods)
            {
                foreach (var freq in uniqueNonNullFrequencies)
                {
                    //Add "<Frequency> + <Collection Method>" Groups
                    var dynamicFreqAndCollectMethodParamGroup = new ParameterGroupDto();
                    dynamicFreqAndCollectMethodParamGroup.Name = $"{freq} {collectMethod}";
                    dynamicFreqAndCollectMethodParamGroup.Description = $"All {freq} {collectMethod} parameters for Monitoring Point {monitoringPointAbbrv}";
                    dynamicFreqAndCollectMethodParamGroup.Parameters = new List<ParameterDto>();

                    //Add Parameters
                    var freqCollectParams = _dbContext.MonitoringPointParameterLimits
                                        .Include(p => p.Parameter)
                                        .Where(p => p.IUSampleFrequency == freq 
                                            && p.CollectionMethod.Name == collectMethod
                                            && p.IsRemoved == false
                                            && p.CollectionMethod.IsRemoved == false);

                    foreach (var mpParamLimit in freqCollectParams)
                    {
                        var param = _mapHelper.GetParameterDtoFromParameter(mpParamLimit.Parameter);

                        //TO-DO: Set concentration, mass loading, default units

                        dynamicFreqAndCollectMethodParamGroup.Parameters.Add(param);
                    }
                    parameterGroupDtos.Add(dynamicFreqAndCollectMethodParamGroup);
                }

                //Add All "<Collection Method>" Groups
                var dynamicAllCollectMethodParamGroup = new ParameterGroupDto();
                dynamicAllCollectMethodParamGroup.Name = $"All {collectMethod}'s";
                dynamicAllCollectMethodParamGroup.Description = $"All {collectMethod} parameters for Monitoring Point {monitoringPointAbbrv}";
                dynamicAllCollectMethodParamGroup.Parameters = new List<ParameterDto>();

                //Add Parameters
                var collectParams = _dbContext.MonitoringPointParameterLimits
                                       .Include(p => p.Parameter)
                                       .Where(p => p.CollectionMethod.Name == collectMethod 
                                            && p.IsRemoved == false 
                                            && p.CollectionMethod.IsRemoved == false);

                foreach (var mpParamLimit in collectParams)
                {
                    var param = _mapHelper.GetParameterDtoFromParameter(mpParamLimit.Parameter);

                    //TO-DO: Set concentration, mass loading, default units

                    dynamicAllCollectMethodParamGroup.Parameters.Add(param);
                }
                parameterGroupDtos.Add(dynamicAllCollectMethodParamGroup);

            }

            return parameterGroupDtos;
        }

    }
}
