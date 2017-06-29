﻿using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Program
{
    public interface IProgramService
    {
        /// <summary>
        /// Get all the OrganizationRegulatoryProgramDto(s) that the users have access to
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <param name="isIncludeRemoved">True if we want to include org reg programs the user has been removed from (for logging purposes)</param>
        /// <returns></returns>
        IEnumerable<OrganizationRegulatoryProgramUserDto> GetUserRegulatoryPrograms(string email, bool isIncludeRemoved = false);
        /// <summary>
        /// Get programs that a user can access
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>A collection of programs</returns>
        IEnumerable<OrganizationRegulatoryProgramDto> GetUserRegulatoryPrograms(int userId);

        /// <summary>
        /// Get the organization regulatory program 
        /// </summary>
        /// <param name="organizationRegulatoryProgramId">The organization regulatory program Id</param>
        /// <returns></returns>
        OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgram(int organizationRegulatoryProgramId);

        OrganizationRegulatoryProgramUserDto CreateOrganizationRegulatoryProgramForUser(int userProfileId, int organizationRegulatoryProgramId, int inviterOrganizationRegulatoryProgramId, RegistrationType registrationType);

        IEnumerable<OrganizationRegulatoryProgramDto> GetChildOrganizationRegulatoryPrograms(int currentOrganizationRegulatoryProgramId, string searchString);

    }
}
