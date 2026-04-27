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
    /*
    [TerbinExecutable((byte)CodeService.CreateInstance)]
    public static async Task<InfoResponse?> Create(Header pHead, byte[] pParameters)
    {
        Console.WriteLine($"[Worker] IdRequest: {pHead.IdRequest}");
        Console.WriteLine($"[Worker] Largo: {pParameters.Length}");
        if (pParameters.Length > 0)
        {
            char[] falseString = Serialineitor.DeserializeArray<char>(pParameters.ToArray());
            Console.WriteLine($"[Worker] Mensaje: {new string(falseString)}");
        }
        else
        {
            Console.WriteLine("[Worker] Mensaje: (Sin payload)");
        }

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }
    */


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
