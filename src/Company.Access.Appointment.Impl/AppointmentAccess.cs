using Company.Access.Appointment.Interface;
using Company.Common.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zametek.Utility.Logging;

namespace Company.Access.Appointment.Impl
{
    [DiagnosticLogging(LogActive.On)]
    public class AppointmentAccess
        : IAppointmentAccess
    {
        private readonly object m_UserLock;
        private readonly IDictionary<int, UserDto> m_UserStorage;

        private readonly object m_OrganisationLock;
        private readonly IDictionary<int, OrganisationDto> m_OrganisationStorage;

        private readonly object m_AppointmentLock;
        private readonly IDictionary<int, AppointmentDto> m_AppointmentStorage;

        private readonly ILogger m_Logger;

        public AppointmentAccess(ILogger logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_UserLock = new object();
            m_UserStorage = new Dictionary<int, UserDto>();
            m_OrganisationLock = new object();
            m_OrganisationStorage = new Dictionary<int, OrganisationDto>();
            m_AppointmentLock = new object();
            m_AppointmentStorage = new Dictionary<int, AppointmentDto>();
        }

        #region IAppointmentAccess Members

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

        public async Task<int> AddOrganisationAsync(OrganisationDto dto)
        {
            m_Logger.Information($"{nameof(AddOrganisationAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            await Task.Yield();

            lock (m_OrganisationLock)
            {
                int id = dto.Id;
                if (id <= 0)
                {
                    throw new InvalidDataException($"{nameof(id)} must be greater than 0");
                }

                if (m_OrganisationStorage.ContainsKey(id))
                {
                    id = 0;
                }
                else
                {
                    m_OrganisationStorage.Add(id, dto);
                }

                return id;
            }
        }

        public async Task<OrganisationDto> GetOrganisationAsync(int organisationId)
        {
            m_Logger.Information($"{nameof(GetOrganisationAsync)} Invoked");
            if (organisationId <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(organisationId)} must be greater than 0");
            }
            await Task.Yield();

            lock (m_OrganisationLock)
            {
                if (!m_OrganisationStorage.TryGetValue(organisationId, out OrganisationDto output))
                {
                    output = null;
                }
                return output;
            }
        }

        public async Task<int> AddAppointmentAsync(AppointmentDto dto)
        {
            m_Logger.Information($"{nameof(AddAppointmentAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            await Task.Yield();

            lock (m_AppointmentLock)
            {
                int id = dto.Id;
                if (id <= 0)
                {
                    throw new InvalidDataException($"{nameof(id)} must be greater than 0");
                }

                if (m_AppointmentStorage.ContainsKey(id))
                {
                    id = 0;
                }
                else
                {
                    m_AppointmentStorage.Add(id, dto);
                }

                return id;
            }
        }

        public async Task<AppointmentDto> GetAppointmentAsync(int appointmentId)
        {
            m_Logger.Information($"{nameof(GetAppointmentAsync)} Invoked");
            if (appointmentId <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(appointmentId)} must be greater than 0");
            }
            await Task.Yield();

            lock (m_AppointmentLock)
            {
                if (!m_AppointmentStorage.TryGetValue(appointmentId, out AppointmentDto output))
                {
                    output = null;
                }
                return output;
            }
        }

        #endregion
    }
}
