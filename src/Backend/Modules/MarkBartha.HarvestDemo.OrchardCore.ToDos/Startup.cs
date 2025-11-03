using MarkBartha.HarvestDemo.OrchardCore.ToDos.Migrations;
using MarkBartha.HarvestDemo.OrchardCore.ToDos.Services;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace MarkBartha.HarvestDemo.OrchardCore.ToDos;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDataMigration, TodoMigrations>();
    }
}
