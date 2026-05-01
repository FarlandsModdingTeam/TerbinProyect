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
using TerbinLibrary.Useful;

namespace TerbinService;

public class BepInExService
{
    [TerbinExecutable((byte)CodeServices.InstallBepInEx)]
    public static async Task<InfoResponse?> IntallBepInEx(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        long? sizeBepInEx = await NetUtil.GetContentLength(TerbinURLs.BepInEx);
        if (sizeBepInEx is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize((ushort)CodeInternalErrors.BepInExNotConect));

        AmongInfoThreads info = Worker.CurrentConst.Value;

        string rute = Serialineitor.DeserializeArray<char>(pParameters).CrString();
        // Habra alguna forma de saber si es un direccion valida?

        if (!Directory.Exists(rute))
            Directory.CreateDirectory(rute);
        // TODO: Comprobar si BepInEx ya esta instalado.

        byte idMemory = 0;
        var rId = await info.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize((ushort)CodeInternalErrors.IdSoliciteError));

        idMemory = rId.Payload[0];
        _ = HandleInstallBepInExWithProgress(idMemory, rute);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = [idMemory, .. Serialineitor.Serialize<long>(sizeBepInEx.Value)],
        };
    }


    public static async Task HandleInstallBepInExWithProgress(byte pIdMemory, string pDir)
    {
        IProgress<TerbinInfoProgrss> progressBarr = new Progress<TerbinInfoProgrss>(p =>
        {
            var Content = p.ToArray();
            _ = Worker.CurrentConst.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemory, Content);
            Console.Write($"\rDescargando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");
        });
        StatusNetUtil? r = await HandleInstallBepInEx(TerbinURLs.BepInEx, progressBarr);
        if (r is null) throw new Exception("TODO: informar de que BepInEx ya esta instalado");
        if (r != StatusNetUtil.Succes)
        {
            CodeInternalErrors error = r switch
            {
                StatusNetUtil.ExceptionOnExtractZip => CodeInternalErrors.ZipExtractException,
                StatusNetUtil.ExceptionDeleteTemporalFile => CodeInternalErrors.ZipDeletedTempException,
                _ => CodeInternalErrors.ZipExtractError
            };
            throw new Exception($"TODO: informar de {error}");

            // Prototipo del funcionamiento de Info
            AmongInfoThreads info = Worker.CurrentConst.Value;
            byte[] pld = new Serialineitor()
                .Add(TypeService.Service)
                .Add(CodeServices.InstallBepInEx)
                .Add(error)
                .ToArray();
            _ = info.Communicator.Send((byte)CodeTerbinProtocol.Info, pld);
        }
    }

    public static async Task<StatusNetUtil?> HandleInstallBepInEx(string pDir, IProgress<TerbinInfoProgrss>? pProgress = default)
    {
        StatusNetUtil r = StatusNetUtil.Succes;
        if (CheckInstallBepInEx(pDir)) return null;
        r = await NetUtil.InstallZip(TerbinURLs.BepInEx, pDir, pProgress);
        return r;
    }

    public static bool CheckInstallBepInEx(string pDir)
    {
        string bep = Path.Combine(pDir, "BepInEx");
        return Directory.Exists(bep);
    }
}