using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Data.References;
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


public class ReferenceInstance 
{
    public string? Name;
    public bool? OutSide;
    public string? Path;

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
