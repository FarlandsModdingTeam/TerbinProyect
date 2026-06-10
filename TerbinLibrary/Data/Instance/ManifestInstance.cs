using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data.Plugin;
using TerbinLibrary.Data.Transport;

namespace TerbinLibrary.Data.Instance;


/// <summary>
/// ______( Manifiesto de la instancia )______<br />
/// - Contiene información sobre la instancia, como su nombre, versión y mods instalados.
/// </summary>
public class ManifestInstance : IManifest
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Executable { get; set; }
    public List<ReferencePlugin> Plugins { get; set; } = [];

    public string? GetId()
    {
        return Name;
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