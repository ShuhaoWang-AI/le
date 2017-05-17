using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Mapping;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Linko.LinkoExchange.Web.ViewModels.User;

namespace Linko.LinkoExchange.Web.shared
{
    public class ProfileHelper
    {
        private readonly string _fakePassword = "********";
        private readonly IHttpContextService _httpContextService;
        private readonly IJurisdictionService _jurisdictionService;
        private readonly IMapHelper _mapHelper;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IUserService _userService;

        public ProfileHelper(
            IQuestionAnswerService questAnswerService,
            IUserService userService,
            IJurisdictionService jurisdictionService,
            IMapHelper mapHelper,
            IHttpContextService httpContextService
        )
        {
            _userService = userService;
            _jurisdictionService = jurisdictionService;
            _questionAnswerService = questAnswerService;
            _mapHelper = mapHelper;
            _httpContextService = httpContextService;
        }

        public UserViewModel GetUserViewModel(int userProfileId)
        {
            var userProfileViewModel = GetUserProfileViewModel(userProfileId:userProfileId);
            var userSqViewModel = GetUserSecurityQuestionViewModel(userProfileId:userProfileId);
            var userKbqViewModel = GetUserKbqViewModel(userProfileId:userProfileId);

            return new UserViewModel
                   {
                       UserKBQ = userKbqViewModel,
                       UserProfile = userProfileViewModel,
                       UserSQ = userSqViewModel
                   };
        }

        public UserProfileViewModel GetUserProfileViewModel(int userProfileId)
        {
            var userProileDto = _userService.GetUserProfileById(userProfileId:userProfileId);
            var userProfileViewModel = _mapHelper.GetUserProfileViewModelFromUserDto(userDto:userProileDto);

            //Need to set the HasSignatory for Org Reg Program User
            var orgRegProgramUserIdString = _httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId);
            if (!string.IsNullOrEmpty(value:orgRegProgramUserIdString))
            {
                var orgRegProgramUserId = int.Parse(s:orgRegProgramUserIdString);
                var orgRegProgamUser = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:orgRegProgramUserId);
                userProfileViewModel.HasSigntory = orgRegProgamUser.IsSignatory;
            }

            // set password to be stars 
            userProfileViewModel.Password = _fakePassword;

            // Get state list   
            userProfileViewModel.StateList = GetStateList();
            userProfileViewModel.Role = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole);

            return userProfileViewModel;
        }

        public UserKBQViewModel GetUserKbqViewModel(int userProfileId)
        {
            var userKbqViewModel = new UserKBQViewModel {UserProfileId = userProfileId};

            var kbqQuestions = _questionAnswerService.GetUsersQuestionAnswers(userProfileId:userProfileId, questionType:QuestionTypeName.KBQ);

            var kbqs = kbqQuestions.Select(i => _mapHelper.GetQuestionAnswerPairViewModelFromQuestionAnswerPairDto(dto:i)).ToList();

            userKbqViewModel.QuestionPool = GetQuestionPool(type:QuestionTypeName.KBQ);

            ////  KBQ questions 
            userKbqViewModel.KBQ1 = kbqs[0].Question.QuestionId ?? -1;
            userKbqViewModel.KBQ2 = kbqs[1].Question.QuestionId ?? -1;
            userKbqViewModel.KBQ3 = kbqs[2].Question.QuestionId ?? -1;
            userKbqViewModel.KBQ4 = kbqs[3].Question.QuestionId ?? -1;
            userKbqViewModel.KBQ5 = kbqs[4].Question.QuestionId ?? -1;

            userKbqViewModel.KBQAnswer1 = kbqs[0].Answer.Content ?? "";
            userKbqViewModel.KBQAnswer2 = kbqs[1].Answer.Content ?? "";
            userKbqViewModel.KBQAnswer3 = kbqs[2].Answer.Content ?? "";
            userKbqViewModel.KBQAnswer4 = kbqs[3].Answer.Content ?? "";
            userKbqViewModel.KBQAnswer5 = kbqs[4].Answer.Content ?? "";

            //// keep track UserQuestionAnswerId
            userKbqViewModel.UserQuestionAnserId_KBQ1 = kbqs[0].Answer.UserQuestionAnswerId ?? -1;
            userKbqViewModel.UserQuestionAnserId_KBQ2 = kbqs[1].Answer.UserQuestionAnswerId ?? -1;
            userKbqViewModel.UserQuestionAnserId_KBQ3 = kbqs[2].Answer.UserQuestionAnswerId ?? -1;
            userKbqViewModel.UserQuestionAnserId_KBQ4 = kbqs[3].Answer.UserQuestionAnswerId ?? -1;
            userKbqViewModel.UserQuestionAnserId_KBQ5 = kbqs[4].Answer.UserQuestionAnswerId ?? -1;

            return userKbqViewModel;
        }

        public UserSQViewModel GetUserSecurityQuestionViewModel(int userProfileId)
        {
            var userSqViewModel = new UserSQViewModel();
            userSqViewModel.UserProfileId = userProfileId;

            var securityQeustions = _questionAnswerService.GetUsersQuestionAnswers(userProfileId:userProfileId, questionType:QuestionTypeName.SQ);
            var sqs = securityQeustions.Select(i => _mapHelper.GetQuestionAnswerPairViewModelFromQuestionAnswerPairDto(dto:i)).ToList();

            userSqViewModel.QuestionPool = GetQuestionPool(type:QuestionTypeName.SQ);

            ////  Security questions 
            userSqViewModel.SecurityQuestion1 = sqs[0].Question.QuestionId ?? -1;
            userSqViewModel.SecurityQuestion2 = sqs[1].Question.QuestionId ?? -1;

            userSqViewModel.SecurityQuestionAnswer2 = sqs[1].Answer.Content ?? "";
            userSqViewModel.SecurityQuestionAnswer1 = sqs[0].Answer.Content ?? "";

            //// Keep track UserQuestionAnswerId 
            userSqViewModel.UserQuestionAnserId_SQ1 = sqs[0].Answer.UserQuestionAnswerId ?? -1;
            userSqViewModel.UserQuestionAnserId_SQ2 = sqs[1].Answer.UserQuestionAnswerId ?? -1;

            return userSqViewModel;
        }

        public List<QuestionViewModel> GetQuestionPool(QuestionTypeName type)
        {
            return _questionAnswerService.GetQuestions().Select(i => _mapHelper.GetQuestionViewModelFromQuestionDto(dto:i)).ToList()
                                         .Where(i => i.QuestionType == type).ToList();
        }

        public List<JurisdictionViewModel> GetStateList()
        {
            var list = _jurisdictionService.GetStateProvs(countryId:(int) Country.USA);

            return list.Select(jur => new JurisdictionViewModel
                                      {
                                          JurisdictionId = jur.JurisdictionId,
                                          StateName = jur.Name
                                      }).ToList();
        }
    }
}