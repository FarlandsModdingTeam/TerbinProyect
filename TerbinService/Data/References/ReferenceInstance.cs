using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinService.Data.References;

public class ReferenceInstance : IStructSerializable
{
    public string? Name;
    public bool? OutSide;
    public string? Path;

    public ushort GetSize() =>
        (ushort)(((Name?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY + ((Path?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY + 1);

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Name = pBuffer.ReadArray<char>(ref offset).CrString();
        OutSide = pBuffer.Read<bool>(ref offset);
        Path = pBuffer.ReadArray<char>(ref offset).CrString();
    }

    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.WriteArray<char>(ref offset, Name?.ToCharArray() ?? "null".ToCharArray());
        pBuffer.Write<bool>(ref offset, OutSide ?? false); // TODO: Solucionar que pueda pasar un dato incorrecto.
        pBuffer.WriteArray<char>(ref offset, Path?.ToCharArray() ?? "null".ToCharArray());
    }
}
