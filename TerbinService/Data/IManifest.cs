using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinService.Data;

public interface IManifest
{
    string? GetId();
}

/// <summary>
/// ______( Manifiesto de la instancia )______<br />
/// - Contiene información sobre la instancia, como su nombre, versión y mods instalados.
/// </summary>
public class InstanceManifest : IManifest
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? Executable { get; set; }
    public List<string>? Plugins { get; set; }

    public string? GetId()
    {
        return Name;
    }
}
[Obsolete]
public class ModManifest : IManifest
{
    public string? Name { get; set; }
    public string? Owner { get; set; }
    public string? Version { get; set; }

    public string? GetId()
    {
        return Name + ":" + Owner;
    }
}
public class PluginManifest : IManifest
{
    public string? Name { get; set; }
    public string? Owner { get; set; }
    public string? UrlWeb { get; set; }
    public string? Version { get; set; }
    public string? Content { get; set; }

    public string? GetId()
    {
        return Name + ":" + Owner;
    }
}