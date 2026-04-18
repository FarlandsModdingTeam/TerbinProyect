using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;

namespace TerbinService.Configuration;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */


public class ConfiguratonFarlands
{
    [TerbinExecutable((byte)CodeMethodsTerbinService.ChangeFarladsRute)]
    public static async Task<PacketRequest> ChangeRuteFarlands(Header pHead, MemoryStream pPLD)
    {


        throw new NotImplementedException("Ñe");
    }


    [TerbinExecutable((byte)CodeMethodsTerbinService.ChangeInstancesRute)]
    public static async Task<PacketRequest> ChangeRuteInstances(Header pHead, MemoryStream pPLD)
    {


        throw new NotImplementedException("Ñe");
    }
}
