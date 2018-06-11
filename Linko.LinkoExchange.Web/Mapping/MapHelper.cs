using System;
using System.Linq;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.Mapping
{
    public class MapHelper : IMapHelper
    {
        #region interface implementations

        public UserDto GetUserDtoFromUserProfileViewModel(UserProfileViewModel viewModel, UserDto dto = null)
        {
            if (dto == null)
            {
                dto = new UserDto();
            }
            dto.UserProfileId = viewModel.UserProfileId;
            dto.TitleRole = viewModel.TitleRole;
            dto.FirstName = viewModel.FirstName;
            dto.LastName = viewModel.LastName;
            dto.UserName = viewModel.UserName;
            dto.BusinessName = viewModel.BusinessName;
            dto.AddressLine1 = viewModel.AddressLine1;
            dto.AddressLine2 = viewModel.AddressLine2;
            dto.CityName = viewModel.CityName;
            dto.ZipCode = viewModel.ZipCode;
            dto.JurisdictionId = viewModel.JurisdictionId;
            dto.PhoneExt = viewModel.PhoneExt;

            //IGNORE IsAccountLocked
            //IGNORE IsAccountResetRequired
            //IGNORE IsIdentityProofed 
            //IGNORE IsInternalAccount
            //IGNORE CreationDateTimeUtc
            //IGNORE LastModificationDateTimeUtc
            dto.Email = viewModel.Email;

            //IGNORE OldEmailAddress
            //IGNORE EmailConfirmed
            //IGNORE PasswordHash
            dto.PhoneNumber = viewModel.PhoneNumber;

            //IGNORE LockoutEndDateUtc
            //IGNORE LockoutEnabled
            //IGNORE Password
            //IGNORE AgreeTermsAndConditions

            return dto;
        }

        public UserProfileViewModel GetUserProfileViewModelFromUserDto(UserDto userDto, UserProfileViewModel viewModel = null)
        {
            if (viewModel == null)
            {
                viewModel = new UserProfileViewModel();
            }
            viewModel.UserProfileId = userDto.UserProfileId;

            //IGNORE Role
            //IGNORE HasSigntory
            //IGNORE HasSigntoryText
            viewModel.TitleRole = userDto.TitleRole;
            viewModel.FirstName = userDto.FirstName;
            viewModel.LastName = userDto.LastName;
            viewModel.BusinessName = userDto.BusinessName;

            //IGNORE OrganizationId
            viewModel.AddressLine1 = userDto.AddressLine1;
            viewModel.AddressLine2 = userDto.AddressLine2;
            viewModel.CityName = userDto.CityName;
            viewModel.JurisdictionId = userDto.JurisdictionId;
            viewModel.ZipCode = userDto.ZipCode;
            viewModel.PhoneNumber = userDto.PhoneNumber;
            viewModel.PhoneExt = userDto.PhoneExt;
            viewModel.Email = userDto.Email;
            viewModel.UserName = userDto.UserName;
            viewModel.Password = userDto.Password;

            //IGNORE StateList

            return viewModel;
        }

        public QuestionAnswerPairViewModel GetQuestionAnswerPairViewModelFromQuestionAnswerPairDto(QuestionAnswerPairDto dto, QuestionAnswerPairViewModel viewModel = null)
        {
            if (viewModel == null)
            {
                viewModel = new QuestionAnswerPairViewModel();
                viewModel.Question = GetQuestionViewModelFromQuestionDto(dto:dto.Question);
                viewModel.Answer = GetAnswerViewModelFromAnswerDto(dto:dto.Answer);
            }
            else
            {
                viewModel.Question = GetQuestionViewModelFromQuestionDto(dto:dto.Question, viewModel:viewModel.Question);
                viewModel.Answer = GetAnswerViewModelFromAnswerDto(dto:dto.Answer, viewModel:viewModel.Answer);
            }

            return viewModel;
        }

        public QuestionViewModel GetQuestionViewModelFromQuestionDto(QuestionDto dto, QuestionViewModel viewModel = null)
        {
            if (viewModel == null)
            {
                viewModel = new QuestionViewModel();
            }
            viewModel.QuestionId = dto.QuestionId;
            viewModel.QuestionType = dto.QuestionType;
            viewModel.Content = dto.Content;
            viewModel.IsActive = dto.IsActive;

            return viewModel;
        }

        #endregion

        private AnswerViewModel GetAnswerViewModelFromAnswerDto(AnswerDto dto, AnswerViewModel viewModel = null)
        {
            if (viewModel == null)
            {
                viewModel = new AnswerViewModel();
            }
            viewModel.UserQuestionAnswerId = dto.UserQuestionAnswerId;
            viewModel.Content = dto.Content;
            viewModel.QuestionId = dto.QuestionId;

            return viewModel;
        }

        public DropdownOptionViewModel ToDropdownOptionViewModel(ListItemDto fromDto)
        {
            if (fromDto == null)
            {
                return null;
            }
            return new DropdownOptionViewModel
            {
                Id = fromDto.Id,
                DisplayName = fromDto.DisplayValue,
                Description = fromDto.Description?? string.Empty
            };
        }

	    public SampleResultViewModel ToViewModel(SampleResultDto sampleResultDto)
	    {
		    if (sampleResultDto == null)
		    {
			    return null; 
		    }

		    var sampleResult = new SampleResultViewModel
		                       {
			                      ParameterId = sampleResultDto.ParameterId,
								  ParameterName = sampleResultDto.ParameterName, 
								  Qualifier = sampleResultDto.Qualifier,
								  Value = sampleResultDto.EnteredValue,
								  UnitId = sampleResultDto.UnitId,
								  UnitName = sampleResultDto.UnitName,
								  EnteredMethodDetectionLimit = sampleResultDto.EnteredMethodDetectionLimit,
								  AnalysisMethod = sampleResultDto.AnalysisMethod,
								  AnalysisDateTimeLocal = sampleResultDto.AnalysisDateTimeLocal, 
								  ConcentrationResultCompliance = sampleResultDto.ConcentrationResultCompliance.ToString(),
								  ConcentrationResultComplianceComment = sampleResultDto.ConcentrationResultComplianceComment,

								  MassLoadingQualifier = sampleResultDto.MassLoadingQualifier, 
								  MassLoadingUnitId = sampleResultDto.MassLoadingUnitId, 
								  MassLoadingValue = sampleResultDto.MassLoadingValue, 
								  MassLoadingUnitName = sampleResultDto.MassLoadingUnitName,
								  MassLoadingSampleResultId = sampleResultDto.MassLoadingSampleResultId,
								  MassResultCompliance = sampleResultDto.MassResultCompliance.ToString(),
								  MassResultComplianceComment = sampleResultDto.MassResultComplianceComment,
								  


		                       };
		    return sampleResult;
	    }

	    public SampleViewModel ToViewModel(SampleDto sampleDto)
	    {
		    if (sampleDto == null)
		    {
			    return null;
		    }

		    var sampleViewModel = new SampleViewModel
		                          {
			                          Id = sampleDto.SampleId,
			                          Name = sampleDto.Name,
			                          MonitoringPointId = sampleDto.MonitoringPointId,
			                          MonitoringPointName = sampleDto.MonitoringPointName,
			                          CtsEventTypeName = sampleDto.CtsEventTypeName,
			                          CtsEventTypeId = sampleDto.CtsEventTypeId,
			                          CollectionMethodId = sampleDto.CollectionMethodId,
			                          CollectionMethodName = sampleDto.CollectionMethodName,
			                          LabSampleIdentifier = sampleDto.LabSampleIdentifier,
			                          StartDateTimeLocal = sampleDto.StartDateTimeLocal,
			                          EndDateTimeLocal = sampleDto.EndDateTimeLocal,
			                          IsReadyToReport = sampleDto.IsReadyToReport,
			                          SampleStatusName = sampleDto.SampleStatusName,
			                          FlowValue = sampleDto.FlowEnteredValue,
			                          FlowUnitId = sampleDto.FlowUnitId,
			                          FlowUnitName = sampleDto.FlowUnitName,
			                          FlowUnitValidValues = sampleDto.FlowUnitValidValues,
									  ResultQualifierValidValues = sampleDto.ResultQualifierValidValues, 
			                          MassLoadingCalculationDecimalPlaces = sampleDto.MassLoadingCalculationDecimalPlaces ?? 0,
			                          MassLoadingConversionFactorPounds = sampleDto.MassLoadingConversionFactorPounds ?? 0.0f,
			                          IsMassLoadingResultToUseLessThanSign = sampleDto.IsMassLoadingResultToUseLessThanSign,
			                          SampleResults = sampleDto.SampleResults.Select(ToViewModel)
		                          };

		    return sampleViewModel;
	    }

    }
}
