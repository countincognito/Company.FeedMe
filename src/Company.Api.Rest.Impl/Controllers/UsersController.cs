using AutoMapper;
using Company.Api.Rest.Data;
using Company.Common.Data;
using Company.Manager.User.Interface;
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
    public class UsersController
        : Controller
    {
        private readonly IMapper m_Mapper;
        private readonly IUserManager m_UserManager;
        private readonly ILogger m_Logger;

        public UsersController(
            IMapper mapper,
            IUserManager userManager,
            ILogger logger)
        {
            m_Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            m_UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Debug.Assert(TrackingContext.Current != null);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Post([FromBody]UserRequestDto requestDto)
        {
            m_Logger.Information($"{nameof(Post)} Invoked");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var request = m_Mapper.Map<UserDto>(requestDto);
                bool result = false;

                if (requestDto.Silent)
                {
                    result = await m_UserManager.AddUserSilentAsync(request).ConfigureAwait(false);
                }
                else
                {
                    result = await m_UserManager.AddUserAsync(request).ConfigureAwait(false);
                }

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
    }
}
