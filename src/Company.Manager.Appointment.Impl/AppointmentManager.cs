using Company.Access.Appointment.Interface;
using Company.Common.Data;
using Company.Manager.Appointment.Interface;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using PubSub.Extension;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Zametek.Utility.Logging;

namespace Company.Manager.Appointment.Impl
{
    [DiagnosticLogging(LogActive.On)]
    public class AppointmentManager
        : IAppointmentManager
    {
        private readonly IAppointmentAccess m_AppointmentAccess;
        private readonly ILogger m_Logger;
        private readonly string m_AppointmentQueueConnectionString;
        private const string c_QueueName = "appointments";
        private readonly Timer m_QueueTimer;

        private readonly object m_AppointmentTableLock;
        private readonly IDictionary<int, AppointmentDto> m_AppointmentTableStorage;
        private readonly Timer m_TableTimer;

        public AppointmentManager(
            IAppointmentAccess appointmentAccess,
            ILogger logger,
            string appointmentQueueConnectionString)
        {
            m_AppointmentAccess = appointmentAccess ?? throw new ArgumentNullException(nameof(appointmentAccess));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_AppointmentQueueConnectionString = appointmentQueueConnectionString ?? throw new ArgumentNullException(nameof(appointmentQueueConnectionString));

            m_QueueTimer = new Timer(10000);
            m_QueueTimer.Elapsed += CheckAppointmentQueueStorage;
            m_QueueTimer.AutoReset = true;
            m_QueueTimer.Enabled = true;

            m_AppointmentTableLock = new object();
            m_AppointmentTableStorage = new Dictionary<int, AppointmentDto>();

            m_TableTimer = new Timer(10000);
            m_TableTimer.Elapsed += CheckAppointmentTableStorage;
            m_TableTimer.AutoReset = true;
            m_TableTimer.Enabled = true;

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            Task.Run(() => this.Subscribe<AddedEvent<int, UserDto>>(async x => await AddUserAsync(x.Dto).ConfigureAwait(false)));
            Task.Run(() => this.Subscribe<AddedEvent<int, OrganisationDto>>(async x => await AddOrganisationAsync(x.Dto).ConfigureAwait(false)));
        }

        private void CheckAppointmentQueueStorage(object source, ElapsedEventArgs e)
        {
            m_Logger.Information($"{nameof(CheckAppointmentQueueStorage)} Invoked");
            try
            {
                m_QueueTimer.Stop();
                CloudQueue queue = GetQueue();
                CloudQueueMessage message = queue.GetMessageAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                if (message != null)
                {
                    AppointmentDto appointment = AppointmentDto.DeSerialize<AppointmentDto>(message.AsBytes);
                    AddAppointmentAsync(appointment).ConfigureAwait(false).GetAwaiter().GetResult();
                    queue.DeleteMessageAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Error caught checking for appointments in queue storage.");
            }
            finally
            {
                m_QueueTimer.Start();
            }
        }

        private void CheckAppointmentTableStorage(object source, ElapsedEventArgs e)
        {
            m_Logger.Information($"{nameof(CheckAppointmentTableStorage)} Invoked");
            try
            {
                m_TableTimer.Stop();
                var appointments = new List<AppointmentDto>();

                lock (m_AppointmentTableLock)
                {
                    appointments.AddRange(m_AppointmentTableStorage.Values);
                    m_AppointmentTableStorage.Clear();
                }

                foreach (AppointmentDto appointment in appointments)
                {
                    PostAppointmentToQueueStorageAsync(appointment).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            finally
            {
                m_TableTimer.Start();
            }
        }

        private async Task PostAppointmentToQueueStorageAsync(AppointmentDto appointment)
        {
            m_Logger.Information($"{nameof(PostAppointmentToQueueStorageAsync)} Invoked");
            try
            {
                CloudQueue queue = GetQueue();
                CloudQueueMessage message = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(
                    AppointmentDto.Serialize<AppointmentDto>(appointment));

                await queue.AddMessageAsync(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex, "Error caught posting appointment to queue storage.");
            }
        }

        private void PostAppointmentToTableStorage(AppointmentDto dto)
        {
            m_Logger.Information($"{nameof(PostAppointmentToTableStorage)} Invoked");
            lock (m_AppointmentTableLock)
            {
                if (!m_AppointmentTableStorage.ContainsKey(dto.Id))
                {
                    m_AppointmentTableStorage.Add(dto.Id, dto);
                }
            }
        }

        private async Task AddUserAsync(UserDto dto)
        {
            m_Logger.Information($"{nameof(AddUserAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            await m_AppointmentAccess.AddUserAsync(dto).ConfigureAwait(false);
        }

        private async Task AddOrganisationAsync(OrganisationDto dto)
        {
            m_Logger.Information($"{nameof(AddOrganisationAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            await m_AppointmentAccess.AddOrganisationAsync(dto).ConfigureAwait(false);
        }

        private void PublishFeedMeEvent<TKey, T>(TKey id)
            where TKey : struct
            where T : DtoBase<TKey>
        {
            m_Logger.Information($"{nameof(PublishFeedMeEvent)} Invoked");
            Task.Run(() => this.Publish(new FeedMeEvent<TKey, T>(id)));
        }

        private CloudQueue GetQueue()
        {
            if (!CloudStorageAccount.TryParse(m_AppointmentQueueConnectionString, out CloudStorageAccount storageAccount))
            {
                throw new Exception(@"Unable to connect to cloud storage");
            }
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(c_QueueName);
            queue.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return queue;
        }

        #region IAppointmentManager Members

        public async Task<bool> AddAppointmentAsync(AppointmentDto dto)
        {
            m_Logger.Information($"{nameof(AddAppointmentAsync)} Invoked");
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            UserDto userDto = await m_AppointmentAccess.GetUserAsync(dto.UserId).ConfigureAwait(false);
            OrganisationDto organisationDto = await m_AppointmentAccess.GetOrganisationAsync(dto.OrganisationId).ConfigureAwait(false);

            bool deferAppointment = false;

            if (userDto == null)
            {
                PublishFeedMeEvent<int, UserDto>(dto.UserId);
                deferAppointment = true;
            }

            if (organisationDto == null)
            {
                PublishFeedMeEvent<int, OrganisationDto>(dto.OrganisationId);
                deferAppointment = true;
            }

            if (deferAppointment)
            {
                // Stick the task into a table for later.
                PostAppointmentToTableStorage(dto);
                return false;
            }

            int id = await m_AppointmentAccess.AddAppointmentAsync(dto).ConfigureAwait(false);
            return id != 0;
        }

        public async Task<AppointmentDto> GetAppointmentAsync(int appointmentId)
        {
            m_Logger.Information($"{nameof(GetAppointmentAsync)} Invoked");
            return await m_AppointmentAccess.GetAppointmentAsync(appointmentId).ConfigureAwait(false);
        }

        #endregion
    }
}
