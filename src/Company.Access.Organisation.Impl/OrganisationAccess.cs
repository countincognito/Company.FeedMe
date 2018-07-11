using Company.Access.Organisation.Interface;
using Company.Common.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zametek.Utility.Logging;

namespace Company.Access.Organisation.Impl
{
    [DiagnosticLogging(LogActive.On)]
    public class OrganisationAccess
        : IOrganisationAccess
    {
        private readonly object m_OrganisationLock;
        private readonly IDictionary<int, OrganisationDto> m_OrganisationStorage;

        private readonly ILogger m_Logger;

        public OrganisationAccess(ILogger logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_OrganisationLock = new object();
            m_OrganisationStorage = new Dictionary<int, OrganisationDto>();
        }

        #region IOrganisationAccess Members

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

        #endregion
    }
}
