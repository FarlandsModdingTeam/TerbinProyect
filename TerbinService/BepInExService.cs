using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.IO;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinService.Useful;

namespace TerbinService;

public class BepInExService
{
    [TerbinExecutable((byte)CodeService.InstallBepInEx)]
    public static async Task<InfoResponse?> IntallBepInEx(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        long? sizeBepInEx = await NetUtil.GetContentLength(TerbinURLs.BepInEx);
        if (sizeBepInEx is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize((ushort)CodeInternalErrors.NotConectToBepInEx));

        AmongInfoThreads info = Worker.CurrentConst.Value;


        string rute = Serialineitor.DeserializeArray<char>(pParameters).CrString();
        // Habra alguna forma de saber si es un direccion valida?

        if (!Directory.Exists(rute))
            Directory.CreateDirectory(rute);
        // TODO: Comprobar si BepInEx ya esta instalado.

        byte idMemory = 0;
        var rId = await info.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize((ushort)CodeInternalErrors.ErrorSoliciteId));

        idMemory = rId.Payload[0];
        _ = handleInstallBepInEx(idMemory, rute);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = [idMemory, .. Serialineitor.Serialize<long>(sizeBepInEx.Value)],
        };
    }


    private static async Task handleInstallBepInEx(byte pIdMemory, string pDir)
    {
        IProgress<TerbinInfoProgrss> progressBarr = new Progress<TerbinInfoProgrss>(p =>
        {
            AmongInfoThreads info = Worker.CurrentConst.Value;

            var Content = p.ToArray();
            _ = info.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemory, Content);

            // TODO: comprobar si es el ultimo, si lo es mandar mensaje de release.

            Console.Write($"\rDescargando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");

            //Console.Write($"\rDescargando... {Math.Round((float)p[0], 2)}% completado|");
        });
        StatusNetUtil r = StatusNetUtil.Succes;
        try
        {
            r = await NetUtil.InstallZip(TerbinURLs.BepInEx, pDir, progressBarr);
            //var result = await NetUtil.DownloadAny(TerbinURLs.BepInEx, progressBarr); // pDir, 
            //r = result.status;
            //Console.WriteLine($"Archivo => {result.tempFilePath}|");
        }
        catch (Exception e)
        {
            Console.WriteLine($"exception: {e.Message}|");
        }
        Console.WriteLine($"Result: {r}|");
        if (r != StatusNetUtil.Succes)
        {
            // TODO: Informar del error al cliente por el communicator que no tengo
        }
    }


    [TerbinExecutable(255)]
    public static async Task<InfoResponse?> TestDowload(Header pHead, byte[] pParameters)
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

}
