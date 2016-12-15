using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Web.Mapping
{
    public interface IMapHelper
    {
        UserDto GetUserDtoFromUserProfileViewModel(UserProfileViewModel viewModel);
        UserProfileViewModel GetUserProfileViewModelFromUserDto(UserDto userDto);
        QuestionAnswerPairViewModel GetQuestionAnswerPairViewModelFromQuestionAnswerPairDto(QuestionAnswerPairDto dto);
        QuestionViewModel GetQuestionViewModelFromQuestionDto(QuestionDto dto);
    }
}
