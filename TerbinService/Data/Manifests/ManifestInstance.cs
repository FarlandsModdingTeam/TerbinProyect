using System;
using System.Collections.Generic;
using System.Text;
using TerbinService.Data.References;

namespace TerbinService.Data.Manifests;


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
}