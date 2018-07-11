using System;

namespace Company.Common.Data
{

    [Serializable]
    public class AppointmentDto
        : DtoBase<int>
    {
        public AppointmentDto(int id, int userId, int organisationId)
            : base(id)
        {
            UserId = userId;
            OrganisationId = organisationId;
        }

        public int UserId
        {
            get;
        }

        public int OrganisationId
        {
            get;
        }
    }
}
