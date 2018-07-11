using AutoMapper;
using Company.Api.Rest.Data;
using Company.Common.Data;
using Company.Manager.Appointment.Interface;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Zametek.Utility;

namespace Company.Api.Rest.Service
{
    [Route("api/[controller]")]
    public class AppointmentsController
        : Controller
    {
        private readonly IMapper m_Mapper;
        private readonly IAppointmentManager m_AppointmentManager;
        private readonly ILogger m_Logger;

        public AppointmentsController(
            IMapper mapper,
            IAppointmentManager appointmentManager,
            ILogger logger)
        {
            m_Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            m_AppointmentManager = appointmentManager ?? throw new ArgumentNullException(nameof(appointmentManager));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Debug.Assert(TrackingContext.Current != null);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody]AppointmentRequestDto requestDto)
        {
            m_Logger.Information($"{nameof(Add)} Invoked");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var request = m_Mapper.Map<AppointmentDto>(requestDto);
                bool result = await m_AppointmentManager.AddAppointmentAsync(request).ConfigureAwait(false);
                if (result)
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Error caught in the controller class.");
            }
            return BadRequest(HttpStatusCode.BadRequest);
        }

        [HttpPost("find")]
        public async Task<IActionResult> Find([FromBody]int appointmentId)
        {
            m_Logger.Information($"{nameof(Find)} Invoked");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                AppointmentDto response = await m_AppointmentManager.GetAppointmentAsync(appointmentId).ConfigureAwait(false);
                var responseDto = m_Mapper.Map<AppointmentResponseDto>(response);
                if (responseDto != null)
                {
                    return Ok(responseDto);
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Error caught in the controller class.");
            }
            return BadRequest(HttpStatusCode.BadRequest);
        }

    }
}
