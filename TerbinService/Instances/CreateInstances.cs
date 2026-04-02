using System;
using System.Collections.Generic;
using TerbinLibrary.Executables;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Serialize;

namespace TerbinService.Instances;

public partial class HandleInstances
{
    [TerbinExecutable((byte)CodeAction.CreateInstance)]
    public static async Task<PacketRequest> Create(Header pHead, MemoryStream pParameters)
    {
        Console.WriteLine($"[Worker] Cliente: {pHead.IdClient}");
        if (pParameters.Length > 0)
        {
            char[] falseString = Serialineitor.DeserializeArray<char>(pParameters.ToArray());
            Console.WriteLine($"[Worker] Mensaje: {new string(falseString)}");
        }
        else
        {
            Console.WriteLine("[Worker] Mensaje: (Sin payload)");
        }

        // Me acuerdo que con MemoryStream podias elegir que cosa leer o algo así.
        // no me acuerdo porque elegi MemoryStream para leer datos.
        // TODO: Acordarme.

        pHead.Status = CodeStatus.Succes;
        return new PacketRequest
        {
            Head = pHead,
            ActionMethod = (byte)CodeAction.CreateInstance,
        };
    }



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool InstallBepInEx(string eDest)
    {
        string url = TerbinURLs.BepInEx;
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
