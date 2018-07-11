using Company.Common.Data;
using System;
using System.Threading.Tasks;

namespace Company.Manager.Appointment.Interface
{
    public interface IAppointmentManager
    {
        Task<bool> AddAppointmentAsync(AppointmentDto dto);

        Task<AppointmentDto> GetAppointmentAsync(int appointmentId);
    }
}
