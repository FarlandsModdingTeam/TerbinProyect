using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using TerbinLibrary.Data;
using TerbinService.Data.References;

namespace TerbinService.Data.Manifests;

internal interface IManifest
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
    public List<ReferencePlugin> Plugins { get; set; } = [];

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
    public string? Id { get; set; }
    public string? UrlWeb { get; set; }
    public string? Version { get; set; }
    public string? PathRoot { get; set; }
    public DirectoryHandwritten? Content { get; set; }

    public string? GetId()
    {
        return Name + ":" + Owner;
    }
}


// ****************( Prototipos )**************** //

public interface IManifest2
{
    IManifestId? Id { get; set; }
    string? NodeName { get; set; }
    string? GetFullPath();
    string? GetLocalPath();
}
public interface IManifest2Embebed<T> : IManifest2
{
    List<T> Values { get; set; }
}
public interface IManifestId
{
    string? Id { get; set; }
    [JsonIgnore]
    Guid? IdGuid { get; set; }
}
public class ManifestId : IManifestId
{
    public string? Id
    { get => field; set => field = value; }
    [JsonIgnore]
    public Guid? IdGuid
    {
        get
        {
            return (Id is null) ? null : Guid.Parse(Id);
        }
        set
        {
            if (value is null) return;
            Id = $"{value:N}";
        }
    }
}
