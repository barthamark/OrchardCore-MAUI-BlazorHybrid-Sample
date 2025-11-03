using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace MarkBartha.HarvestDemo.OrchardCore.ToDos.Models;

public class Todo : ContentPart
{
    public BooleanField Done { get; set; } = new();
}