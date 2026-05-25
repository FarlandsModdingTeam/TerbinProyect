using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TerbinLibrary.Data;


public class DirectoryHandwritten
{
    public List<string> Directories { get; set; } = new();
    public List<string> Files { get; set; } = new();

    public string ToJson(JsonSerializerOptions options) => JsonSerializer.Serialize(this, options);
    public string ToJson() => JsonSerializer.Serialize(this, _options);
    public long GetSize() => (long)Directories.Count + (long)Files.Count;

    [JsonIgnore]
    private static JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };
}
