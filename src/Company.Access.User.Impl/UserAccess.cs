using Company.Access.User.Interface;
using Company.Common.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zametek.Utility.Logging;

namespace Company.Access.User.Impl
{
    [DiagnosticLogging(LogActive.On)]
    public class UserAccess
        : IUserAccess
    {
        private readonly object m_UserLock;
        private readonly IDictionary<int, UserDto> m_UserStorage;

        private readonly ILogger m_Logger;

        public UserAccess(ILogger logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_UserLock = new object();
            m_UserStorage = new Dictionary<int, UserDto>();
        }

        #region IUserAccess Members

        public async Task<int> AddUserAsync(UserDto dto)
        {
            m_Logger.Information($"{nameof(AddUserAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            await Task.Yield();

            lock (m_UserLock)
            {
                int id = dto.Id;
                if (id <= 0)
                {
                    throw new InvalidDataException($"{nameof(id)} must be greater than 0");
                }

                if (m_UserStorage.ContainsKey(id))
                {
                    id = 0;
                }
                else
                {
                    m_UserStorage.Add(id, dto);
                }

                return id;
            }
        }

        public async Task<UserDto> GetUserAsync(int userId)
        {
            m_Logger.Information($"{nameof(GetUserAsync)} Invoked");
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(userId)} must be greater than 0");
            }
            await Task.Yield();

            lock (m_UserLock)
            {
                if (!m_UserStorage.TryGetValue(userId, out UserDto output))
                {
                    output = null;
                }
                return output;
            }
        }

        #endregion
    }
}
