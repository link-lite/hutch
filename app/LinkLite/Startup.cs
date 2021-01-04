using LinkLite.OptionsModels;
using LinkLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkLite
{
    public static class Startup
    {
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<RquestConnectorApiOptions>(
                context.Configuration.GetSection("RquestConnectorApi"));

            services.Configure<RquestPollingServiceOptions>(context.Configuration);

            services.AddHttpClient<RquestConnectorApiClient>();
        }
    }
}
