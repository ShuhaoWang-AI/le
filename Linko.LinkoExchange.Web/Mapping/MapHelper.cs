using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.Mapping
{
    public class MapHelper : IMapHelper
    {
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
                viewModel.Question = this.GetQuestionViewModelFromQuestionDto(dto.Question);
                viewModel.Answer = this.GetAnswerViewModelFromAnswerDto(dto.Answer);
            }
            else
            {
                viewModel.Question = this.GetQuestionViewModelFromQuestionDto(dto.Question, viewModel.Question);
                viewModel.Answer = this.GetAnswerViewModelFromAnswerDto(dto.Answer, viewModel.Answer);
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
    }
}