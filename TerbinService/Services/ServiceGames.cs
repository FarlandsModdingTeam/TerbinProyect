using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution;
using TerbinLibrary.Serialize;
using TerbinLibrary.Extension;
using TerbinService.Managers;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.Protocol;

namespace TerbinService.Services;

internal static class ServiceGames
{
    [TerbinExecutable((byte)TerbinCRUD.Duplicate, (byte)CodeServicesSection.Game)]
    public static async Task<InfoResponse?> CloneGame(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> buffer = pParameters;
        string nameInstance = buffer.ReadArray<char>().CrString();
        string dirGame = buffer.ReadArray<char>().CrString();

        var sizes = Manager.Node.GetSizeDir(dirGame);
        if (sizes.maxFiles == null || sizes.maxDir == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceGetSizeError));

        var rId = await Worker.CurrentContext.Value.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        byte id = rId.Payload[0];

        //_ = Manager.Games.HandleCloneInInstanceWithProgress(nameInstance, id, dirGame);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = new Serialineitor()
                        .Add(id)
                        .Add(sizes.maxFiles.Value)
                        .Add(sizes.maxDir.Value)
                        .Serialize(),
        };
    }


    [TerbinExecutable((byte)CodeServices.Execute, (byte)CodeSubServices.Game)]
    public static async Task<InfoResponse?> ExecuteGame(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> buffer = pParameters;
        string nameInstance = buffer.ReadArray<char>().CrString();
        string dirGame = buffer.ReadArray<char>().CrString();

        var sizes = Manager.Node.GetSizeDir(dirGame);
        if (sizes.maxFiles == null || sizes.maxDir == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceGetSizeError));

        var rId = await Worker.CurrentContext.Value.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        byte id = rId.Payload[0];

        //_ = Manager.Games.HandleCloneInInstanceWithProgress(nameInstance, id, dirGame);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = new Serialineitor()
                        .Add(id)
                        .Add(sizes.maxFiles.Value)
                        .Add(sizes.maxDir.Value)
                        .Serialize(),
        };
    }
}
