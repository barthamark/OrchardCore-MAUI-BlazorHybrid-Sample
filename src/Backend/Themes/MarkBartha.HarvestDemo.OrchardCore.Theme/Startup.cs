using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace MarkBartha.HarvestDemo.OrchardCore.Theme;

public class Startup : StartupBase
{
    override public void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IDataMigration, OpenIdApplicationMigrations>();
}