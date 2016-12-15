using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Mapping;
using Linko.LinkoExchange.Web.ViewModels.Shared;
using Linko.LinkoExchange.Web.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Linko.LinkoExchange.Web.shared
{
    public class ProfileHelper
    {
        private readonly ISessionCache _sessionCache;
        private readonly IUserService _userService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IJurisdictionService _jurisdictionService;
        private readonly string fakePassword = "********";
        private readonly IMapHelper _mapHelper;

        public ProfileHelper(
            IQuestionAnswerService questAnswerService,
            ISessionCache sessionCache,
            IUserService userService,
            IJurisdictionService jurisdictionService,
            IMapHelper mapHelper

            ) { 
            _sessionCache = sessionCache;
            _userService = userService;
            _jurisdictionService = jurisdictionService;
            _questionAnswerService = questAnswerService;
            _mapHelper = mapHelper;
        }


        public UserViewModel GetUserViewModel(int userProfileId)
        {
            var userProfileViewModel = GetUserProfileViewModel(userProfileId);
            var userSQViewModel = GetUserSecurityQuestionViewModel(userProfileId);
            var userKbqViewModel = GetUserKbqViewModel(userProfileId);

            return new UserViewModel
            {
                UserKBQ = userKbqViewModel,
                UserProfile = userProfileViewModel,
                UserSQ = userSQViewModel
            };
        }

        public UserProfileViewModel GetUserProfileViewModel(int userProfileId)
        {
            var userProileDto = _userService.GetUserProfileById(userProfileId);
            var userProfileViewModel = _mapHelper.GetUserProfileViewModelFromUserDto(userProileDto);

            //Need to set the HasSignatory for Org Reg Program User
            string orgRegProgramUserIdString = _sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId);
            if (!String.IsNullOrEmpty(orgRegProgramUserIdString))
            {
                int orgRegProgramUserId = int.Parse(orgRegProgramUserIdString);
                var orgRegProgamUser = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgramUserId);
                userProfileViewModel.HasSigntory = orgRegProgamUser.IsSignatory;
            }

            // set password to be stars 
            userProfileViewModel.Password = fakePassword;

            // Get state list   
            userProfileViewModel.StateList = GetStateList();
            userProfileViewModel.Role = _sessionCache.GetClaimValue(CacheKey.UserRole);

            return userProfileViewModel;
        }

        public UserKBQViewModel GetUserKbqViewModel(int userProfileId)
        {

            var userKbqViewModel = new UserKBQViewModel();
            userKbqViewModel.UserProfileId = userProfileId;

            var kbqQuestions = _questionAnswerService.GetUsersQuestionAnswers(userProfileId, QuestionTypeName.KBQ);

            var kbqs = kbqQuestions.Select(i => _mapHelper.GetQuestionAnswerPairViewModelFromQuestionAnswerPairDto(i)).ToList();

            userKbqViewModel.QuestionPool = GetQuestionPool(QuestionTypeName.KBQ);

            ////  KBQ questions 
            userKbqViewModel.KBQ1 = kbqs.Count > 4 ? kbqs[0].Question.QuestionId.Value : -1;
            userKbqViewModel.KBQ2 = kbqs.Count > 4 ? kbqs[1].Question.QuestionId.Value : -1;
            userKbqViewModel.KBQ3 = kbqs.Count > 4 ? kbqs[2].Question.QuestionId.Value : -1;
            userKbqViewModel.KBQ4 = kbqs.Count > 4 ? kbqs[3].Question.QuestionId.Value : -1;
            userKbqViewModel.KBQ5 = kbqs.Count > 4 ? kbqs[4].Question.QuestionId.Value : -1;


            userKbqViewModel.KBQAnswer1 = kbqs.Count > 4 ? kbqs[0].Answer.Content : "";
            userKbqViewModel.KBQAnswer2 = kbqs.Count > 4 ? kbqs[1].Answer.Content : "";
            userKbqViewModel.KBQAnswer3 = kbqs.Count > 4 ? kbqs[2].Answer.Content : "";
            userKbqViewModel.KBQAnswer4 = kbqs.Count > 4 ? kbqs[3].Answer.Content : "";
            userKbqViewModel.KBQAnswer5 = kbqs.Count > 4 ? kbqs[4].Answer.Content : "";

            //// keep track UserQuestionAnswerId
            userKbqViewModel.UserQuestionAnserId_KBQ1 = kbqs.Count > 4 ? kbqs[0].Answer.UserQuestionAnswerId.Value : -1;
            userKbqViewModel.UserQuestionAnserId_KBQ2 = kbqs.Count > 4 ? kbqs[1].Answer.UserQuestionAnswerId.Value : -1;
            userKbqViewModel.UserQuestionAnserId_KBQ3 = kbqs.Count > 4 ? kbqs[2].Answer.UserQuestionAnswerId.Value : -1;
            userKbqViewModel.UserQuestionAnserId_KBQ4 = kbqs.Count > 4 ? kbqs[3].Answer.UserQuestionAnswerId.Value : -1;
            userKbqViewModel.UserQuestionAnserId_KBQ5 = kbqs.Count > 4 ? kbqs[4].Answer.UserQuestionAnswerId.Value : -1;

            return userKbqViewModel;
        }

        public UserSQViewModel GetUserSecurityQuestionViewModel(int userProfileId)
        {
            var userSQViewModel = new UserSQViewModel();
            userSQViewModel.UserProfileId = userProfileId;

            var securityQeustions = _questionAnswerService.GetUsersQuestionAnswers(userProfileId, QuestionTypeName.SQ);
            var sqs = securityQeustions.Select(i => _mapHelper.GetQuestionAnswerPairViewModelFromQuestionAnswerPairDto(i)).ToList();

            userSQViewModel.QuestionPool = GetQuestionPool(QuestionTypeName.SQ);

            ////  Security questions 
            userSQViewModel.SecurityQuestion1 = sqs.Count > 1 ? sqs[0].Question.QuestionId.Value : -1;
            userSQViewModel.SecurityQuestion2 = sqs.Count > 1 ? sqs[1].Question.QuestionId.Value : -1;

            userSQViewModel.SecurityQuestionAnswer2 = sqs.Count > 1 ? sqs[1].Answer.Content : "";
            userSQViewModel.SecurityQuestionAnswer1 = sqs.Count > 1 ? sqs[0].Answer.Content : "";

            //// Keep track UserQuestionAnswerId 
            userSQViewModel.UserQuestionAnserId_SQ1 = sqs.Count > 1 ? sqs[0].Answer.UserQuestionAnswerId.Value : -1;
            userSQViewModel.UserQuestionAnserId_SQ2 = sqs.Count > 1 ? sqs[1].Answer.UserQuestionAnswerId.Value : -1;

            return userSQViewModel;
        }

        public List<QuestionViewModel> GetQuestionPool(QuestionTypeName type)
        {
            return _questionAnswerService.GetQuestions().Select(i => _mapHelper.GetQuestionViewModelFromQuestionDto(i)).ToList()
                .Where(i => i.QuestionType == type).ToList();
        }

        public List<JurisdictionViewModel> GetStateList()
        {
            var list = _jurisdictionService.GetStateProvs((int) (Country.USA));
            var stateList = new List<JurisdictionViewModel>();
            foreach (var jur in list)
            {
                stateList.Add(new JurisdictionViewModel
                {
                    JurisdictionId = jur.JurisdictionId,
                    StateName = jur.Name
                });
            };

            return stateList;
        }
    }
}