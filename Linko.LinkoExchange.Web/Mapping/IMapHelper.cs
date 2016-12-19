﻿using Linko.LinkoExchange.Services.Dto;
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
        UserDto GetUserDtoFromUserProfileViewModel(UserProfileViewModel viewModel, UserDto dto = null);
        UserProfileViewModel GetUserProfileViewModelFromUserDto(UserDto userDto, UserProfileViewModel viewModel = null);
        QuestionAnswerPairViewModel GetQuestionAnswerPairViewModelFromQuestionAnswerPairDto(QuestionAnswerPairDto dto, QuestionAnswerPairViewModel viewModel = null);
        QuestionViewModel GetQuestionViewModelFromQuestionDto(QuestionDto dto, QuestionViewModel viewModel = null);
    }
}
