﻿using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Linko.LinkoExchange.Services.Cache;
using System.Collections.ObjectModel;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Unit;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Runtime.CompilerServices;

namespace Linko.LinkoExchange.Services.Sample
{
    public class SampleService : BaseService, ISampleService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContext;
        private readonly IOrganizationService _orgService;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISettingService _settings;
        private readonly IUnitService _unitService;

        public SampleService(LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IOrganizationService orgService,
            IMapHelper mapHelper,
            ILogger logger,
            ITimeZoneService timeZoneService,
            ISettingService settings,
            IUnitService unitService)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _orgService = orgService;
            _mapHelper = mapHelper;
            _logger = logger;
            _timeZoneService = timeZoneService;
            _settings = settings;
            _unitService = unitService;
        }

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            bool retVal = false;

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentPortalName = _httpContext.GetClaimValue(CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value: currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetSampleDetails":
                    {
                        var sampleId = id[0];
                        if (currentPortalName.Equals("authority"))
                        {
                            //currentOrgRegProgramId must match the authority of the ForOrganizationRegulatoryProgram of the sample
                            var authorityOrgRegProgramId = _orgService.GetAuthority(_dbContext.Samples
                                                        .Single(s => s.SampleId == sampleId).ForOrganizationRegulatoryProgramId)
                                                        .OrganizationRegulatoryProgramId;

                            retVal = (currentOrgRegProgramId == authorityOrgRegProgramId);
                        }
                        else
                        {
                            //currentOrgRegProgramId must match the ForOrganizationRegulatoryProgramId of the sample
                            //(this also handles unknown sampleId's)
                            var isSampleWithThisOwnerExist = _dbContext.Samples
                                            .Any(s => s.SampleId == sampleId
                                                && s.ForOrganizationRegulatoryProgramId == currentOrgRegProgramId);

                            retVal = isSampleWithThisOwnerExist;
                        }
                    }

                    break;

                default:

                    throw new Exception($"ERROR: Unhandled API authorization attempt using name = '{apiName}'");


            }

            return retVal;
        }

        /// <summary>
        /// Maps Sample Dto to Sample and saves to database. No validation.
        /// </summary>
        /// <param name="sampleDto">Sample Dto to map and save</param>
        /// <returns>Existing Id or newly created Id of Sample row in tSample table</returns>
        private int SimplePersist(SampleDto sampleDto)
        {
            string sampleIdString = string.Empty;
            if (sampleDto.SampleId.HasValue)
            {
                sampleIdString = sampleDto.SampleId.Value.ToString();
            }
            else
            {
                sampleIdString = "null";
            }

            _logger.Info($"Enter SampleService.SimplePersist. sampleDto.SampleId.Value={sampleIdString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            //var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            var sampleStartDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(sampleDto.StartDateTimeLocal, timeZoneId);
            var sampleEndDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(sampleDto.EndDateTimeLocal, timeZoneId);
            var massLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.MassLoading.ToString()).LimitBasisId;
            var concentrationLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.Concentration.ToString()).LimitBasisId;
            var dailyLimitTypeId = _dbContext.LimitTypes.Single(lt => lt.Name == LimitTypeName.Daily.ToString()).LimitTypeId;

            Core.Domain.Sample sampleToPersist = null;
            if (sampleDto.SampleId.HasValue && sampleDto.SampleId.Value > 0)
            {
                //Update existing
                sampleToPersist = _dbContext.Samples
                    .Include(c => c.SampleResults)
                    .Include(c => c.SampleResults.Select(sr => sr.LimitBasis))
                    .Single(c => c.SampleId == sampleDto.SampleId);
                sampleToPersist = _mapHelper.GetSampleFromSampleDto(sampleDto, sampleToPersist);

                //Handle Sample Result "Deletions"
                //(Remove items from the database that cannot be found in the dto collection)
                //
                var existingSampleResults = sampleToPersist.SampleResults
                    .Where(sr => sr.LimitBasis.Name == LimitBasisName.Concentration.ToString()
                        || sr.LimitBasis.Name == LimitBasisName.MassLoading.ToString())
                    .ToArray();

                for (var i = 0; i < existingSampleResults.Length; i++)
                {
                    var existingSampleResult = existingSampleResults[i];
                    //Find match in sample dto results
                    var matchedSampleResult = sampleDto.SampleResults
                        .SingleOrDefault(sr => (sr.ConcentrationSampleResultId.HasValue && sr.ConcentrationSampleResultId.Value == existingSampleResult.SampleResultId)
                                        || (sr.MassLoadingSampleResultId.HasValue && sr.MassLoadingSampleResultId.Value == existingSampleResult.SampleResultId));

                    if (matchedSampleResult == null)
                    {
                        //existing sample result must have been deleted -- remove
                        _dbContext.SampleResults.Remove(existingSampleResult);
                    }
                }
            }
            else
            {
                //Get new
                sampleToPersist = _mapHelper.GetSampleFromSampleDto(sampleDto);
                sampleToPersist.CreationDateTimeUtc = DateTimeOffset.Now;
                sampleToPersist.SampleResults = new List<SampleResult>();
                _dbContext.Samples.Add(sampleToPersist);
            }

            //Set Name auto-generated using settings format
            string sampleName;
            string nameCreationRule = _settings.GetOrgRegProgramSettingValue(currentOrgRegProgramId, SettingType.SampleNameCreationRule);
            if (nameCreationRule == SampleNameCreationRuleOption.SampleEventType.ToString())
            {
                sampleName = sampleToPersist.CtsEventTypeName;
            }
            else if (nameCreationRule == SampleNameCreationRuleOption.SampleEventTypeCollectionMethod.ToString())
            {
                sampleName = $"{sampleToPersist.CtsEventTypeName} {sampleToPersist.CollectionMethodName}";
            }
            else
            {
                throw new Exception($"ERROR: Unknown SampleNameCreationRuleOption={nameCreationRule}, currentOrgRegProgramId={currentOrgRegProgramId}");
            }

            sampleToPersist.Name = sampleName;
            sampleToPersist.ByOrganizationRegulatoryProgramId = currentOrgRegProgramId; //these are the same only per current workflow
            sampleToPersist.ForOrganizationRegulatoryProgramId = currentOrgRegProgramId; //these are the same only per current workflow
            sampleToPersist.StartDateTimeUtc = sampleStartDateTimeUtc;
            sampleToPersist.EndDateTimeUtc = sampleEndDateTimeUtc;
            sampleToPersist.LastModificationDateTimeUtc = DateTimeOffset.Now;
            sampleToPersist.LastModifierUserId = currentUserId;

            //Handle FlowUnitValidValues
            if (sampleDto.FlowUnitValidValues != null)
            {
                var flowUnitValidValues = "";
                var commaString = "";
                foreach (var unitDto in sampleDto.FlowUnitValidValues)
                {
                    flowUnitValidValues += commaString + unitDto.Name;
                    commaString = ",";
                }

                sampleToPersist.FlowUnitValidValues = flowUnitValidValues;
            }

            //Need to Save to get new SampleId and to load LimitBasis navigation property for next query (only LimitBasisId was set up to this point for new Results!)
            _dbContext.SaveChanges();

            //
            //Handle Flow Result
            //
            var existingFlowResultRow = sampleToPersist.SampleResults
                    .SingleOrDefault(sr => sr.LimitBasis.Name == LimitBasisName.VolumeFlowRate.ToString());

            if (!string.IsNullOrEmpty(sampleDto.FlowValue) && sampleDto.FlowUnitId != null && !string.IsNullOrEmpty(sampleDto.FlowUnitName))
            {
                //Check flow unit id is within valid values
                if (sampleDto.FlowUnitValidValues == null || !sampleDto.FlowUnitValidValues.Select(fu => fu.UnitId).Contains(sampleDto.FlowUnitId.Value))
                {
                    ThrowSimpleException($"ERROR: Selected flow unit (id={sampleDto.FlowUnitId.Value}, name={sampleDto.FlowUnitName}) could not be found within passed in FlowUnitValidValues collection.");
                }

                if (existingFlowResultRow == null)
                {
                    existingFlowResultRow = new SampleResult() { CreationDateTimeUtc = DateTimeOffset.UtcNow, SampleId = sampleToPersist.SampleId };
                    sampleToPersist.SampleResults.Add(existingFlowResultRow);
                }

                var flowParameter = _dbContext.Parameters
                    .First(p => p.IsFlowForMassLoadingCalculation == true); //Chris: "Should be one but just get first". // TODO: Need to check OrganizationRegulatoryProgramId

                var flowLimitBasisId = _dbContext.LimitBases.Single(lb => lb.Name == LimitBasisName.VolumeFlowRate.ToString()).LimitBasisId;

                existingFlowResultRow.SampleId = sampleToPersist.SampleId;
                existingFlowResultRow.ParameterId = flowParameter.ParameterId;
                existingFlowResultRow.ParameterName = flowParameter.Name;
                existingFlowResultRow.Qualifier = "";
                existingFlowResultRow.EnteredValue = sampleDto.FlowValue;
                //existingFlowResultRow.Value = Convert.ToDouble(sampleDto.FlowValue); //handle this below
                existingFlowResultRow.UnitId = sampleDto.FlowUnitId.Value;
                existingFlowResultRow.UnitName = sampleDto.FlowUnitName;
                existingFlowResultRow.EnteredMethodDetectionLimit = "";
                existingFlowResultRow.LimitTypeId = null;
                existingFlowResultRow.LimitBasisId = flowLimitBasisId;
                existingFlowResultRow.LastModificationDateTimeUtc = DateTimeOffset.Now;
                existingFlowResultRow.LastModifierUserId = currentUserId;


                Double valueAsDouble;
                if (!Double.TryParse(existingFlowResultRow.EnteredValue, out valueAsDouble))
                {
                    //Could not convert
                    return -1; // throw exception than -1 as that will wrongly treat as sample ID
                }
                existingFlowResultRow.Value = valueAsDouble;    
            }
            else
            {
                if (existingFlowResultRow != null)
                {
                    //Flow value must have been deleted -- must remove
                    _dbContext.SampleResults.Remove(existingFlowResultRow);
                }
            }
             
            //Handle Sample Results "Updates and/or Additions"
            foreach (var sampleResultDto in sampleDto.SampleResults)
            {
                //Each SampleResultDto has at least a Concentration component
                SampleResult concentrationResultRowToUpdate = null;
                if (!sampleResultDto.ConcentrationSampleResultId.HasValue)
                {
                    concentrationResultRowToUpdate = new SampleResult() { CreationDateTimeUtc = DateTimeOffset.UtcNow, SampleId = sampleToPersist.SampleId };
                    _dbContext.SampleResults.Add(concentrationResultRowToUpdate);
                }
                else
                {
                    concentrationResultRowToUpdate = sampleToPersist.SampleResults
                        .Single(sr => sr.SampleResultId == sampleResultDto.ConcentrationSampleResultId.Value);
                }

                //Update Concentration Result
                concentrationResultRowToUpdate = _mapHelper.GetConcentrationSampleResultFromSampleResultDto(sampleResultDto, concentrationResultRowToUpdate);

                if (!String.IsNullOrEmpty(concentrationResultRowToUpdate.EnteredValue))
                {
                    Double valueAsDouble, mdlAsDouble;
                    if (!Double.TryParse(concentrationResultRowToUpdate.EnteredValue, out valueAsDouble))
                    {
                        //Could not convert
                        return -1; // throw exception than -1 as that will wrongly treat as sample ID
                    }
                    concentrationResultRowToUpdate.Value = valueAsDouble; 

                    if(double.TryParse(concentrationResultRowToUpdate.EnteredMethodDetectionLimit, out mdlAsDouble))
                    {
                        concentrationResultRowToUpdate.MethodDetectionLimit = mdlAsDouble;
                    } 
                }
                if (sampleResultDto.AnalysisDateTimeLocal.HasValue)
                {
                    concentrationResultRowToUpdate.AnalysisDateTimeUtc = _timeZoneService
                        .GetDateTimeOffsetFromLocalUsingThisTimeZoneId(sampleResultDto.AnalysisDateTimeLocal.Value, timeZoneId);
                }
                concentrationResultRowToUpdate.LimitBasisId = concentrationLimitBasisId;
                concentrationResultRowToUpdate.LimitTypeId = dailyLimitTypeId;
                concentrationResultRowToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;
                concentrationResultRowToUpdate.LastModifierUserId = currentUserId;

                //this is always persisted with the concentration result NOT the mass loading result
                concentrationResultRowToUpdate.IsMassLoadingCalculationRequired = sampleResultDto.IsCalcMassLoading;

                //... the SampleResultDto MAY ALSO have a Mass Loading component
                if (!string.IsNullOrEmpty(sampleResultDto.MassLoadingValue))
                {
                    SampleResult massResultRowToUpdate = null;
                    if (!sampleResultDto.MassLoadingSampleResultId.HasValue)
                    {
                        massResultRowToUpdate = new SampleResult() { CreationDateTimeUtc = DateTimeOffset.UtcNow, SampleId = sampleToPersist.SampleId };
                        _dbContext.SampleResults.Add(massResultRowToUpdate);
                    }
                    else
                    {
                        massResultRowToUpdate = sampleToPersist.SampleResults
                            .Single(sr => sr.SampleResultId == sampleResultDto.MassLoadingSampleResultId.Value);
                    }

                    //Update Mass Loading Result
                    massResultRowToUpdate = _mapHelper.GetMassSampleResultFromSampleResultDto(sampleResultDto, massResultRowToUpdate);
                    massResultRowToUpdate.IsMassLoadingCalculationRequired = false; //always FALSE for mass loading result
                    if (!String.IsNullOrEmpty(massResultRowToUpdate.EnteredValue))
                    {
                        Double massValueAsDouble, mdlAsDouble;
                        if (!Double.TryParse(massResultRowToUpdate.EnteredValue, out massValueAsDouble))
                        {
                            //Could not convert
                            return -1; // throw exception than -1 as that will wrongly treat as sample ID
                        }
                        massResultRowToUpdate.Value = massValueAsDouble; 

                        if( double.TryParse(massResultRowToUpdate.EnteredMethodDetectionLimit, out mdlAsDouble))
                        {
                            massResultRowToUpdate.MethodDetectionLimit = mdlAsDouble; 
                        }
                    }
                    if (sampleResultDto.AnalysisDateTimeLocal.HasValue)
                    {
                        massResultRowToUpdate.AnalysisDateTimeUtc = _timeZoneService
                            .GetDateTimeOffsetFromLocalUsingThisTimeZoneId(sampleResultDto.AnalysisDateTimeLocal.Value, timeZoneId);
                    }
                    massResultRowToUpdate.LimitBasisId = massLimitBasisId;
                    massResultRowToUpdate.LimitTypeId = dailyLimitTypeId;
                    massResultRowToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;
                    massResultRowToUpdate.LastModifierUserId = currentUserId;
                }
            }


            _dbContext.SaveChanges();

            _logger.Info($"Leaving SampleService.SimplePersist. sampleIdToReturn={sampleToPersist.SampleId}");

            return sampleToPersist.SampleId;

        }

        /// <summary>
        /// Saves a Sample to the database after validating. Throw a list of RuleViolation exceptions
        /// for failed validation issues. If SampleDto.IsReadyToReport is true, validation is more strict.
        /// </summary>
        /// <param name="sample">Sample Dto</param>
        /// <returns>Existing Sample Id or newly created Sample Id</returns>
        public int SaveSample(SampleDto sampleDto)
        {
            string sampleIdString = string.Empty;
            if (sampleDto.SampleId.HasValue)
            {
                sampleIdString = sampleDto.SampleId.Value.ToString();
            }
            else
            {
                sampleIdString = "null";
            }
            _logger.Info($"Enter SampleService.SaveSample. sampleDto.SampleId.Value={sampleIdString}, isSavingAsReadyToReport={sampleDto.IsReadyToReport}");

            var sampleId = -1;
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    //Cannot save if included in a report
                    //      (UC-15-1.2(*.a.) - System identifies Sample is in use in a Report Package (draft or otherwise) an displays the "REPORTED" Status.  
                    //      Actor cannot perform any actions of any kind except view all details.)
                    if (sampleDto.SampleId.HasValue &&
                        this.IsSampleIncludedInReportPackage(sampleDto.SampleId.Value))
                    {
                        ThrowSimpleException("Sample is in use in a Report Package and is therefore READ-ONLY.");
                    }

                    if (this.IsValidSample(sampleDto, isSuppressExceptions: false))
                    {
                        sampleId = this.SimplePersist(sampleDto);
                        transaction.Commit();
                    }
                }
                catch (RuleViolationException ex)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();
                    var errors = new List<string>() { ex.Message };

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            errors.Add(message);
                        }
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

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

            _logger.Info($"Leaving SampleService.SaveSample. sampleId={sampleId}, isSavingAsReadyToReport={sampleDto.IsReadyToReport}");
            return sampleId;
        }

        /// <summary>
        /// Used to simplify and clean up methods where there are multiple validation tests.
        /// </summary>
        /// <param name="message">Rule violation message to use when throwing the exception.</param>
        private void ThrowSimpleException(string message)
        {
            _logger.Info($"Enter SampleService.ThrowSimpleException. message={message}");

            List<RuleViolation> validationIssues = new List<RuleViolation>();
            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));

            _logger.Info($"Leaving SampleService.ThrowSimpleException. message={message}");

            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
        }

        /// <summary>
        /// Tests validation of a passed in Sample in either Draft Mode (sampleDto.IsReadyToReport = false)
        /// or ReadyToReport Mode (sampleDto.IsReadyToReport = true)
        /// </summary>
        /// <param name="sampleDto">Sample to validate</param>
        /// <param name="isSuppressExceptions">False = throws RuleViolation exception, True = does not throw RuleViolation exceptions</param>
        /// <returns>Boolean indicating if Sample passed all validation (Draft or ReadyToReport mode)</returns>
        public bool IsValidSample(SampleDto sampleDto, bool isSuppressExceptions = false)
        {
            string sampleIdString = string.Empty;
            if (sampleDto.SampleId.HasValue)
            {
                sampleIdString = sampleDto.SampleId.Value.ToString();
            }
            else
            {
                sampleIdString = "null";
            }
            _logger.Info($"Enter SampleService.IsValidSample. sampleDto.SampleId.Value={sampleIdString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authOrgRegProgramId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationRegulatoryProgramId;
            var currentUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(authOrgRegProgramId, SettingType.TimeZone));
            bool isValid = true;

            //Check required field (UC-15-1.2.1): "Sample Type"
            if (sampleDto.CtsEventTypeId < 1)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("Sample Type is required.");
                }
            }
            //Check required field (UC-15-1.2.1): "Collection Method"
            if (sampleDto.CollectionMethodId < 1)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("Collection Method is required.");
                }
            }
            //Check required field (UC-15-1.2.1): "Start Date/Time"
            if (sampleDto.StartDateTimeLocal == null || sampleDto.StartDateTimeLocal == DateTime.MinValue)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("Start Date/Time is required.");
                }
            }
            //Check required field (UC-15-1.2.1): "End Date/Time"
            if (sampleDto.EndDateTimeLocal == null || sampleDto.EndDateTimeLocal == DateTime.MinValue)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("End Date/Time is required.");
                }
            }

            //Check sample start/end dates are not in the future (UC-15-1.2.9.1.b)

            if (_timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(sampleDto.StartDateTimeLocal, timeZoneId) > DateTimeOffset.Now ||
                _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(sampleDto.EndDateTimeLocal, timeZoneId) > DateTimeOffset.Now)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("Sample dates cannot be future dates.");
                }
            }

            //Check end date is not before start date
            if (sampleDto.EndDateTimeLocal < sampleDto.StartDateTimeLocal)
            {
                isValid = false;
                if (!isSuppressExceptions)
                {
                    this.ThrowSimpleException("End date must be after start date");
                }
            }


            //Check Flow Value exists and is complete if provided (both value and unit)

            bool isValidFlowValueExists = false;
            if (!string.IsNullOrEmpty(sampleDto.FlowValue) && sampleDto.FlowUnitId.HasValue &&
                sampleDto.FlowUnitId.Value > 0 && !string.IsNullOrEmpty(sampleDto.FlowUnitName))
            {
                isValidFlowValueExists = true;

                Double flowValueAsDouble;
                if (!Double.TryParse(sampleDto.FlowValue, out flowValueAsDouble))
                {
                    //Could not convert -- throw exception
                    ThrowSimpleException($"Could not convert provided flow value '{sampleDto.FlowValue}' to double.");
                }
            }

            //
            //Validation for Sample Results
            //

            if (sampleDto.SampleResults != null)
            {
                foreach (var resultDto in sampleDto.SampleResults)
                {
                    //Check if concentration entered value is numeric and can be converted to double.
                    if (!String.IsNullOrEmpty(resultDto.EnteredValue))
                    {
                        Double concentrationValueAsDouble;
                        if (!Double.TryParse(resultDto.EnteredValue, out concentrationValueAsDouble))
                        {
                            //Could not convert -- throw exception
                            ThrowSimpleException($"Could not convert provided concentration value '{resultDto.EnteredValue}' to double.");
                        }
                    }

                    //Check if mass loading entered value is numeric and can be converted to double.
                    if (!String.IsNullOrEmpty(resultDto.MassLoadingValue))
                    {
                        Double massLoadingValueAsDouble;
                        if (!Double.TryParse(resultDto.MassLoadingValue, out massLoadingValueAsDouble))
                        {
                            //Could not convert -- throw exception
                            ThrowSimpleException($"Could not convert provided mass loading value '{resultDto.MassLoadingValue}' to double.");
                        }
                    }

                    //All results must have a unit if provided (applied to both Draft or ReadyToReport)
                    if (resultDto.UnitId < 1)
                    {
                        isValid = false;
                        if (!isSuppressExceptions)
                        {
                            this.ThrowSimpleException("All results must be associated with a valid unit.");
                        }
                    }

                    //if (resultDto.IsCalcMassLoading && resultDto.MassLoadingUnitId < 1)
                    //{
                    //    isValid = false;
                    //    if (!isSuppressExceptions)
                    //    {
                    //        this.ThrowSimpleException("All mass loading calculations must be associated with a valid unit.");
                    //    }
                    //}

                    if (sampleDto.IsReadyToReport)
                    {
                        //
                        //ReadyToReport Validation For Concentration Results
                        //

                        if ((resultDto.Qualifier == ">" || resultDto.Qualifier == "<" || string.IsNullOrEmpty(resultDto.Qualifier))
                        && string.IsNullOrEmpty(resultDto.EnteredValue))
                        {
                            isValid = false;
                            if (!isSuppressExceptions)
                            {
                                this.ThrowSimpleException("Result is required.");
                            }
                        }

                        if ((resultDto.Qualifier == "ND" || resultDto.Qualifier == "NF")
                            && !string.IsNullOrEmpty(resultDto.EnteredValue))
                        {
                            isValid = false;
                            if (!isSuppressExceptions)
                            {
                                this.ThrowSimpleException("ND or NF qualifiers cannot be followed by a value.");
                            }
                        }

                    }
                }
            }


            _logger.Info($"Leaving SampleService.IsValidSample. sampleDto.SampleId.Value={sampleIdString}, isValid={isValid}");

            return isValid;
        }

        /// <summary>
        /// Deletes a sample from the database
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        public void DeleteSample(int sampleId)
        {
            _logger.Info($"Enter SampleService.DeleteSample. sampleId={sampleId}");
            List<RuleViolation> validationIssues = new List<RuleViolation>();
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {

                    if (this.IsSampleIncludedInReportPackage(sampleId))
                    {
                        string message = "Attempting to delete a sample that is included in a report.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }
                    else
                    {
                        var sampleToRemove = _dbContext.Samples
                            .Include(s => s.SampleResults)
                            .Single(s => s.SampleId == sampleId);

                        //First remove results (if applicable)
                        if (sampleToRemove.SampleResults != null)
                        {
                            _dbContext.SampleResults.RemoveRange(sampleToRemove.SampleResults);
                        }
                        _dbContext.Samples.Remove(sampleToRemove);
                        _dbContext.SaveChanges();
                        transaction.Commit();
                    }

                    _logger.Info($"Leaving SampleService.DeleteSample. sampleId={sampleId}");

                }
                catch (RuleViolationException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();
                    var errors = new List<string>() { ex.Message };

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            errors.Add(message);
                        }
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));
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
        /// Converts a Sample POCO into the complete details of a single Sample (Dto)
        /// </summary>
        /// <param name="sample">POCO</param>
        /// <param name="isIncludeChildObjects">Switch to load result list or not (for display in grid)</param>
        /// <returns></returns>
        public SampleDto GetSampleDetails(Core.Domain.Sample sample, bool isIncludeChildObjects = true)
        {
            _logger.Info($"Enter SampleService.GetSampleDetails. sample.SampleId={sample.SampleId}");
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            int flowLimitBasisId = _dbContext.LimitBases
                .Single(lb => lb.Name == LimitBasisName.VolumeFlowRate.ToString()).LimitBasisId;

            var dto = _mapHelper.GetSampleDtoFromSample(sample);

            //Handle FlowUnitValidValues
            dto.FlowUnitValidValues = _unitService.GetFlowUnitsFromCommaDelimitedString(sample.FlowUnitValidValues);

            //Set Sample Start Local Timestamp
            dto.StartDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(sample.StartDateTimeUtc.UtcDateTime, timeZoneId);

            //Set Sample End Local Timestamp
            dto.EndDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(sample.EndDateTimeUtc.UtcDateTime, timeZoneId);

            //Set LastModificationDateTimeLocal
            dto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId((sample.LastModificationDateTimeUtc.HasValue ? sample.LastModificationDateTimeUtc.Value.UtcDateTime
                        : sample.CreationDateTimeUtc.UtcDateTime), timeZoneId);

            if (sample.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == sample.LastModifierUserId.Value);
                dto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                dto.LastModifierFullName = "N/A";
            }

            if (sample != null && sample.IsReadyToReport)
            {
                dto.SampleStatusName = SampleStatusName.ReadyToReport;
            }
            else
            {
                dto.SampleStatusName = SampleStatusName.Draft;
            }

            if (sample?.ReportSamples != null && sample.ReportSamples.Count() > 0)
            {
                dto.SampleStatusName = SampleStatusName.Reported;
            }

            var resultDtoList = new Dictionary<int, SampleResultDto>();

            if (isIncludeChildObjects)
            {
                foreach (var sampleResult in sample.SampleResults)
                {
                    //Handle "special case" Sample Results. These do not get mapped to their own
                    //SampleResult dtos.
                    //1. Flow - gets mapped to properties of the parent Sample Dto
                    //2. Mass - gets mapped to corresponding Concentration result Dto
                    //
                    //Remember that the items in this collection are unordered.

                    var resultDto = new SampleResultDto();

                    if (sampleResult.LimitBasisId == flowLimitBasisId)
                    {
                        dto.FlowValue = sampleResult.EnteredValue;
                        dto.FlowUnitId = sampleResult.UnitId;
                        dto.FlowUnitName = sampleResult.UnitName;
                    }
                    else if (sampleResult.LimitType.Name == LimitTypeName.Daily.ToString() &&
                        (sampleResult.LimitBasis.Name == LimitBasisName.MassLoading.ToString()
                        || sampleResult.LimitBasis.Name == LimitBasisName.Concentration.ToString()))
                    {

                        if (resultDtoList.ContainsKey(sampleResult.ParameterId))
                        {
                            //There was already a result dto added for this parameter
                            //and we are now handling the corresponding concentration (or mass) result
                            //and must attach these fields to that dto
                            resultDto = resultDtoList[sampleResult.ParameterId];
                        }
                        else
                        {
                            //There may be a corresponding concentration (or mass) result
                            //later in the collection that needs to be attached to this result dto
                            //so we need to save this for looking up later. 
                            resultDtoList.Add(sampleResult.ParameterId, resultDto);
                        }


                        if (sampleResult.LimitBasis.Name == LimitBasisName.Concentration.ToString())
                        {
                            resultDto.ConcentrationSampleResultId = sampleResult.SampleResultId;
                            resultDto.ParameterId = sampleResult.ParameterId;
                            resultDto.ParameterName = sampleResult.ParameterName;
                            resultDto.EnteredMethodDetectionLimit = sampleResult.EnteredMethodDetectionLimit;
                            resultDto.AnalysisMethod = sampleResult.AnalysisMethod;
                            resultDto.IsApprovedEPAMethod = sampleResult.IsApprovedEPAMethod;
                            resultDto.ParameterId = sampleResult.ParameterId;
                            resultDto.IsCalcMassLoading = sampleResult.IsMassLoadingCalculationRequired;
                            resultDto.Qualifier = sampleResult.Qualifier;
                            resultDto.EnteredValue = sampleResult.EnteredValue;
                            resultDto.UnitId = sampleResult.UnitId;
                            resultDto.UnitName = sampleResult.UnitName;

                            SetSampleResultDatesAndLastModified(sampleResult, ref resultDto, timeZoneId);

                        }
                        else
                        {
                            //Mass Result -- must be included with a resultDto where LimitBasisName = LimitBasisName.Concentration
                            resultDto.MassLoadingSampleResultId = sampleResult.SampleResultId;
                            resultDto.MassLoadingQualifier = sampleResult.Qualifier;
                            resultDto.MassLoadingValue = sampleResult.EnteredValue;
                            resultDto.MassLoadingUnitId = sampleResult.UnitId;
                            resultDto.MassLoadingUnitName = sampleResult.UnitName;
                        }
                    }
                    else
                    {
                        //  "Any introduction of a new Limit Type or new Limit Basis at the data level 
                        //  will be ignored until we change the code..." - mj

                    }
                }
            }

            dto.SampleResults = resultDtoList.Values.ToList();
            _logger.Info($"Leaving SampleService.GetSampleDetails. sample.SampleId={sample.SampleId}");
            return dto;
        }

        /// <summary>
        /// Helper function to localize the date/times found in a Sample Result. Also sets Last Modifier.
        /// </summary>
        /// <param name="sampleResult">Sample Result containing UTC date/times</param>
        /// <param name="resultDto">Output Sample Result dto that needs localized date/times</param>
        /// <param name="timeZoneId">The time zone id used to localize the date/times</param>
        private void SetSampleResultDatesAndLastModified(SampleResult sampleResult, ref SampleResultDto resultDto, int timeZoneId)
        {
            _logger.Info($"Enter SampleService.SetSampleResultDatesAndLastModified. sampleResult.SampleId={sampleResult.SampleId}");

            //Need to set localized time stamps for SampleResults
            if (sampleResult.AnalysisDateTimeUtc.HasValue)
            {
                resultDto.AnalysisDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(sampleResult.AnalysisDateTimeUtc.Value.UtcDateTime, timeZoneId);
            }

            //Set LastModificationDateTimeLocal
            resultDto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId((sampleResult.LastModificationDateTimeUtc.HasValue ? sampleResult.LastModificationDateTimeUtc.Value.UtcDateTime
                        : sampleResult.CreationDateTimeUtc.UtcDateTime), timeZoneId);

            if (sampleResult.LastModifierUserId.HasValue)
            {
                var lastModifierUser = _dbContext.Users.Single(user => user.UserProfileId == sampleResult.LastModifierUserId.Value);
                resultDto.LastModifierFullName = $"{lastModifierUser.FirstName} {lastModifierUser.LastName}";
            }
            else
            {
                resultDto.LastModifierFullName = "N/A";
            }

            _logger.Info($"Leaving SampleService.SetSampleResultDatesAndLastModified. sampleResult.SampleId={sampleResult.SampleId}");
        }

        /// <summary>
        /// Gets the complete details of a single Sample
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        /// <returns>Sample Dto associated with the passed in Id</returns>
        public SampleDto GetSampleDetails(int sampleId)
        {
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            _logger.Info($"Enter SampleService.GetSampleDetails. sampleId={sampleId}, currentOrgRegProgramId={currentOrgRegProgramId}");

            if (!CanUserExecuteApi(id: sampleId))
            {
                throw new UnauthorizedAccessException();
            }

            var sample = _dbContext.Samples
                .Include(s => s.ReportSamples)
                .Include(s => s.SampleResults)
                .Include(s => s.ByOrganizationRegulatoryProgram)
                .SingleOrDefault(s => s.SampleId == sampleId);

            if (sample == null)
            {
                throw new Exception($"ERROR: Could not find Sample associated with sampleId={sampleId}");
            }

            var dto = this.GetSampleDetails(sample);

            _logger.Info($"Leaving SampleService.GetSampleDetails. sampleId={sampleId}");

            return dto;
        }

        /// <summary>
        /// Test to see if a Sample is included in at least 1 report package
        /// </summary>
        /// <param name="sampleId">SampleId associated with the object in the tSample table</param>
        /// <returns>Boolean indicating if the Sample is or isn't included in at least 1 report package</returns>
        public bool IsSampleIncludedInReportPackage(int sampleId)
        {
            _logger.Info($"Enter SampleService.IsSampleIncludedInReportPackage. sampleId={sampleId}");

            var isExists = _dbContext.ReportSamples.Any(rs => rs.SampleId == sampleId);

            _logger.Info($"Leaving SampleService.IsSampleIncludedInReportPackage. sampleId={sampleId}");

            return isExists;
        }

        /// <summary>
        /// The the sample statistic count for different sample status  
        /// </summary>
        /// <param name="startDate">Nullable localized date/time time period range. 
        /// Sample start dates must on or after this date/time. Null parameters are ignored and not part of the filter.</param>
        /// <param name="endDate">Nullable localized date/time time period range. 
        /// Sample end dates must on or before this date/time. Null parameters are ignored and not part of the filter.</param>
        /// <returns>A list of different sample status</returns>
        public List<SampleCount> GetSampleCounts(DateTime? startDate = null, DateTime? endDate = null)
        {
            var startDateString = startDate?.ToString() ?? "null";
            var endDateString = endDate?.ToString() ?? "null";

            _logger.Info($"Enter SampleService.GetSamples. startDate={startDateString}, endDate={endDateString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

            var foundSamples = _dbContext.Samples
                .Include(s => s.ReportSamples)
                .Include(s => s.SampleResults)
                .Include(s => s.ByOrganizationRegulatoryProgram)
                .Where(s => s.ForOrganizationRegulatoryProgramId == currentOrgRegProgramId);

            if (startDate.HasValue)
            {
                var startDateUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(startDate.Value, timeZoneId);
                foundSamples = foundSamples.Where(s => s.StartDateTimeUtc >= startDateUtc);
            }
            if (endDate.HasValue)
            {
                var endDateUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(endDate.Value, timeZoneId);
                foundSamples = foundSamples.Where(s => s.EndDateTimeUtc <= endDateUtc);
            }

            var sampleCounts = new List<SampleCount>();
            var draftSamples = new SampleCount
            {
                Status = SampleStatusName.Draft,
                Count = foundSamples.Count(i => !i.IsReadyToReport)
            };
            sampleCounts.Add(draftSamples);

            var readyToSubmitSamples = new SampleCount
            {
                Status = SampleStatusName.ReadyToReport,
                Count = foundSamples.Count(i => i.IsReadyToReport && !i.ReportSamples.Any())
            };

            sampleCounts.Add(readyToSubmitSamples);

            _logger.Info($"Leaving SampleService.GetSamples. startDate={startDateString}, endDate={endDateString}");

            return sampleCounts;
        }

        /// <summary>
        /// Gets Samples from the database for displaying in a grid
        /// </summary>
        /// <param name="status">SampletStatus type to filter by</param>
        /// <param name="startDate">Nullable localized date/time time period range. 
        /// Sample start dates must on or after this date/time. Null parameters are ignored and not part of the filter.</param>
        /// <param name="endDate">Nullable localized date/time time period range. 
        /// Sample end dates must on or before this date/time. Null parameters are ignored and not part of the filter.</param>
        /// <returns>Collection of filtered Sample Dto's</returns>
        /// <param name="isIncludeChildObjects">Switch to load result list or not (for display in grid)</param>
        public IEnumerable<SampleDto> GetSamples(SampleStatusName status, DateTime? startDate = null, DateTime? endDate = null, bool isIncludeChildObjects = false)
        {
            string startDateString = startDate.HasValue ? startDate.Value.ToString() : "null";
            string endDateString = endDate.HasValue ? endDate.Value.ToString() : "null";
            _logger.Info($"Enter SampleService.GetSamples. status={status}, startDate={startDateString}, endDate={endDateString}");

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
            var dtos = new List<SampleDto>();
            var foundSamples = _dbContext.Samples
                .Include(s => s.ReportSamples)
                .Include(s => s.SampleResults)
                .Include(s => s.ByOrganizationRegulatoryProgram)
                .Where(s => s.ForOrganizationRegulatoryProgramId == currentOrgRegProgramId);

            if (startDate.HasValue)
            {
                var startDateUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(startDate.Value, timeZoneId);
                foundSamples = foundSamples.Where(s => s.StartDateTimeUtc >= startDateUtc);
            }
            if (endDate.HasValue)
            {
                var endDateUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(endDate.Value, timeZoneId);
                foundSamples = foundSamples.Where(s => s.EndDateTimeUtc <= endDateUtc);
            }


            switch (status)
            {
                case SampleStatusName.All:
                    //don't filter any further
                    break;
                case SampleStatusName.Draft:
                    foundSamples = foundSamples.Where(s => !s.IsReadyToReport);
                    break;
                case SampleStatusName.ReadyToReport:
                    foundSamples = foundSamples.Where(s => s.IsReadyToReport && s.ReportSamples.Count() < 1);
                    break;
                case SampleStatusName.Reported:
                    foundSamples = foundSamples.Where(s => s.ReportSamples.Count() > 0);
                    break;
                case SampleStatusName.DraftOrReadyToReport:
                    foundSamples = foundSamples.Where(s => s.ReportSamples.Count() < 1);
                    break;
                default:
                    throw new Exception($"Unhandled SampleStatusName = {status}");
            }

            foreach (var sample in foundSamples.ToList())
            {
                var dto = this.GetSampleDetails(sample, isIncludeChildObjects);
                dtos.Add(dto);
            }

            _logger.Info($"Leaving SampleService.GetSamples. status={status}, startDate={startDateString}, endDate={endDateString}, dtos.Count={dtos.Count()}");

            return dtos;
        }

        public IEnumerable<CollectionMethodDto> GetCollectionMethods()
        {
            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            _logger.Info($"Enter SampleService.GetCollectionMethods. currentOrgRegProgramId={currentOrgRegProgramId}");

            var collectionMethodList = new List<CollectionMethodDto>();
            var authOrganizationId = _orgService.GetAuthority(currentOrgRegProgramId).OrganizationId;
            var timeZoneId = Convert.ToInt32(_settings.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

            var collectionMethods = _dbContext.CollectionMethods
                .Where(cm => cm.IsEnabled
                && !cm.IsRemoved
                && cm.OrganizationId == authOrganizationId);

            foreach (var cm in collectionMethods)
            {
                collectionMethodList.Add(new Dto.CollectionMethodDto() { CollectionMethodId = cm.CollectionMethodId, Name = cm.Name });
            }

            _logger.Info($"Leave SampleService.GetCollectionMethods. currentOrgRegProgramId={currentOrgRegProgramId}");

            return collectionMethodList;
        }
    }
}
