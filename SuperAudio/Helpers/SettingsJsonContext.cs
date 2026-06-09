using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace SuperAudio.Helpers;

[JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(Dictionary<string, System.Text.Json.JsonElement>))]
[JsonSerializable(typeof(SettingsHelper.SettingsHelper))]
internal partial class SettingsJsonContext : JsonSerializerContext
{
}