using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;

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


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaxProgress : IStructSerializable
{
    public long Max;

    public int GetSize() => 8;

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Max = pBuffer.Read<long>(ref offset);
    }

    public void WriteTo(Span<byte> pBuffer)
    {
        int offset = 0;
        pBuffer.Write<long>(ref offset, Max);
    }
}
