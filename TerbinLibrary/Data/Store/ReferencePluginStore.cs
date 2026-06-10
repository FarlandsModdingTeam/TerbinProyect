using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Data.Store;

public class ReferencePluginStore : IManifest, IToDTO<ReferencePluginStoreDTO>, IToSerializeDTO
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

    public ReferencePluginStoreDTO ToDTO()
    {
        return (ReferencePluginStoreDTO)this;
    }

    public byte[] ToSerilizeDTO()
    {
        return ((ReferencePluginStoreDTO)this).Serialize();
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
