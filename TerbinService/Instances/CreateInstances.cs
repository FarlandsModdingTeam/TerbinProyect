using System;
using System.Collections.Generic;
using TerbinLibrary.Execution;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Serialize;

namespace TerbinService.Instances;

public partial class InstancesService
{
    // El nombre de la instancia, bool si quieres instalar BepInEx, 
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Instances)]
    public static async Task<InfoResponse?> CreateInstance(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        // TODO: De carpeta de Farlands.
        // ├─Coger Dir Carpeta.
        // └─Comprobar si esta el ejecutable <todo: mover dentro del metodo>.

        // TODO: Del nombre carpeta destino.
        // └─Comprobar si la carpeta esta vacia.

        // HandleCreateInstance(); // Tengo sueño.
        // HandleCloneFarlands();

        // TODO: Instalar BepInEx si el entrante es que si.

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    public static async Task HandleCreateInstance()
    {
        // TODO: De la carpeta destino.
        // ├─Comprobar si esta vacia.
        // └─Comprobar si ya hay una instancia.
    }
    public static async Task HandleCloneFarlands()
    {
        // TODO: Clonar Farlands.
        // ├─Crear Manifest.
        // └─Guardar en Config la dir de la instancia.
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
