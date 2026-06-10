using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TerbinService.Valve;
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


public static class Proton
{
    private static string _rute_proton = "";

    public static bool LauncheGame(string pPath)
    {
        var p = Process.Start(new ProcessStartInfo
        {
            FileName = _rute_proton,
            ArgumentList =
            {
                "run",
                pPath
            }
        });

        return true;
    }

    public static bool FindProton(out string pPathProton)
    {
        pPathProton = "";



        return true;
    }
}
