using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary.Communication.Packets;
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
public struct Header // la memoria es constante es unmanaged.
{
    public ushort IdRequest;
    public ushort OrderRequest; // 0 = solo uno, 1 = es el primero, ushort.MaxValue = es el ultimo.
    public CodeStatus Status;
    public byte IdMemory;

    public Header()
    {
        IdRequest = 0;
        OrderRequest = 0;
        Status = CodeStatus.NotAsign;
        IdMemory = (byte)CodeTerbinMemory.Undefined;
    }

    public Header(
        ushort pIdRequest = 0,
        ushort pOrderRequest = TerbinProtocol.ORDER_SINGLE,
        CodeStatus pStatus = CodeStatus.NotAsign,
        byte pIdMemory = (byte)CodeTerbinMemory.Undefined)
    {
        IdRequest = (pIdRequest == 0) ? (ushort)1 : pIdRequest;
        OrderRequest = pOrderRequest;
        Status = pStatus;
        IdMemory = pIdMemory;
    }
}