using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TerbinLibrary.Instances;

namespace TerbinService.Instances;

public partial class HandleInstances
{





    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool InstallBepInEx(string eDest)
    {
        string url = LibraryInstances.URL_BepInEx;
        // return NetUtil.InstallZip(url, eDest); // devuelve Task<>
    
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool CreateManifest()
    {
        // TODO: Crear manifiesto de la instancia con todos los mods.

        throw new NotImplementedException();
    }
}
