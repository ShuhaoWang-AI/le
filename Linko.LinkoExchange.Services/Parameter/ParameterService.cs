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
        /// Gets all parameters associated with this Authority with optional parameters to filter the returned collection
        /// </summary>
        /// <param name="startsWith">Optional parameter to filter the Parameter name using "Starts With" condition</param>
        /// <param name="monitoringPointId">Optional Monitoring Point parameter must be combined with the other
        /// optional parameter "sampleEndDateTimeUtc"</param>
        /// <param name="sampleEndDateTimeUtc">If monitoring point and sample end date/time are passed in,
        ///default unit gets overidden with monitoring point specific unit and default "Calc Mass" boolean is set
        ///for each child parameter that is associated with the monitoring point and effective date range.</param>
        /// <returns>A parameter group with children parameters some with potentially overidden default units</returns>
        public IEnumerable<ParameterDto> GetGlobalParameters(string startsWith = null, int? monitoringPointId = null, DateTimeOffset? sampleEndDateTimeUtc = null)
        {
            string monitoringPointIdString = string.Empty;
            if (monitoringPointId.HasValue)
            {
                monitoringPointIdString = monitoringPointId.Value.ToString();
            }
            else
            {
                monitoringPointIdString = "null";
            }

            _logger.Info($"Enter ParameterService.GetGlobalParameters. monitoringPointId.Value={monitoringPointIdString}");

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

                //If monitoring point and sample end datetime is passed in,
                //get Unit and Calc mass data if this parameter is associated with the monitoring point
                //and effective date range.
                if (monitoringPointId.HasValue && sampleEndDateTimeUtc.HasValue)
                {
                    UpdateParameterForMonitoringPoint(ref dto, monitoringPointId.Value, sampleEndDateTimeUtc.Value);
                }

                dto.LastModificationDateTimeLocal = _timeZoneService
                                        .GetLocalizedDateTimeUsingThisTimeZoneId((parameter.LastModificationDateTimeUtc.HasValue ? parameter.LastModificationDateTimeUtc.Value.UtcDateTime
                                         : parameter.CreationDateTimeUtc.UtcDateTime), timeZoneId);

                if (parameter.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == parameter.LastModifierUserId.Value);
                    dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    dto.LastModifierFullName = "N/A";
                }
                parameterDtos.Add(dto);
            }

            _logger.Info($"Leaving ParameterService.GetGlobalParameters. monitoringPointId.Value={monitoringPointIdString}, parameterDtos.Count={parameterDtos.Count()}");

            return parameterDtos;
        }

        /// <summary>
        /// Used to obtain a collection of Parameter Groups from the database that matches optionally passed in criteria
        /// </summary>
        /// <param name="monitoringPointId">Optional Monitoring Point parameter must be combined with the other
        /// optional parameter "sampleEndDateTimeUtc"</param>
        /// <param name="sampleEndDateTimeUtc">If monitoring point and sample end date/time are passed in,
        ///default unit gets overidden with monitoring point specific unit and default "Calc Mass" boolean is set
        ///for each child parameter that is associated with the monitoring point and effective date range.</param>
        /// <returns>Collection of parameter groups with children parameters some with potentially overidden default units</returns>
        public IEnumerable<ParameterGroupDto> GetStaticParameterGroups(int? monitoringPointId = null, DateTimeOffset? sampleEndDateTimeUtc = null)
        {
            string monitoringPointIdString = string.Empty;
            if (monitoringPointId.HasValue)
            {
                monitoringPointIdString = monitoringPointId.Value.ToString();
            }
            else
            {
                monitoringPointIdString = "null";
            }

            _logger.Info($"Enter ParameterService.GetStaticParameterGroups. monitoringPointId.Value={monitoringPointIdString}");

            var authOrgRegProgramId = _orgService.GetAuthority(int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId))).OrganizationRegulatoryProgramId;
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var parameterGroupDtos = new List<ParameterGroupDto>();
            var foundParamGroups = _dbContext.ParameterGroups
                .Include(param => param.ParameterGroupParameters)
                .Where(param => param.OrganizationRegulatoryProgramId == authOrgRegProgramId
                    && param.IsActive)
                .ToList();

            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            foreach (var paramGroup in foundParamGroups)
            {
                var paramGroupDto = _mapHelper.GetParameterGroupDtoFromParameterGroup(paramGroup);

                //If monitoring point and sample end datetime is passed in,
                //get Unit and Calc mass data if this parameter is associated with the monitoring point
                //and effective date range.
                if (monitoringPointId.HasValue && sampleEndDateTimeUtc.HasValue)
                {
                    for (int paramIndex = 0; paramIndex < paramGroupDto.Parameters.Count; paramIndex++)
                    {
                        var paramDto = paramGroupDto.Parameters.ElementAt(paramIndex);
                        UpdateParameterForMonitoringPoint(ref paramDto, monitoringPointId.Value, sampleEndDateTimeUtc.Value); // TODO: Need to reduce DB call inside the function
                    }
                }

                //Set LastModificationDateTimeLocal
                paramGroupDto.LastModificationDateTimeLocal = _timeZoneService
                        .GetLocalizedDateTimeUsingThisTimeZoneId((paramGroup.LastModificationDateTimeUtc.HasValue ? paramGroup.LastModificationDateTimeUtc.Value.UtcDateTime
                         : paramGroup.CreationDateTimeUtc.UtcDateTime), timeZoneId);

                if (paramGroup.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == paramGroup.LastModifierUserId.Value);
                    paramGroupDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    paramGroupDto.LastModifierFullName = "N/A";
                }

                parameterGroupDtos.Add(paramGroupDto);
            }

            _logger.Info($"Leaving ParameterService.GetStaticParameterGroups. monitoringPointId.Value={monitoringPointIdString}, parameterGroupDtos.Count={parameterGroupDtos.Count()}");

            return parameterGroupDtos;
        }

        /// <summary>
        /// Overrides the given parameter's default unit with one found for the parameter at a given monitoring point
        /// and effective date range. Also updates the default setting for IsCalcMassLoading based on "Mass Daily" limit(s) found
        /// </summary>
        /// <param name="paramDto"></param>
        /// <param name="monitoringPointId"></param>
        /// <param name="sampleEndDateTimeUtc"></param>
        private void UpdateParameterForMonitoringPoint(ref ParameterDto paramDto, int monitoringPointId, DateTimeOffset sampleEndDateTimeUtc)
        {
            var parameterId = paramDto.ParameterId;

            _logger.Info($"Enter ParameterService.UpdateParameterForMonitoringPoint. monitoringPointId={monitoringPointId}, parameterId={parameterId}");

            //Check MonitoringPointParameter table
            var foundMonitoringPointParameter = _dbContext.MonitoringPointParameters
                .Include(mppl => mppl.DefaultUnit)
                .FirstOrDefault(mppl => mppl.MonitoringPointId == monitoringPointId
                    && mppl.ParameterId == parameterId
                    && mppl.EffectiveDateTimeUtc <= sampleEndDateTimeUtc
                    && mppl.RetirementDateTimeUtc >= sampleEndDateTimeUtc);

            if (foundMonitoringPointParameter?.DefaultUnit != null)
            {
                paramDto.DefaultUnit = _mapHelper.GetUnitDtoFromUnit(foundMonitoringPointParameter.DefaultUnit);
                paramDto.IsCalcMassLoading = _dbContext.MonitoringPointParameterLimits
                    .Include(mppl => mppl.LimitBasis)
                    .Include(mppl => mppl.LimitType)
                    .Any(mppl => mppl.MonitoringPointParameterId == foundMonitoringPointParameter.MonitoringPointParameterId
                        && mppl.LimitBasis.Name == LimitBasisName.MassLoading.ToString() && mppl.LimitType.Name == LimitTypeName.Daily.ToString());

            }

            _logger.Info($"Leaving ParameterService.UpdateParameterForMonitoringPoint. monitoringPointId={monitoringPointId}, parameterId={parameterId}");

        }

        /// <summary>
        /// Used to read the details of a static ParameterGroup from the database along with
        /// Parameter children contained within.
        /// </summary>
        /// <param name="parameterGroupId">Id from tParameterGroup associated with Parameter Group to read</param>
        /// <returns></returns>
        public ParameterGroupDto GetParameterGroup(int parameterGroupId)
        {
            _logger.Info($"Enter ParameterService.GetParameterGroup. parameterGroupId={parameterGroupId}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var foundParamGroup = _dbContext.ParameterGroups
                .Include(param => param.ParameterGroupParameters.Select(i=>i.Parameter))
                .Single(param => param.ParameterGroupId == parameterGroupId);

            var parameterGroupDto = _mapHelper.GetParameterGroupDtoFromParameterGroup(foundParamGroup);
            
            //Set LastModificationDateTimeLocal
            parameterGroupDto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingSettingForThisOrg((foundParamGroup.LastModificationDateTimeUtc.HasValue ? foundParamGroup.LastModificationDateTimeUtc.Value.UtcDateTime
                        : foundParamGroup.CreationDateTimeUtc.UtcDateTime), currentOrgRegProgramId);

            if (foundParamGroup.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == foundParamGroup.LastModifierUserId.Value);
                parameterGroupDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                parameterGroupDto.LastModifierFullName = "N/A";
            }

            _logger.Info($"Leaving ParameterService.GetParameterGroup. parameterGroupId={parameterGroupId}");

            return parameterGroupDto;
        }

        /// <summary>
        /// Creates a new Parameter group or updates and existing one in the database.
        /// </summary>
        /// <param name="parameterGroup">Parameter group to create new or update if and Id is included</param>
        /// <returns>Existing Id or newly created Id from tParameterGroup</returns>
        public int SaveParameterGroup(ParameterGroupDto parameterGroup)
        {
            string parameterGroupIdString = string.Empty;
            if (parameterGroup.ParameterGroupId.HasValue)
            {
                parameterGroupIdString = parameterGroup.ParameterGroupId.Value.ToString();
            }
            else
            {
                parameterGroupIdString = "null";
            }

            _logger.Info($"Enter ParameterService.SaveParameterGroup. parameterGroup.ParameterGroupId.Value={parameterGroupIdString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId; 
            var parameterGroupIdToReturn = -1;
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
                                string message = "A Parameter Group with that name already exists. Please select another name.";
                                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                            }
                        }
                
                        //Update existing
                        paramGroupToPersist = _dbContext.ParameterGroups.Single(param => param.ParameterGroupId == parameterGroup.ParameterGroupId);

                        //First remove all children (parameter associations)
                        _dbContext.ParameterGroupParameters.RemoveRange(paramGroupToPersist.ParameterGroupParameters);

                        paramGroupToPersist = _mapHelper.GetParameterGroupFromParameterGroupDto(parameterGroup, paramGroupToPersist);
                        paramGroupToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        paramGroupToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
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
                        paramGroupToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        paramGroupToPersist.CreationDateTimeUtc = DateTimeOffset.Now;
                        paramGroupToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        paramGroupToPersist.LastModifierUserId = currentUserProfileId;
                        _dbContext.ParameterGroups.Add(paramGroupToPersist);
                    }

                    _dbContext.SaveChanges();

                    parameterGroupIdToReturn = paramGroupToPersist.ParameterGroupId;

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

            _logger.Info($"Leaving ParameterService.SaveParameterGroup. parameterGroupIdToReturn={parameterGroupIdToReturn}");

            return parameterGroupIdToReturn;

        }

        /// <summary>
        /// Removes a Parameter Group from the database
        /// </summary>
        /// <param name="parameterGroupId">ParameterGroupId from tParameterGroup of the Parameter Group to delete.</param>
        public void DeleteParameterGroup(int parameterGroupId)
        {
            _logger.Info($"Enter ParameterService.DeleteParameterGroup. parameterGroupId={parameterGroupId}");

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

            _logger.Info($"Leave ParameterService.DeleteParameterGroup. parameterGroupId={parameterGroupId}");

        }

        /// <summary>
        /// Gets a collection of both static and dynamic Parameter Groups associated with a Monitoring Point and
        /// a Sample End Date/time (Local will get converted to UTC for comparison against database items)
        /// </summary>
        /// <param name="monitoringPointId">Monitoring point that must be associated with a Sample</param>
        /// <param name="sampleEndDateTimeLocal">Sample end date/time, once converted to UTC will be used to get monitoring point
        /// specific parameter information if it falls between effective and retirement date/time values.</param>
        /// <returns>Static and Dynamic Parameter Groups</returns>
        public IEnumerable<ParameterGroupDto> GetAllParameterGroups(int monitoringPointId, DateTime sampleEndDateTimeLocal)
        {
            _logger.Info($"Enter ParameterService.GetAllParameterGroups. monitoringPointId={monitoringPointId}, sampleEndDateTimeLocal={sampleEndDateTimeLocal}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            var sampleEndDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(sampleEndDateTimeLocal, timeZoneId);
            string monitoringPointAbbrv = _dbContext.MonitoringPoints
                                        .Single(mp => mp.MonitoringPointId == monitoringPointId).Name; //TO-DO: Is this the same as Abbreviation? Or do we take Id?
            //Static Groups
            var parameterGroupDtos = new List<ParameterGroupDto>();
            parameterGroupDtos = this.GetStaticParameterGroups(monitoringPointId, sampleEndDateTimeLocal).ToList();

            //Add Dyanamic Groups
            var uniqueNonNullFrequencies = _dbContext.SampleFrequencies
                .Include(ss => ss.MonitoringPointParameter)
                .Where(ss => ss.MonitoringPointParameter.MonitoringPointId == monitoringPointId
                    && ss.MonitoringPointParameter.EffectiveDateTimeUtc <= sampleEndDateTimeUtc
                    && ss.MonitoringPointParameter.RetirementDateTimeUtc >= sampleEndDateTimeUtc
                    && !string.IsNullOrEmpty(ss.IUSampleFrequency))
                .Select(x => x.IUSampleFrequency)
                .Distinct()
                .ToList();

            var uniqueCollectionMethodIds = _dbContext.SampleFrequencies
                .Include(ss => ss.MonitoringPointParameter)
                .Where(ss => ss.MonitoringPointParameter.MonitoringPointId == monitoringPointId
                    && ss.MonitoringPointParameter.EffectiveDateTimeUtc <= sampleEndDateTimeUtc
                    && ss.MonitoringPointParameter.RetirementDateTimeUtc >= sampleEndDateTimeUtc)
                .Select(x => x.CollectionMethodId)
                .Distinct()
                .ToList();

            foreach (var collectMethodId in uniqueCollectionMethodIds)
            {
                var collectionMethodName = _dbContext.CollectionMethods
                    .Single(cm => cm.CollectionMethodId == collectMethodId).Name;

                foreach (var freq in uniqueNonNullFrequencies)
                {
                    //Add "<Frequency> + <Collection Method>" Groups
                    var dynamicFreqAndCollectMethodParamGroup = new ParameterGroupDto();
                    dynamicFreqAndCollectMethodParamGroup.Name = $"{freq} {collectionMethodName}";
                    dynamicFreqAndCollectMethodParamGroup.Description = $"All {freq} {collectionMethodName} parameters for Monitoring Point {monitoringPointAbbrv}";
                    dynamicFreqAndCollectMethodParamGroup.Parameters = new List<ParameterDto>();

                    //Add Parameters
                    var freqCollectParams = _dbContext.SampleFrequencies
                                        .Include(ss => ss.MonitoringPointParameter)
                                        .Include(ss => ss.CollectionMethod)
                                        .Include(ss => ss.MonitoringPointParameter.Parameter)
                                        .Where(ss => ss.MonitoringPointParameter.MonitoringPointId == monitoringPointId
                                            && ss.MonitoringPointParameter.EffectiveDateTimeUtc <= sampleEndDateTimeUtc
                                            && ss.MonitoringPointParameter.RetirementDateTimeUtc >= sampleEndDateTimeUtc
                                            && ss.IUSampleFrequency == freq
                                            && ss.CollectionMethodId == collectMethodId
                                            && ss.CollectionMethod.IsRemoved == false
                                            && ss.CollectionMethod.IsEnabled == true)
                                        .Select(ss => ss.MonitoringPointParameter.Parameter)
                                        .Distinct()
                                        .ToList();

                    foreach (var parameter in freqCollectParams.ToList())
                    {
                        var paramDto = _mapHelper.GetParameterDtoFromParameter(parameter);
                        UpdateParameterForMonitoringPoint(ref paramDto, monitoringPointId, sampleEndDateTimeUtc);

                        dynamicFreqAndCollectMethodParamGroup.Parameters.Add(paramDto);
                    }

                    if (dynamicFreqAndCollectMethodParamGroup.Parameters.Count() > 0)
                    {
                        parameterGroupDtos.Add(dynamicFreqAndCollectMethodParamGroup);
                    }
                }

                //Add All "<Collection Method>" Groups
                var dynamicAllCollectMethodParamGroup = new ParameterGroupDto();
                dynamicAllCollectMethodParamGroup.Name = $"All {collectionMethodName}'s";
                dynamicAllCollectMethodParamGroup.Description = $"All {collectionMethodName} parameters for Monitoring Point {monitoringPointAbbrv}";
                dynamicAllCollectMethodParamGroup.Parameters = new List<ParameterDto>();

                //Add Parameters
                var collectParams = _dbContext.SampleFrequencies
                                        .Include(ss => ss.MonitoringPointParameter)
                                        .Include(ss => ss.CollectionMethod)
                                        .Include(ss => ss.MonitoringPointParameter.Parameter)
                                        .Where(ss => ss.MonitoringPointParameter.MonitoringPointId == monitoringPointId
                                            && ss.MonitoringPointParameter.EffectiveDateTimeUtc <= sampleEndDateTimeUtc
                                            && ss.MonitoringPointParameter.RetirementDateTimeUtc >= sampleEndDateTimeUtc
                                            && ss.CollectionMethodId == collectMethodId
                                            && ss.CollectionMethod.IsRemoved == false
                                            && ss.CollectionMethod.IsEnabled == true)
                                        .Select(ss => ss.MonitoringPointParameter.Parameter)
                                        .Distinct()
                                        .ToList();

                foreach (var parameter in collectParams)
                {
                    var paramDto = _mapHelper.GetParameterDtoFromParameter(parameter);
                    UpdateParameterForMonitoringPoint(ref paramDto, monitoringPointId, sampleEndDateTimeUtc);

                    dynamicAllCollectMethodParamGroup.Parameters.Add(paramDto);
                }

                //No need to check if Group.Count > 0 because this is guaranteed
                parameterGroupDtos.Add(dynamicAllCollectMethodParamGroup);

            }

            _logger.Info($"Enter ParameterService.GetAllParameterGroups. monitoringPointId={monitoringPointId}, sampleEndDateTimeLocal={sampleEndDateTimeLocal}, parameterGroupDtos.Count={parameterGroupDtos.Count()}");

            return parameterGroupDtos;
        }

    }
}
