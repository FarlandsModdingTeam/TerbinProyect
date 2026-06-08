using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data.Manifests;
using TerbinLibrary.Data.Transport;

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

    public static explicit operator ReferencePluginStore(ReferencePluginStoreDTO pData)
    {
        return new ReferencePluginStore
        {
            Name = pData.Name,
            Id = pData.Id,
            FileName = pData.FileName,
            UrlWeb = pData.UrlWeb,
            Version = pData.Version,
        };
    }
}
