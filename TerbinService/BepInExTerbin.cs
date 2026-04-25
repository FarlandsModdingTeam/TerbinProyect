using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinService.Useful;

namespace TerbinService;

public class BepInExTerbin
{
    [TerbinExecutable((byte)CodeService.InstallBepInEx)]
    public static async Task<InfoResponse?> IntallBepInEx(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        long? sizeBepInEx = await NetUtil.GetContentLength(TerbinURLs.BepInEx);
        if (sizeBepInEx is null)
            return new InfoResponse
            {
                IdRequest = pHead.IdRequest,
                Status = CodeStatus.InternalWorkerError,
                Payload = Serialineitor.Serialize<ushort>((ushort)CodeInternalErrors.NotConectToBepInEx),

            };

        string rute = Serialineitor.DeserializeArray<char>(pParameters).CrString();
        // Habra alguna forma de saber si es un direccion valida?
        if (!Directory.Exists(rute))
            Directory.CreateDirectory(rute);

        // TODO: Solicitar IdMemory :)
        byte idPlaceHolder = 0;

        _ = handleInstallBepInEx(idPlaceHolder, rute);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = [idPlaceHolder, .. Serialineitor.Serialize(sizeBepInEx.Value)],
        };
    }


    private static async Task handleInstallBepInEx(byte pIdMemory, string pDir)
    {
        IProgress<byte[]> progressBarr = new Progress<byte[]>(p =>
        {
            // TODO: Mandar al cliente por el communicator que no tengo XD
            Console.Write($"\rDescargando... {Math.Round((float)p[0], 2)}% completado");
        });
        StatusNetUtil r = StatusNetUtil.Succes;
        try
        {
            r = await NetUtil.InstallZip(TerbinURLs.BepInEx, pDir, progressBarr);
        }
        catch (Exception e)
        {
            Console.Write($"exception: {e.Message}");
        }
        Console.Write($"Result: {r}");
        if (r != StatusNetUtil.Succes)
        {
            // TODO: Informar del error al cliente por el communicator que no tengo
        }
    }
}
