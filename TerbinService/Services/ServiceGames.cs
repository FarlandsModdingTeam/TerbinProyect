using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Managers;

namespace TerbinService.Services;

internal static class ServiceGames
{
    [TerbinExecutable((byte)TerbinCRUD.Duplicate, (byte)CodeServicesSection.Game)]
    public static async Task<InfoResponse?> CloneGame(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string nameInstance = reader.ReadArray<char>().CrString();
        string dirGame = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        IProgress<TerbinInfoProgrss>? progress = null;
        if (useProgress)
        {
            long maxSize = (long)NodeUtil.CountContent(dirGame);
            MaxProgressDTO max = new(maxSize);
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin);
        }

        var result = await Manager.Games.CloneInInstance(dirGame, nameInstance, true, progress, pToken);
        if (result != InternalErrors.IsSucces)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize<ushort>((ushort)result));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    [TerbinExecutable((byte)TerbinCRUD.Deleted, (byte)CodeServicesSection.Game)]
    public static async Task<InfoResponse?> DeletedGame(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string nameInstance = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        string? dirGame = Manager.Instances.GetPathFolder(nameInstance);
        if (string.IsNullOrEmpty(dirGame))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.InstanceNotExist));

        IProgress<TerbinInfoProgrss>? progress = null;
        if (useProgress)
        {
            long maxSize = (long)NodeUtil.CountContent(dirGame);
            MaxProgressDTO max = new(maxSize);
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin);
        }

        var result = await Manager.Games.RemoveInInstance(nameInstance, progress, pToken);
        if (result != InternalErrors.IsSucces)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize<ushort>((ushort)result));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    [TerbinExecutable((byte)CodeServices.Execute, (byte)CodeServicesSection.Game)]
    public static async Task<InfoResponse?> ExecuteGame(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> buffer = pParameters;
        string nameInstance = buffer.ReadArray<char>().CrString();

        var result = await Manager.Games.RunInInstance(nameInstance);
        if (result != InternalErrors.IsSucces)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize<ushort>((ushort)result));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }
}
