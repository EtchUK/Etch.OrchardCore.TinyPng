using Etch.OrchardCore.TinyPNG.Drivers;
using Etch.OrchardCore.TinyPNG.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace Etch.OrchardCore.TinyPNG
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<ISite>, TinyPngSettingsDisplayDriver>();

            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<ITinyPngService, TinyPngService>();
        }
    }
}
