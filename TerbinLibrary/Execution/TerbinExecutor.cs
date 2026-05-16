using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Memory;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Execution;

// TODO: (Verano) Mover a NO estatico para permitir empalmes.
public static class TerbinExecutor
{
    private static TerbinCommunicator? _communicator;

    public static void Init(TerbinCommunicator pCommunicator)
    {
        RegisterInternal();
        _communicator = pCommunicator;
    }

    public static void RegisterInternal()
    {
        //TerbinExecutableManagerSimple.RegisterFromAssembly(Assembly.GetExecutingAssembly());
        TerbinExecutableManager.RegisterFromAssembly(Assembly.GetExecutingAssembly());
    }
    public static void Register(Assembly pAssembly)
    {
        //TerbinExecutableManagerSimple.RegisterFromAssembly(pAssembly);
        TerbinExecutableManager.RegisterFromAssembly(pAssembly);
    }

    //public static async Task<InfoResponse?> Execution(PacketRequest pRequest)
    //{
    //    var capR = await TerbinExecutableManager.DispatchAsync(pRequest);
    //    return capR;
    //}

    [TerbinExecutable((byte)CodeTerbinProtocol.Load)]
    public static async Task<InfoResponse?> Load(Header pHead, byte[] pParameters)
    {
        if (pHead.OrderRequest > 0)
        {
            TerbinMemoryManager.Store(pHead.IdMemory, pHead.OrderRequest, pParameters);
        }
        else if (pHead.OrderRequest == 0)
        {
            TerbinMemoryManager.OverwriteStore(pHead.IdMemory, 1, pParameters);
        }

        return null;
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Solicit)]
    public static async Task<InfoResponse?> Solicit(Header pHead, byte[] pParameters)
    {
        if (pHead.IdMemory == (byte)CodeTerbinMemory.New)
        {
            byte id = TerbinMemoryManager.GetFreeStore();
            return new InfoResponse
            {
                Status = CodeStatus.Succes,
                IdRequest = pHead.IdRequest,
                Payload = [id]
            };
        }

        return null;
    }



    [TerbinExecutable((byte)CodeTerbinProtocol.Create)]
    public static async Task<InfoResponse?> Create(Header pHead, byte[] pParameters)
    {
        if (!ExecutableDispatcher.TryGetEntity(pParameters, out var entity, out var memo))
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetEntity);
        InfoResponse? r = await TerbinExecutableManager.DispatchAsync(pHead, memo, (byte)CodeTerbinProtocol.Create, entity);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Read)]
    public static async Task<InfoResponse?> Read(Header pHead, byte[] pParameters)
    {
        if (!ExecutableDispatcher.TryGetEntity(pParameters, out var entity, out var memo))
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetEntity);
        InfoResponse? r = await TerbinExecutableManager.DispatchAsync(pHead, memo, (byte)CodeTerbinProtocol.Read, entity);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Update)]
    public static async Task<InfoResponse?> Update(Header pHead, byte[] pParameters)
    {
        if (!ExecutableDispatcher.TryGetEntity(pParameters, out var entity, out var memo))
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetEntity);
        InfoResponse? r = await TerbinExecutableManager.DispatchAsync(pHead, memo, (byte)CodeTerbinProtocol.Update, entity);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Deleted)]
    public static async Task<InfoResponse?> Deleted(Header pHead, byte[] pParameters)
    {
        if (!ExecutableDispatcher.TryGetEntity(pParameters, out var entity, out var memo))
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetEntity);
        InfoResponse? r = await TerbinExecutableManager.DispatchAsync(pHead, memo, (byte)CodeTerbinProtocol.Deleted, entity);
        return r;
    }




    [TerbinExecutable((byte)CodeTerbinProtocol.Prolong)]
    public static async Task<InfoResponse?> Prolong(Header pHead, byte[] pParameters)
    {
        ushort id = Serialineitor.Deserialize<ushort>(pParameters);
        _communicator?.GiveProlong(id);
        return null;
    }



    [TerbinExecutable((byte)CodeTerbinProtocol.Response)]
    public static async Task<InfoResponse?> Response(Header pHead, byte[] pParameters)
    {
        _communicator?.GiveResponse(new PacketRequest(pHead: pHead, [(byte)CodeTerbinProtocol.Response], pParameters));
        return null;
    }
}