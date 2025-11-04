using System.Text.Json;

namespace MarkBartha.HarvestDemo.App.Maui.Constants;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}