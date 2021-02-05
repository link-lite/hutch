using LinkLite.Data;
using LinkLite.OptionsModels;
using LinkLite.Services;
using LinkLite.Services.QueryServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkLite
{
    public static class Startup
    {
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddDbContext<OmopContext>(options =>
                options.UseNpgsql(context.Configuration.GetConnectionString("Omop")));

            services.Configure<RquestConnectorApiOptions>(
                context.Configuration.GetSection("RquestConnectorApi"));

            services.Configure<RquestPollingServiceOptions>(context.Configuration);

            services.AddHttpClient<RquestConnectorApiClient>();

            services.AddTransient<RquestOmopQueryService>();
        }
    }
}
