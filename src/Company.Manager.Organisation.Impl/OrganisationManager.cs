using Company.Access.Organisation.Interface;
using Company.Common.Data;
using Company.Manager.Organisation.Interface;
using PubSub;
using Serilog;
using System;
using System.Threading.Tasks;
using Zametek.Utility.Logging;

namespace Company.Manager.Organisation.Impl
{
    [DiagnosticLogging(LogActive.On)]
    public class OrganisationManager
        : IOrganisationManager
    {
        private readonly IOrganisationAccess m_OrganisationAccess;
        private readonly ILogger m_Logger;

        public OrganisationManager(
            IOrganisationAccess organisationAccess,
            ILogger logger)
        {
            m_OrganisationAccess = organisationAccess ?? throw new ArgumentNullException(nameof(organisationAccess));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            Task.Run(() => this.Subscribe<FeedMeEvent<int, OrganisationDto>>(async x => await PublishOrganisationAddedEventAsync(x.Id).ConfigureAwait(false)));
        }

        private async Task PublishOrganisationAddedEventAsync(int id)
        {
            m_Logger.Information($"{nameof(PublishOrganisationAddedEventAsync)} Invoked");
            OrganisationDto dto = await m_OrganisationAccess.GetOrganisationAsync(id).ConfigureAwait(false);
            if (dto != null)
            {
                PublishOrganisationAddedEvent(dto);
            }
        }

        private void PublishOrganisationAddedEvent(OrganisationDto dto)
        {
            m_Logger.Information($"{nameof(PublishOrganisationAddedEvent)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            Task.Run(() => this.Publish<AddedEvent<int, OrganisationDto>>(new AddedEvent<int, OrganisationDto>(dto)));
        }

        #region IOrganisationManager Members

        public async Task<bool> AddOrganisationAsync(OrganisationDto dto)
        {
            m_Logger.Information($"{nameof(AddOrganisationAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            bool result = await AddOrganisationSilentAsync(dto).ConfigureAwait(false);

            if (result)
            {
                PublishOrganisationAddedEvent(dto);
            }

            return result;
        }

        public async Task<bool> AddOrganisationSilentAsync(OrganisationDto dto)
        {
            m_Logger.Information($"{nameof(AddOrganisationSilentAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            int id = await m_OrganisationAccess.AddOrganisationAsync(dto).ConfigureAwait(false);
            return id != 0;
        }

        #endregion
    }
}
