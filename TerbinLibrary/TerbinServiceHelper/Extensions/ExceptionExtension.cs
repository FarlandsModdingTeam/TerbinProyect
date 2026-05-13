using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.TerbinServiceHelper.Extensions;
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



public static class ExceptionExtension
{
    extension(Exception)
    {
        public static string CreateStringException(Exception pE, string pSite = "ExceptionError")
        {
            return
            $$"""
            [{{pSite}}] =>
            {
                Message: {{pE.Message}};
                Source: {{pE.Source ?? "N/A"}};
                Inner: {{pE.InnerException?.Message ?? "N/A"}};
                Trace: {{pE.StackTrace ?? "N/A"}};
                String: {{pE.ToString()}}
            }
            """;
        }
    }

    public static string CrString(this Exception pE, string pSite = "ExceptionError")
    {
        return Exception.CreateStringException(pE, pSite);
    }
    public static void PrintException(this Exception pE, string pSite = "ExceptionError")
    {
        string e = Exception.CreateStringException(pE, pSite);
        Console.Error(e);
    }
}