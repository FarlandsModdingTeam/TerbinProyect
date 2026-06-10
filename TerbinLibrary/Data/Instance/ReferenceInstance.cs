using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Data.Instance;
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


public class ReferenceInstance : IToDTO<ReferenceInstanceDTO>, IToSerializeDTO
{
    public string? Name;
    public bool? OutSide;
    public string? Path;

    public ReferenceInstanceDTO ToDTO()
    {
        return (ReferenceInstanceDTO)this;
    }

    public byte[] ToSerilizeDTO()
    {
        return ((ReferenceInstanceDTO)this).Serialize();
    }

    public static explicit operator ReferenceInstance(ReferenceInstanceDTO pData)
    {
        return new ReferenceInstance
        {
            Name = pData.Name,
            OutSide = pData.OutSide,
            Path = pData.Path,
        };
    }
}
