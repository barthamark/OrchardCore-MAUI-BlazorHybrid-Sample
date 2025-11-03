using MarkBartha.HarvestDemo.OrchardCore.ToDos.Constants;
using MarkBartha.HarvestDemo.OrchardCore.ToDos.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace MarkBartha.HarvestDemo.OrchardCore.ToDos.Migrations;

public class TodoMigrations: DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public TodoMigrations(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;
    
    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(Todo), part => part
            .WithField(nameof(Todo.Done), field => field
                .OfType("BooleanField")
                .WithDisplayName("Done"))
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync(ContentTypes.Todo, type => type
            .Creatable()
            .Listable()
            .Securable()
            .WithPart("TitlePart")
            .WithPart(nameof(Todo))
        );

        return 1;
    }
}