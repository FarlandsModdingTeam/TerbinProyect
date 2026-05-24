using System;
using System.Collections.Generic;
using System.Text;
using TerbinService.Data.References;

namespace TerbinService.Data.Manifests;

public class ManifestStorage : IManifest
{
    public string? Name;
    public string? Game;
    public string? Guid;

    public List<ReferencePluginStore>? References;

    public string? GetId()
    {
        return Guid;
    }
}
