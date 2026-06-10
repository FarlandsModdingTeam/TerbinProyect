using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data.Plugin;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Data.Instance;


/// <summary>
/// ______( Manifiesto de la instancia )______<br />
/// - Contiene información sobre la instancia, como su nombre, versión y mods instalados.
/// </summary>
public class ManifestInstance : IManifest, IToDTO<ManifestInstanceDTO>, IToSerializeDTO
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Executable { get; set; }
    public List<ReferencePlugin> Plugins { get; set; } = [];

    public string? GetId()
    {
        return Name;
    }

    public ManifestInstanceDTO ToDTO()
    {
        return (ManifestInstanceDTO)this;
    }

    public byte[] ToSerilizeDTO()
    {
        return ((ManifestInstanceDTO)this).Serialize();
    }

    public static explicit operator ManifestInstance(ManifestInstanceDTO pData)
    {
        return new ManifestInstance
        {
            Name = pData.Name,
            Version = pData.Version,
        };
    }
}