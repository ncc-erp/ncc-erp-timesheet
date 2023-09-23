using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncc.Authorization.Users;
using Timesheet.DomainServices.Dto;
using System.Threading.Tasks;

namespace Timesheet.DomainServices
{
    public interface IUserServices : IDomainService
    {
        IQueryable<User> GetUserByRole(string roleName);
        bool UserHasRole(long userId, string roleName);
        IQueryable<string> UserAllRoles(long userId);
        Task<User> CreateUserAsync(CreateUserDto input);
        Task<UserDto> UpdateUserAsync(UserDto input,bool isUpdateUserWorkingTime);
        Task<ulong?> UpdateKomuUserId(long userId);
        Task<long?> GetUserIdByEmail(string email);
        Task<NotifyUserInfoDto> getKomuUserInfo(long userId);
        Task<User> CreateUserFromHrmv2Async(CreateUpdateByHRMV2Dto input);
        Task<User> UpdateUserFromHrmV2Async(CreateUpdateByHRMV2Dto input);
    }
}
