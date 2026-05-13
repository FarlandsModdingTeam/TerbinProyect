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
using static TerbinService.Managers.ManagerGames;

namespace TerbinService.Services;

internal static class ServiceGames
{
    [TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Game)]
    public static async Task<InfoResponse?> CloneGame(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> buffer = pParameters;
        string nameInstance = buffer.ReadArray<char>().CrString();
        string dirGame = buffer.ReadArray<char>().CrString();

        var sizes = ManagerNode.GetSizeDir(dirGame);
        if (sizes.maxFiles == null || sizes.maxDir == null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.InstaceGetSizeError));

        var rId = await Worker.CurrentConst.Value.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        byte id = rId.Payload[0];

        _ = HandleCloneInInstanceWithProgress(nameInstance, id, dirGame);

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
