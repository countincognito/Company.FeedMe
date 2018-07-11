using System;

namespace Company.Api.Rest.Data
{
    [Serializable]
    public class AppointmentRequestDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int OrganisationId { get; set; }
    }
}
