using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinService.Data.Manifests;

internal class ManifestStorage : IManifest
{
    public string? Game;
    public string? Guid;

    public string? GetId()
    {
        throw new NotImplementedException();
    }
}
