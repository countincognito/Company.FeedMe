using AutoMapper;
using Company.Api.Rest.Data;
using Company.Common.Data;

namespace Company.Api.Rest.Impl
{
    public class RestProfile
        : Profile
    {
        public RestProfile()
        {
            CreateMap<AppointmentRequestDto, AppointmentDto>();
            CreateMap<UserRequestDto, UserDto>();
            CreateMap<OrganisationRequestDto, OrganisationDto>();
        }
    }
}
