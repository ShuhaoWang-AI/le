using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.Parameter
{
    public class ParameterService : BaseService, IParameterService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _orgService;
        private readonly ISettingService _settings;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region constructors and destructor

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

        #endregion

        #region interface implementations

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            var retVal = false;

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentPortalName = _httpContext.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetParameterGroup":
                {
                    //
                    //Authorize the correct Authority owner
                    //

                    var parameterGroupId = id[0];
                    var parameterGroup = _dbContext.ParameterGroups
                                                   .SingleOrDefault(pg => pg.ParameterGroupId == parameterGroupId);

                    if (currentPortalName.Equals(value:"authority"))
                    {
                        if (parameterGroup != null && parameterGroup.OrganizationRegulatoryProgramId == currentOrgRegProgramId)
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                    else
                    {
                        //
                        //Authorize Industries of the Authority owner only
                        //
                        var authorityOfCurrentUser = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId);
                        if (parameterGroup != null && parameterGroup.OrganizationRegulatoryProgramId == authorityOfCurrentUser.OrganizationRegulatoryProgramId)
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                }

                    break;

                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

        /// <summary>
        ///     Gets all parameters associated with this Authority with optional parameters to filter the returned collection
        /// </summary>
        /// <param name="startsWith"> Optional parameter to filter the Parameter name using "Starts With" condition </param>
        /// <param name="monitoringPointId">
        ///     Optional Monitoring Point parameter must be combined with the other
        ///     optional parameter "sampleEndDateTimeUtc"
        /// </param>
        /// <param name="sampleEndDateTimeLocal">
        ///     If monitoring point and sample end date/time are passed in,
        ///     default unit gets overridden with monitoring point specific unit and default "Calc Mass" boolean is set
        ///     for each child parameter that is associated with the monitoring point and effective date range.
        /// </param>
        /// <returns> A parameter group with children parameters some with potentially overridden default units </returns>
        public IEnumerable<ParameterDto> GetGlobalParameters(string startsWith = null, int? monitoringPointId = null, DateTime? sampleEndDateTimeLocal = null)
        {
            var monitoringPointIdString = string.Empty;
            if (monitoringPointId.HasValue)
            {
                monitoringPointIdString = monitoringPointId.Value.ToString();
            }
            else
            {
                monitoringPointIdString = "null";
            }

            _logger.Info(message:$"Enter ParameterService.GetGlobalParameters. monitoringPointId.Value={monitoringPointIdString}");

            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId)))
                                                 .OrganizationRegulatoryProgramId;
            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var parameterDtos = new List<ParameterDto>();
            var foundParams = _dbContext.Parameters
                                        .Include(p => p.DefaultUnit)
                                        .Where(param => !param.IsRemoved //excluded deleted parameters
                                                        && param.OrganizationRegulatoryProgramId == authOrgRegProgramId); // need to find authority OrganizationRegulatoryProgramId

            if (!string.IsNullOrEmpty(value:startsWith))
            {
                startsWith = startsWith.TrimStart();

                // ReSharper disable once ArgumentsStyleNamedExpression
                foundParams = foundParams.Where(param => param.Name.StartsWith(startsWith));
            }

            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
            foreach (var parameter in foundParams.ToList())
            {
                var dto = _mapHelper.GetParameterDtoFromParameter(parameter:parameter);

                //If monitoring point and sample end datetime is passed in,
                //get Unit and Calc mass data if this parameter is associated with the monitoring point
                //and effective date range.
                if (monitoringPointId.HasValue && sampleEndDateTimeLocal.HasValue)
                {
                    UpdateParameterForMonitoringPoint(paramDto:ref dto, monitoringPointId:monitoringPointId.Value, sampleEndDateTimeLocal:sampleEndDateTimeLocal.Value);
                }

                dto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:parameter.LastModificationDateTimeUtc.HasValue
                                                                             ? parameter.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                             : parameter.CreationDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

                if (parameter.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == parameter.LastModifierUserId.Value);
                    dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    dto.LastModifierFullName = "N/A";
                }
                parameterDtos.Add(item:dto);
            }

            _logger.Info(message:$"Leaving ParameterService.GetGlobalParameters. monitoringPointId.Value={monitoringPointIdString}, parameterDtos.Count={parameterDtos.Count()}");

            return parameterDtos;
        }

        /// <summary>
        ///     Used to obtain a collection of Parameter Groups from the database that matches optionally passed in criteria
        /// </summary>
        /// <param name="monitoringPointId">
        ///     Optional Monitoring Point parameter must be combined with the other
        ///     optional parameter "sampleEndDateTimeUtc"
        /// </param>
        /// <param name="sampleEndDateTimeLocal">
        ///     If monitoring point and sample end date/time are passed in,
        ///     default unit gets overridden with monitoring point specific unit and default "Calc Mass" boolean is set
        ///     for each child parameter that is associated with the monitoring point and effective date range.
        /// </param>
        /// <param name="isGetActiveOnly"> True when being called by IU for use in Sample creation. False when being called by Authority </param>
        /// <returns> Collection of parameter groups with children parameters some with potentially overridden default units </returns>
        public IEnumerable<ParameterGroupDto> GetStaticParameterGroups(int? monitoringPointId = null, DateTime? sampleEndDateTimeLocal = null, bool? isGetActiveOnly = null)
        {
            var monitoringPointIdString = string.Empty;
            if (monitoringPointId.HasValue)
            {
                monitoringPointIdString = monitoringPointId.Value.ToString();
            }
            else
            {
                monitoringPointIdString = "null";
            }

            _logger.Info(message:$"Enter ParameterService.GetStaticParameterGroups. monitoringPointId.Value={monitoringPointIdString}");

            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId)))
                                                 .OrganizationRegulatoryProgramId;
            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var parameterGroupDtos = new List<ParameterGroupDto>();
            var foundParamGroups = _dbContext.ParameterGroups
                                             .Include(param => param.ParameterGroupParameters)
                                             .Where(param => param.OrganizationRegulatoryProgramId == authOrgRegProgramId)
                                             .ToList();

            if (isGetActiveOnly.HasValue && isGetActiveOnly.Value)
            {
                foundParamGroups = foundParamGroups
                    .Where(param => param.IsActive
                                    && param.ParameterGroupParameters.Count(pgp => !pgp.Parameter.IsRemoved) > 0)
                    .ToList();
            }

            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
            foreach (var paramGroup in foundParamGroups)
            {
                var paramGroupDto = _mapHelper.GetParameterGroupDtoFromParameterGroup(parameterGroup:paramGroup);

                //If monitoring point and sample end datetime is passed in,
                //get Unit and Calc mass data if this parameter is associated with the monitoring point
                //and effective date range.
                if (monitoringPointId.HasValue && sampleEndDateTimeLocal.HasValue)
                {
                    for (var paramIndex = 0; paramIndex < paramGroupDto.Parameters.Count; paramIndex++)
                    {
                        var paramDto = paramGroupDto.Parameters.ElementAt(index:paramIndex);
                        UpdateParameterForMonitoringPoint(paramDto:ref paramDto, monitoringPointId:monitoringPointId.Value,
                                                          sampleEndDateTimeLocal:sampleEndDateTimeLocal.Value); // TODO: Need to reduce DB call inside the function
                    }
                }

                //Set LastModificationDateTimeLocal
                paramGroupDto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:paramGroup.LastModificationDateTimeUtc.HasValue
                                                                             ? paramGroup.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                             : paramGroup.CreationDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

                if (paramGroup.LastModifierUserId.HasValue)
                {
                    var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == paramGroup.LastModifierUserId.Value);
                    paramGroupDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
                }
                else
                {
                    paramGroupDto.LastModifierFullName = "N/A";
                }

                parameterGroupDtos.Add(item:paramGroupDto);
            }

            _logger.Info(message:
                         $"Leaving ParameterService.GetStaticParameterGroups. monitoringPointId.Value={monitoringPointIdString}, parameterGroupDtos.Count={parameterGroupDtos.Count()}");

            return parameterGroupDtos;
        }

        /// <summary>
        ///     Used to read the details of a static ParameterGroup from the database along with
        ///     Parameter children contained within.
        /// </summary>
        /// <param name="parameterGroupId"> Id from tParameterGroup associated with Parameter Group to read </param>
        /// <returns> </returns>
        public ParameterGroupDto GetParameterGroup(int parameterGroupId, bool isAuthorizationRequired = false)
        {
            _logger.Info(message:$"Enter ParameterService.GetParameterGroup. parameterGroupId={parameterGroupId}");

            if (isAuthorizationRequired && !CanUserExecuteApi(id:parameterGroupId))
            {
                throw new UnauthorizedAccessException();
            }

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var foundParamGroup = _dbContext.ParameterGroups
                                            .Include(param => param.ParameterGroupParameters.Select(i => i.Parameter))
                                            .Single(param => param.ParameterGroupId == parameterGroupId);

            var parameterGroupDto = _mapHelper.GetParameterGroupDtoFromParameterGroup(parameterGroup:foundParamGroup);

            //Set LastModificationDateTimeLocal
            parameterGroupDto.LastModificationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:foundParamGroup.LastModificationDateTimeUtc.HasValue
                                                                            ? foundParamGroup.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                            : foundParamGroup.CreationDateTimeUtc.UtcDateTime, orgRegProgramId:currentOrgRegProgramId);

            if (foundParamGroup.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == foundParamGroup.LastModifierUserId.Value);
                parameterGroupDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                parameterGroupDto.LastModifierFullName = "N/A";
            }

            _logger.Info(message:$"Leaving ParameterService.GetParameterGroup. parameterGroupId={parameterGroupId}");

            return parameterGroupDto;
        }

        /// <summary>
        ///     Creates a new Parameter group or updates and existing one in the database.
        /// </summary>
        /// <param name="parameterGroup"> Parameter group to create new or update if and Id is included </param>
        /// <returns> Existing Id or newly created Id from tParameterGroup </returns>
        public int SaveParameterGroup(ParameterGroupDto parameterGroupDto)
        {
            var parameterGroupIdString = string.Empty;
            if (parameterGroupDto.ParameterGroupId.HasValue)
            {
                parameterGroupIdString = parameterGroupDto.ParameterGroupId.Value.ToString();
            }
            else
            {
                parameterGroupIdString = "null";
            }

            _logger.Info(message:$"Enter ParameterService.SaveParameterGroup. parameterGroup.ParameterGroupId.Value={parameterGroupIdString}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var parameterGroupIdToReturn = -1;
            var currentUserProfileId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.UserProfileId));
            var validationIssues = new List<RuleViolation>();

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(value:parameterGroupDto.Name))
                    {
                        var message = "Name is required.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    //Find existing groups with same Name (UC-33-1 7.1)
                    var proposedParamGroupName = parameterGroupDto.Name.Trim().ToLower();
                    var paramGroupsWithMatchingName = _dbContext.ParameterGroups
                                                                .Where(param => param.Name.Trim().ToLower() == proposedParamGroupName
                                                                                && param.OrganizationRegulatoryProgramId == authOrgRegProgramId);

                    //Make sure there is at least 1 parameter
                    if (parameterGroupDto.Parameters == null || parameterGroupDto.Parameters.Count() < 1)
                    {
                        var message = "At least 1 parameter must be added to the group.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    //Make sure parameters are unique
                    var isDuplicates = parameterGroupDto.Parameters
                                                        .GroupBy(p => p.ParameterId)
                                                        .Select(grp => new {Count = grp.Count()})
                                                        .Any(grp => grp.Count > 1);
                    if (isDuplicates)
                    {
                        var message = "Parameters added to the group must be unique.";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }

                    ParameterGroup paramGroupToPersist = null;
                    if (parameterGroupDto.ParameterGroupId.HasValue && parameterGroupDto.ParameterGroupId.Value > 0)
                    {
                        //Ensure there are no other groups with same name
                        foreach (var paramGroupWithMatchingName in paramGroupsWithMatchingName)
                        {
                            if (paramGroupWithMatchingName.ParameterGroupId != parameterGroupDto.ParameterGroupId.Value)
                            {
                                var message = "A Parameter Group with that name already exists. Please select another name.";
                                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                            }
                        }

                        //Update existing
                        paramGroupToPersist = _dbContext.ParameterGroups.Single(param => param.ParameterGroupId == parameterGroupDto.ParameterGroupId);

                        paramGroupToPersist = _mapHelper.GetParameterGroupFromParameterGroupDto(parameterGroupDto:parameterGroupDto, parameterGroup:paramGroupToPersist);
                        paramGroupToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        paramGroupToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        paramGroupToPersist.LastModifierUserId = currentUserProfileId;
                    }
                    else
                    {
                        //Ensure there are no other groups with same name
                        if (paramGroupsWithMatchingName.Count() > 0)
                        {
                            var message = "A Parameter Group with that name already exists.  Please select another name.";
                            validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                            throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                        }

                        //Get new
                        paramGroupToPersist = _mapHelper.GetParameterGroupFromParameterGroupDto(parameterGroupDto:parameterGroupDto);
                        paramGroupToPersist.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                        paramGroupToPersist.CreationDateTimeUtc = DateTimeOffset.Now;
                        paramGroupToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        paramGroupToPersist.LastModifierUserId = currentUserProfileId;
                        _dbContext.ParameterGroups.Add(entity:paramGroupToPersist);
                    }

                    //
                    //Handle updating parameters
                    //
                    if (paramGroupToPersist.ParameterGroupParameters != null && parameterGroupDto.Parameters != null)
                    {
                        //1) Deletes
                        for (var i = 0; i < paramGroupToPersist.ParameterGroupParameters.Count(); i++)
                        {
                            var parameter = paramGroupToPersist.ParameterGroupParameters.ElementAt(index:i);
                            var isNotRemoved = parameterGroupDto.Parameters
                                                                .Any(param => param.ParameterId == parameter.ParameterId);
                            if (!isNotRemoved)
                            {
                                _dbContext.ParameterGroupParameters.Remove(entity:parameter);
                            }
                        }

                        //2) Additions
                        foreach (var dtoParameter in parameterGroupDto.Parameters)
                        {
                            if (!paramGroupToPersist.ParameterGroupParameters.Any(pgp => pgp.ParameterId == dtoParameter.ParameterId))
                            {
                                paramGroupToPersist.ParameterGroupParameters.Add(item:new ParameterGroupParameter {ParameterId = dtoParameter.ParameterId});
                            }
                        }
                    }

                    _dbContext.SaveChanges();

                    parameterGroupIdToReturn = paramGroupToPersist.ParameterGroupId;

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:$"Leaving ParameterService.SaveParameterGroup. parameterGroupIdToReturn={parameterGroupIdToReturn}");

            return parameterGroupIdToReturn;
        }

        /// <summary>
        ///     Removes a Parameter Group from the database
        /// </summary>
        /// <param name="parameterGroupId"> ParameterGroupId from tParameterGroup of the Parameter Group to delete. </param>
        public void DeleteParameterGroup(int parameterGroupId)
        {
            _logger.Info(message:$"Enter ParameterService.DeleteParameterGroup. parameterGroupId={parameterGroupId}");

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var childAssociations = _dbContext.ParameterGroupParameters
                                                      .Where(child => child.ParameterGroupId == parameterGroupId);

                    if (childAssociations.Count() > 0)
                    {
                        _dbContext.ParameterGroupParameters.RemoveRange(entities:childAssociations);
                    }

                    var foundParameterGroup = _dbContext.ParameterGroups
                                                        .Single(pg => pg.ParameterGroupId == parameterGroupId);

                    _dbContext.ParameterGroups.Remove(entity:foundParameterGroup);

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:$"Leave ParameterService.DeleteParameterGroup. parameterGroupId={parameterGroupId}");
        }

        /// <summary>
        ///     Gets a collection of both static and dynamic Parameter Groups associated with a Monitoring Point and
        ///     a Sample End Date/time (Local will get converted to UTC for comparison against database items)
        /// </summary>
        /// <param name="monitoringPointId"> Monitoring point that must be associated with a Sample </param>
        /// <param name="sampleEndDateTimeLocal">
        ///     Sample end date/time, once converted to UTC will be used to get monitoring point
        ///     specific parameter information if it falls between effective and retirement date/time values.
        /// </param>
        /// <returns> Static and Dynamic Parameter Groups </returns>
        public IEnumerable<ParameterGroupDto> GetAllParameterGroups(int monitoringPointId, DateTime sampleEndDateTimeLocal)
        {
            _logger.Info(message:$"Enter ParameterService.GetAllParameterGroups. monitoringPointId={monitoringPointId}, sampleEndDateTimeLocal={sampleEndDateTimeLocal}");

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(value:_settings.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
            var monitoringPointAbbrv = _dbContext.MonitoringPoints
                                                 .Single(mp => mp.MonitoringPointId == monitoringPointId).Name; //TO-DO: Is this the same as Abbreviation? Or do we take Id?

             var authorityOrganizationId = _orgService.GetAuthority(orgRegProgramId:int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId)))
                                                 .OrganizationId;
            //Static Groups
            var parameterGroupDtos = new List<ParameterGroupDto>();
            parameterGroupDtos = GetStaticParameterGroups(monitoringPointId:monitoringPointId, sampleEndDateTimeLocal:sampleEndDateTimeLocal, isGetActiveOnly:true).ToList();

            //Add Dynamic Groups
            var uniqueNonNullFrequencies = _dbContext.SampleFrequencies
                                                     .Include(ss => ss.MonitoringPointParameter)
                                                     .Include(x=>x.MonitoringPointParameter) 
                                                     .Where(ss => ss.MonitoringPointParameter.MonitoringPointId == monitoringPointId
                                                                  && ss.MonitoringPointParameter.EffectiveDateTime <= sampleEndDateTimeLocal
                                                                  && ss.MonitoringPointParameter.RetirementDateTime >= sampleEndDateTimeLocal
                                                                  && ss.MonitoringPointParameter.MonitoringPoint.OrganizationRegulatoryProgram.RegulatorOrganizationId == authorityOrganizationId
                                                                  && ss.CollectionMethod.OrganizationId == authorityOrganizationId
                                                                  // ReSharper disable once ArgumentsStyleNamedExpression
                                                                  && !string.IsNullOrEmpty(ss.IUSampleFrequency)) 
                                                     .Select(x => x.IUSampleFrequency)
                                                     .Distinct()
                                                     .ToList();

            var uniqueCollectionMethodIds = _dbContext.SampleFrequencies
                                                      .Include(ss => ss.MonitoringPointParameter)
                                                      .Include(s=>s.MonitoringPointParameter)
                                                      .Where(ss => ss.MonitoringPointParameter.MonitoringPointId == monitoringPointId
                                                                   && ss.MonitoringPointParameter.EffectiveDateTime <= sampleEndDateTimeLocal
                                                                   && ss.MonitoringPointParameter.RetirementDateTime >= sampleEndDateTimeLocal
                                                                   && ss.MonitoringPointParameter.MonitoringPoint.OrganizationRegulatoryProgram.RegulatorOrganizationId == authorityOrganizationId
                                                                   && ss.CollectionMethod.OrganizationId == authorityOrganizationId
                                                                   )  
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
                                                                   && ss.MonitoringPointParameter.EffectiveDateTime <= sampleEndDateTimeLocal
                                                                   && ss.MonitoringPointParameter.RetirementDateTime >= sampleEndDateTimeLocal
                                                                   && ss.IUSampleFrequency == freq
                                                                   && ss.CollectionMethodId == collectMethodId
                                                                   && ss.CollectionMethod.IsRemoved == false
                                                                   && ss.CollectionMethod.IsEnabled)
                                                      .Select(ss => ss.MonitoringPointParameter.Parameter)
                                                      .Distinct()
                                                      .ToList();

                    foreach (var parameter in freqCollectParams.ToList())
                    {
                        var paramDto = _mapHelper.GetParameterDtoFromParameter(parameter:parameter);
                        UpdateParameterForMonitoringPoint(paramDto:ref paramDto, monitoringPointId:monitoringPointId, sampleEndDateTimeLocal:sampleEndDateTimeLocal);

                        dynamicFreqAndCollectMethodParamGroup.Parameters.Add(item:paramDto);
                    }

                    if (dynamicFreqAndCollectMethodParamGroup.Parameters.Count() > 0)
                    {
                        parameterGroupDtos.Add(item:dynamicFreqAndCollectMethodParamGroup);
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
                                                           && ss.MonitoringPointParameter.EffectiveDateTime <= sampleEndDateTimeLocal
                                                           && ss.MonitoringPointParameter.RetirementDateTime >= sampleEndDateTimeLocal
                                                           && ss.CollectionMethodId == collectMethodId
                                                           && ss.MonitoringPointParameter.MonitoringPoint.OrganizationRegulatoryProgram.RegulatorOrganizationId == authorityOrganizationId
                                                           && ss.CollectionMethod.IsRemoved == false
                                                           && ss.CollectionMethod.IsEnabled)
                                              .Select(ss => ss.MonitoringPointParameter.Parameter)
                                              .Distinct()
                                              .ToList();

                foreach (var parameter in collectParams)
                {
                    var paramDto = _mapHelper.GetParameterDtoFromParameter(parameter:parameter);
                    UpdateParameterForMonitoringPoint(paramDto:ref paramDto, monitoringPointId:monitoringPointId, sampleEndDateTimeLocal:sampleEndDateTimeLocal);

                    dynamicAllCollectMethodParamGroup.Parameters.Add(item:paramDto);
                }

                //No need to check if Group.Count > 0 because this is guaranteed
                parameterGroupDtos.Add(item:dynamicAllCollectMethodParamGroup);
            }

            _logger.Info(message:
                         $"Enter ParameterService.GetAllParameterGroups. monitoringPointId={monitoringPointId}, sampleEndDateTimeLocal={sampleEndDateTimeLocal}, parameterGroupDtos.Count={parameterGroupDtos.Count()}");

            return parameterGroupDtos;
        }

        #endregion

        /// <summary>
        /// Overrides the given parameter's default unit with one found for the parameter at a given monitoring point
        /// and effective date range. Also updates the default setting for IsCalcMassLoading based on "Mass Daily" limit(s) found
        /// </summary>
        /// <param name="paramDto"></param>
        /// <param name="monitoringPointId"></param>
        /// <param name="sampleEndDateTimeUtc"></param>
        private void UpdateParameterForMonitoringPoint(ref ParameterDto paramDto, int monitoringPointId, DateTime sampleEndDateTimeLocal)
        {
            var parameterId = paramDto.ParameterId;

            _logger.Info($"Enter ParameterService.UpdateParameterForMonitoringPoint. monitoringPointId={monitoringPointId}, parameterId={parameterId}");

            //Check MonitoringPointParameter table
            var foundMonitoringPointParameter = _dbContext.MonitoringPointParameters
                .Include(mppl => mppl.DefaultUnit)
                .FirstOrDefault(mppl => mppl.MonitoringPointId == monitoringPointId
                    && mppl.ParameterId == parameterId
                    && mppl.EffectiveDateTime <= sampleEndDateTimeLocal
                    && mppl.RetirementDateTime >= sampleEndDateTimeLocal);

            if (foundMonitoringPointParameter?.DefaultUnit != null)
            {
                paramDto.DefaultUnit = _mapHelper.GetUnitDtoFromUnit(foundMonitoringPointParameter.DefaultUnit);

                var foundLimits = _dbContext.MonitoringPointParameterLimits
                    .Include(mppl => mppl.LimitBasis)
                    .Include(mppl => mppl.LimitType)
                    .Where(mppl => mppl.MonitoringPointParameterId == foundMonitoringPointParameter.MonitoringPointParameterId
                            && !mppl.IsAlertOnly) //Future feature? Need this filter?
                    .ToList();

                paramDto.IsCalcMassLoading = foundLimits
                    .Any(mppl => mppl.LimitBasis.Name == LimitBasisName.MassLoading.ToString() && mppl.LimitType.Name == LimitTypeName.Daily.ToString());

                foreach (var limit in foundLimits)
                {
                    if (limit.LimitBasis.Name == LimitBasisName.Concentration.ToString() && limit.LimitType.Name == LimitTypeName.Daily.ToString())
                    {
                        paramDto.ConcentrationMinValue = limit.MinimumValue;
                        paramDto.ConcentrationMaxValue = limit.MaximumValue;
                    }
                    else if (limit.LimitBasis.Name == LimitBasisName.MassLoading.ToString() && limit.LimitType.Name == LimitTypeName.Daily.ToString())
                    {
                        paramDto.MassLoadingMinValue = limit.MinimumValue;
                        paramDto.MassLoadingMaxValue = limit.MaximumValue;
                    }
                }
            }

            _logger.Info($"Leaving ParameterService.UpdateParameterForMonitoringPoint. monitoringPointId={monitoringPointId}, parameterId={parameterId}");

        }
    }
}