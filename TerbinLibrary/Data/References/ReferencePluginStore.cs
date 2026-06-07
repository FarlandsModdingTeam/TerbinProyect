using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data.Manifests;

namespace TerbinLibrary.Data.References;

public class ReferencePluginStore : IManifest
{
    public string? Name { get; set; }
    public string? Id { get; set; } // Guid
    public string? FileName { get; set; }
    public string? UrlWeb { get; set; }
    public string? Version { get; set; }

    public string? GetId()
    {
        return Id;
    }
}
