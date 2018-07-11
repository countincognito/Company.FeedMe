using Company.Common.Data;
using System.Threading.Tasks;

namespace Company.Manager.User.Interface
{
    public interface IUserManager
    {
        Task<bool> AddUserAsync(UserDto dto);

        Task<bool> AddUserSilentAsync(UserDto dto);
    }
}
