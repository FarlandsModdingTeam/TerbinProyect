using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using TerbinLibrary.Data.Store;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Data.Transport;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */


public struct ReferencePluginStoreDTO : IStructSerializable
{
    public string? Name { get; set; }
    public string? Id { get; set; } // Guid
    public string? FileName { get; set; }
    public string? UrlWeb { get; set; }
    public string? Version { get; set; }

    [TODO("Optimizar")]
    public readonly int GetSize() =>
        ((Name?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY +
        ((Id?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY +
        ((FileName?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY +
        ((UrlWeb?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY +
        ((Version?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY;

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Name = pBuffer.ReadArray<char>(ref offset).CrString();
        Id = pBuffer.ReadArray<char>(ref offset).CrString();
        FileName = pBuffer.ReadArray<char>(ref offset).CrString();
        UrlWeb = pBuffer.ReadArray<char>(ref offset).CrString();
        Version = pBuffer.ReadArray<char>(ref offset).CrString();
    }

    public readonly void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.WriteArray<char>(ref offset, Name?.ToCharArray() ?? "".ToCharArray());
        pBuffer.WriteArray<char>(ref offset, Id?.ToCharArray() ?? "".ToCharArray());
        pBuffer.WriteArray<char>(ref offset, FileName?.ToCharArray() ?? "".ToCharArray());
        pBuffer.WriteArray<char>(ref offset, UrlWeb?.ToCharArray() ?? "".ToCharArray());
        pBuffer.WriteArray<char>(ref offset, Version?.ToCharArray() ?? "".ToCharArray());
    }

    public static explicit operator ReferencePluginStoreDTO(ReferencePluginStore pData)
    {
        return new ReferencePluginStoreDTO
        {
            Name = pData.Name,
            Id = pData.Id,
            FileName = pData.FileName,
            UrlWeb = pData.UrlWeb,
            Version = pData.Version,
        };
    }
}
