using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Data.References;
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



public struct ReferenceInstanceDTO : IStructSerializable
{
    public string? Name;
    public bool? OutSide;
    public string? Path;

    [TODO("Optimizar")]
    public readonly int GetSize() =>
        ((Name?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY +
        ((Path?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY + 
        1;

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Name = pBuffer.ReadArray<char>(ref offset).CrString();
        OutSide = pBuffer.Read<sbyte>(ref offset).ToBoolUk();
        Path = pBuffer.ReadArray<char>(ref offset).CrString();
    }

    public readonly void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.WriteArray<char>(ref offset, Name?.ToCharArray() ?? "".ToCharArray());
        pBuffer.Write<sbyte>(ref offset, OutSide.ToSByte());
        pBuffer.WriteArray<char>(ref offset, Path?.ToCharArray() ?? "".ToCharArray());
    }


    public static explicit operator ReferenceInstanceDTO(ReferenceInstance pData)
    {
        return new ReferenceInstanceDTO
        {
            Name = pData.Name,
            OutSide = pData.OutSide,
            Path = pData.Path,
        };
    }
}