using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;

namespace TerbinService.Instances;

public partial class HandleInstances
{
    [TerbinExecutable(CodeAction.CreateInstance)]
    public static async Task<PacketRequest> Create(Header pHead, MemoryStream pParameters)
    {
        Console.WriteLine("[Worker] Pruebas...");
        Console.WriteLine($"[Worker] Cliente: {pHead.IdRequest:N}");

        pHead.Status = CodeStatus.Succes;
        return new PacketRequest
        {
            Head = pHead,
            ActionMethod = CodeAction.CreateInstance,
            //Parameters = ""// Convert.ToBase64String(Convert.FromBase64String("f")) // pruebas
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
