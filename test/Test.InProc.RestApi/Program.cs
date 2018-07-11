using Company.Access.Appointment.Impl;
using Company.Access.Appointment.Interface;
using Company.Access.Organisation.Impl;
using Company.Access.Organisation.Interface;
using Company.Access.User.Impl;
using Company.Access.User.Interface;
using Company.Api.Rest.Impl;
using Company.Manager.Appointment.Impl;
using Company.Manager.Appointment.Interface;
using Company.Manager.Organisation.Impl;
using Company.Manager.Organisation.Interface;
using Company.Manager.User.Impl;
using Company.Manager.User.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using Zametek.Utility.Logging;

namespace Test.InProc.RestApi
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            ILogger serilog = new LoggerConfiguration()
                .Enrich.FromLogProxy()
                //.Destructure.UsingAttributes()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
            Log.Logger = serilog;

            BuildWebHost(serilog).Run();
        }

        public static IWebHost BuildWebHost(ILogger serilog)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            string appointmentsQueueConnectionString = Configuration[@"AppointmentManager:QueueConnectionString"];

            LogType logType = LogType.Tracking | LogType.Performance | LogType.Error | LogType.Diagnostic;

            var userAccess = LogProxy.Create<IUserAccess>(new UserAccess(serilog), serilog, logType);
            var organisationAccess = LogProxy.Create<IOrganisationAccess>(new OrganisationAccess(serilog), serilog, logType);
            var appointmentAccess = LogProxy.Create<IAppointmentAccess>(new AppointmentAccess(serilog), serilog, logType);

            var userManager = LogProxy.Create<IUserManager>(new UserManager(userAccess, serilog), serilog, logType);
            var organisationManager = LogProxy.Create<IOrganisationManager>(new OrganisationManager(organisationAccess, serilog), serilog, logType);
            var appointmentManager = LogProxy.Create<IAppointmentManager>(new AppointmentManager(appointmentAccess, serilog, appointmentsQueueConnectionString), serilog, logType);

            var restApiLogger = serilog;

            return new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(
                    services => services
                        .AddSingleton(userManager)
                        .AddSingleton(organisationManager)
                        .AddSingleton(appointmentManager)
                        .AddSingleton(restApiLogger))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();
        }
    }
}
