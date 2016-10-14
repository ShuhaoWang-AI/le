using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;
using System;

namespace Linko.LinkoExchange.Services.Permission
{
    // TODO to define more methods
    public interface IPermissionService
    {
        IEnumerable<UserDto> GetApprovalPeople(int userId, int organizationId);
    }

    public class PermissionService : IPermissionService
    {
        // TODO: to implement 
        public IEnumerable<UserDto> GetApprovalPeople(int userId, int organizationId)
        {
            return new UserDto[]
            {
                 new UserDto
                 {
                      Email = "Shuhao.wang@watertrax.com"
                 },

                new UserDto{
                     Email = "shuhao.wang@watertax.com"
                }
             };
        }
    }
}
