using Company.Common.Data;
using System.Threading.Tasks;

namespace Company.Manager.Organisation.Interface
{
    public interface IOrganisationManager
    {
        Task<bool> AddOrganisationAsync(OrganisationDto dto);

        Task<bool> AddOrganisationSilentAsync(OrganisationDto dto);
    }
}
