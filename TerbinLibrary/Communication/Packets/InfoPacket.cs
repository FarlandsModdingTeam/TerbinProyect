using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Protocol;

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

[Obsolete]
public struct InfoPacket
{
    public ushort? IdRequest { get => field; set => field = value; }
    public byte? ActionMethod { get => field; set => field = value; }
    public byte[]? Payload { get => field; set => field = value; }
    public CodeStatus? Status { get => field; set => field = value; }
    public bool Recuperate { get => field; set => field = value; }

    public InfoPacket()
    {
        ActionMethod = null;
        Payload = null;
        IdRequest = null;
        Status = null;
        Recuperate = false;
    }
}