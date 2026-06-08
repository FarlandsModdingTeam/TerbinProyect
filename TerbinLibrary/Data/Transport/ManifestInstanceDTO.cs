using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TerbinLibrary.Data.Manifests;
using TerbinLibrary.Data.References;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Data.Transport;

public struct ManifestInstanceDTO : IStructSerializable
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public ThreeQuartersInt PluginCount { get; set; } = 0;

    public ManifestInstanceDTO(string pName, string pVersion, ThreeQuartersInt pPlugins) : this(pName, pVersion)
    {
        this.PluginCount = pPlugins;
    }
    public ManifestInstanceDTO(string pName, string pVersion) : this(pName)
    {
        this.Version = pVersion;
    }
    public ManifestInstanceDTO(string pName)
    {
        this.Name = pName;
    }
    public ManifestInstanceDTO()
    {

    }

    [TODO("Optimizar")]
    public readonly int GetSize()
    {
        int size = ThreeQuartersInt.Size;

        size += ((Name?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY;
        size += ((Version?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY;

        return size;
    }

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Name = pBuffer.ReadArray<char>(ref offset).CrString();
        Version = pBuffer.ReadArray<char>(ref offset).CrString();
        PluginCount = pBuffer.Read<ThreeQuartersInt>(ref offset);
    }

    public readonly void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.WriteArray<char>(ref offset, Name?.ToCharArray() ?? "".ToCharArray());
        pBuffer.WriteArray<char>(ref offset, Version?.ToCharArray() ?? "".ToCharArray());
        pBuffer.Write<ThreeQuartersInt>(ref offset, PluginCount);
    }


    public static explicit operator ManifestInstanceDTO(ManifestInstance pData)
    {
        return new ManifestInstanceDTO
        {
            Name = pData.Name,
            Version = pData.Version,
            PluginCount = pData.Plugins.Count,
        };
    }
}
