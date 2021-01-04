using LinkLite;
using LinkLite.HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;

// Configure the Host
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(b =>
        b.AddUserSecrets(Assembly.GetAssembly(typeof(Startup))))
    .UseSerilog((context, services, loggerConfig) => loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext())

    // Configure dependencies, like ASP.NET Core
    .ConfigureServices(Startup.ConfigureServices)

    // Add our actual Hosted Services
    .ConfigureServices(services =>
    {
        services.AddHostedService<RquestPollingService>();
    });

// Run the app
host.Build().Run();
