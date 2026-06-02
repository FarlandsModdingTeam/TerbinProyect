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



// ****************( Prototipos )**************** //
public interface IManifest_Prototype
{
    IManifestId? Id { get; set; }
    string? NodeName { get; set; }
    string? GetFullPath();
    string? GetLocalPath();
}
public interface IManifestEmbebed_Prototype<T> : IManifest_Prototype
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
