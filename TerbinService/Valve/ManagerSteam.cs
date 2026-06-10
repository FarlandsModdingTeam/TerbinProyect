using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using TerbinService.Managers;

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


public static class Steam
{

    public static bool IsOpenSteam
    {
        get => Process.GetProcessesByName("steam").Length > 0 ||
               Process.GetProcessesByName("steamwebhelper").Length > 0;
    }


    /*
    public static Status LaunchFarlandsByPotron()
    {
        var potronPath = GetPotronPath();
        if (potronPath == null)
            return Status.NotInstaled;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = potronPath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(potronPath)
            });
            return Status.Succes;
        }
        catch
        {
            return Status.NotInstaled;
        }
    }
    */

}
