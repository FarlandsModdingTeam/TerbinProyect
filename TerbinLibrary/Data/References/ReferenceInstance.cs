using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Data.References;

public class ReferenceInstance 
{
    public string? Name;
    public bool? OutSide;
    public string? Path;

    public static explicit operator ReferenceInstance(ReferenceInstanceSerilizable pData)
    {
        return new ReferenceInstance
        {
            Name = pData.Name,
            OutSide = pData.OutSide,
            Path = pData.Path,
        };
    }
}

public struct ReferenceInstanceSerilizable : IStructSerializable
{
    public string? Name;
    public bool? OutSide;
    public string? Path;

    public int GetSize() =>
        ((Name?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY + ((Path?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY + 1;

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Name = pBuffer.ReadArray<char>(ref offset).CrString();
        OutSide = pBuffer.Read<bool>(ref offset);
        Path = pBuffer.ReadArray<char>(ref offset).CrString();
    }

    [TODO("Solucionar que pueda pasar un dato incorrecto")]
    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.WriteArray<char>(ref offset, Name?.ToCharArray() ?? "null".ToCharArray());
        pBuffer.Write<bool>(ref offset, OutSide ?? false); // TODO: Solucionar que pueda pasar un dato incorrecto.
        pBuffer.WriteArray<char>(ref offset, Path?.ToCharArray() ?? "null".ToCharArray());
    }


    public static explicit operator ReferenceInstanceSerilizable(ReferenceInstance pData)
    {
        return new ReferenceInstanceSerilizable
        {
            Name = pData.Name,
            OutSide = pData.OutSide,
            Path = pData.Path,
        };
    }
}