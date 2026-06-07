using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Data.Transport;

public struct ManifestPluginDTO : IStructSerializable
{
    // Nombre del mod
    public string? Name { get; set; }
    // Id en el Storage
    public string? Id { get; set; }
    // Id generado al instalar
    public string? IdLocal { get; set; }
    // Si fue instalado fuera de la instancia y por tanto las rutas no son relativas.
    public bool? OutSideIntance { get; set; }

    public readonly int GetSize() =>
        ((Name?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY +
        ((Id?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY +
        ((IdLocal?.Length ?? 0) * 2) + TerbinProtocol.LENGTH_ARRAY +
        1;

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Name = pBuffer.ReadArray<char>(ref offset).CrString();
        Id = pBuffer.ReadArray<char>(ref offset).CrString();
        IdLocal = pBuffer.ReadArray<char>(ref offset).CrString();
        OutSideIntance = pBuffer.Read<sbyte>(ref offset).ToBoolUk();
    }

    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.WriteArray<char>(ref offset, Name?.ToCharArray() ?? "".ToCharArray());
        pBuffer.WriteArray<char>(ref offset, Id?.ToCharArray() ?? "".ToCharArray());
        pBuffer.WriteArray<char>(ref offset, IdLocal?.ToCharArray() ?? "".ToCharArray());
        pBuffer.Write<sbyte>(ref offset, OutSideIntance.ToSByte());
    }
}
