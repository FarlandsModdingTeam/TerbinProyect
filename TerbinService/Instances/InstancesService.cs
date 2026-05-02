using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinService.Instances;

// Crear, Listar, Run, Abrir, Eliminar, AñadirMod.
public partial class InstancesService
{



    public static bool IsInstance(string pDir)
    {
        string information;
        string manifest;

        information = Path.Combine(pDir, TerbinServiceConst.FOLDER_INFORMATION_INSTANCE);

        if (!Directory.Exists(information)) return false;

        manifest = Path.Combine(information, TerbinServiceConst.NAME_OF_MANIFEST);

        return File.Exists(manifest);
    }
}
