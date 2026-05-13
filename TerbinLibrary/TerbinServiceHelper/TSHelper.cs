using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.TerbinServiceHelper.Consoles;

namespace TerbinLibrary.TerbinServiceHelper;
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



public class TSHelper
{
    public static byte[] GetError(CodeInternalErrors pError)
    {
        return Serialineitor.Serialize((ushort)pError);
    }

    public static string CreateStringException(Exception pE, string pSite = "ExceptionError")
    {
        return Exception.CreateStringException(pE, pSite);
    }

    public static class Debug
    {
        public static void PrintHas(params object[] pObjs)
        {
            string allHas = "";
            for (int i = 0; i < pObjs.Length; i++)
            {
                allHas += pObjs[i].GetType() + ": ";
                allHas += pObjs[i].GetHashCode() + ((i < pObjs.Length) ? "; \n" : "");
            }
            Console.Log($"{allHas}");
        }
        public static void PrintAllHas(params object[] pObjs)
        {
            string allHas = string.Join("; \n", pObjs.Select(o => $"{o.GetType()}: {o.GetHashCode()}"));
            Console.Log($"{allHas}");
        }
        public static void PrintHas(object pObj)
        {
            Console.Log($"{pObj.GetType()}: {pObj.GetHashCode()}");
        }
    }
}