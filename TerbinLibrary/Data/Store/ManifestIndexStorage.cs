using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Data.Store;

public class ManifestIndexStorage : IManifest
{
    public string? Name { get; set; }
    public string? Game { get; set; }

    public string? IdLocal { get; set; }
    public string? KeySteam { get; set; }

    public List<ReferencePluginStore> References = new();

    public string? GetId()
    {
        return IdLocal;
    }
}
