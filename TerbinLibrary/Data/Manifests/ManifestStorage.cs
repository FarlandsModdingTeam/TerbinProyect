using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data.References;

namespace TerbinLibrary.Data.Manifests;

public class ManifestStorage : IManifest
{
    public string? Name;
    public string? Game;
    public string? Guid;

    public List<ReferencePluginStore> References = new();

    public string? GetId()
    {
        return Guid;
    }
}
