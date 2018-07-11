using Company.Common.Data;
using System;
using System.Threading.Tasks;

namespace Company.Access.Appointment.Interface
{
    public interface IAppointmentAccess
    {
        Task<int> AddUserAsync(UserDto dto);

        Task<UserDto> GetUserAsync(int userId);

        Task<int> AddOrganisationAsync(OrganisationDto dto);

        Task<OrganisationDto> GetOrganisationAsync(int organisationId);

        Task<int> AddAppointmentAsync(AppointmentDto dto);

        Task<AppointmentDto> GetAppointmentAsync(int appointmentId);
    }
}
