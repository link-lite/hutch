using LinkLite.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Configure the Host
var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, loggerConfig) => loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext())

    // Configure dependencies, like ASP.NET Core
    .ConfigureServices(LinkLite.Startup.ConfigureServices)

    // Add our actual Hosted Services
    .ConfigureServices((services =>
    {
        services.AddHostedService<RquestPollingService>();
    }));

// Run the app
host.Build().Run();
