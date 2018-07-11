using Company.Common.Data;
using System;
using System.Threading.Tasks;

namespace Company.Access.User.Interface
{
    public interface IUserAccess
    {
        Task<int> AddUserAsync(UserDto dto);

        Task<UserDto> GetUserAsync(int userId);
    }
}
