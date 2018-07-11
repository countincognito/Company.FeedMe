using Company.Common.Data;
using System;
using System.Threading.Tasks;

namespace Company.Access.Organisation.Interface
{
    public interface IOrganisationAccess
    {
        Task<int> AddOrganisationAsync(OrganisationDto dto);

        Task<OrganisationDto> GetOrganisationAsync(int organisationId);
    }
}
