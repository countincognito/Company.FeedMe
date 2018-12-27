using Company.Access.User.Interface;
using Company.Common.Data;
using Company.Manager.User.Interface;
using PubSub.Extension;
using Serilog;
using System;
using System.Threading.Tasks;
using Zametek.Utility.Logging;

namespace Company.Manager.User.Impl
{
    [DiagnosticLogging(LogActive.On)]
    public class UserManager
        : IUserManager
    {
        private readonly IUserAccess m_UserAccess;
        private readonly ILogger m_Logger;

        public UserManager(
            IUserAccess userAccess,
            ILogger logger)
        {
            m_UserAccess = userAccess ?? throw new ArgumentNullException(nameof(userAccess));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            Task.Run(() => this.Subscribe<FeedMeEvent<int, UserDto>>(async x => await PublishUserAddedEventAsync(x.Id).ConfigureAwait(false)));
        }

        private async Task PublishUserAddedEventAsync(int id)
        {
            m_Logger.Information($"{nameof(PublishUserAddedEventAsync)} Invoked");
            UserDto dto = await m_UserAccess.GetUserAsync(id).ConfigureAwait(false); if (dto != null)
            {
                PublishUserAddedEvent(dto);
            }
        }

        private void PublishUserAddedEvent(UserDto dto)
        {
            m_Logger.Information($"{nameof(PublishUserAddedEvent)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            Task.Run(() => this.Publish<AddedEvent<int, UserDto>>(new AddedEvent<int, UserDto>(dto)));
        }

        #region IUserManager Members

        public async Task<bool> AddUserAsync(UserDto dto)
        {
            m_Logger.Information($"{nameof(AddUserAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            bool result = await AddUserSilentAsync(dto).ConfigureAwait(false);

            if (result)
            {
                PublishUserAddedEvent(dto);
            }

            return result;
        }

        public async Task<bool> AddUserSilentAsync(UserDto dto)
        {
            m_Logger.Information($"{nameof(AddUserSilentAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            int id = await m_UserAccess.AddUserAsync(dto).ConfigureAwait(false);
            return id != 0;
        }

        #endregion
    }
}
