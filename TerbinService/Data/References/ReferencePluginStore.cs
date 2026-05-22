using System;
using System.Collections.Generic;
using System.Text;
using TerbinService.Data.Manifests;

namespace TerbinService.Data.References;

public class ReferencePluginStore : IManifest
{
    public string? Name;
    public string? Id; // Guid
    public string? FileName;
    public string? UrlWeb { get; set; }
    public string? Version { get; set; }

    public string? GetId()
    {
        return Id;
    }
}
