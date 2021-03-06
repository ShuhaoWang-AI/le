﻿using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.Mapping
{
    public interface IMapHelper
    {
        UserDto GetUserDtoFromUserProfileViewModel(UserProfileViewModel viewModel, UserDto dto = null);
        UserProfileViewModel GetUserProfileViewModelFromUserDto(UserDto userDto, UserProfileViewModel viewModel = null);
        QuestionAnswerPairViewModel GetQuestionAnswerPairViewModelFromQuestionAnswerPairDto(QuestionAnswerPairDto dto, QuestionAnswerPairViewModel viewModel = null);
        QuestionViewModel GetQuestionViewModelFromQuestionDto(QuestionDto dto, QuestionViewModel viewModel = null);
        DropdownOptionViewModel ToDropdownOptionViewModel(ListItemDto fromDto);
	    SampleViewModel ToViewModel(SampleDto sampleDto);
	    SampleResultViewModel ToViewModel(SampleResultDto sampleResultDto);

    }
}