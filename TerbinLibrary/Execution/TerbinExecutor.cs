using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;

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

    // TODO: perdirle a Luis que sea mi tutor.

    public static void RegisterInternal()
    {
        TerbinExecutableManager.RegisterFromAssembly(Assembly.GetExecutingAssembly());
        TerbinExecutableManagerCompound.RegisterFromAssembly(Assembly.GetExecutingAssembly());
    }
    public static void Register(Assembly pAssembly)
    {
        TerbinExecutableManager.RegisterFromAssembly(pAssembly);
        TerbinExecutableManagerCompound.RegisterFromAssembly(pAssembly);
    }

    //public static async Task<InfoResponse?> Execution(PacketRequest pRequest)
    //{
    //    var capR = await TerbinExecutableManager.DispatchAsync(pRequest);
    //    return capR;
    //}


    //public void Prueba(byte[]? b = null, char[]? c = null)
    //{
    //    Prueba(c:['1', '2', '3']);
    //}

    [TerbinExecutable((byte)CodeTerbinProtocol.Load)]
    public static async Task<InfoResponse?> Load(Header pHead, byte[] pParameters)
    {
        if (pHead.IdRequest > 0)
        {
            TerbinMemoryManager.Store(pHead.IdMemory, pHead.OrderRequest, pParameters);
        }

        return null;
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Solicit)]
    public static async Task<InfoResponse?> Solicit(Header pHead, byte[] pParameters)
    {
        if (pHead.IdMemory == (byte)CodeTerbinMemory.New)
        {
            byte id = TerbinMemoryManager.GetStore();
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
        if (!CompoundExecutableDispatcher.TryGetEntity(pParameters, out var entity, out var memo))
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetEntity);
        InfoResponse? r = await TerbinExecutableManagerCompound.DispatchAsync(pHead, (byte)CodeTerbinProtocol.Create, entity, memo);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Read)]
    public static async Task<InfoResponse?> Read(Header pHead, byte[] pParameters)
    {
        if (!CompoundExecutableDispatcher.TryGetEntity(pParameters, out var entity, out var memo))
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetEntity);
        InfoResponse? r = await TerbinExecutableManagerCompound.DispatchAsync(pHead, (byte)CodeTerbinProtocol.Read, entity, memo);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Update)]
    public static async Task<InfoResponse?> Update(Header pHead, byte[] pParameters)
    {
        if (!CompoundExecutableDispatcher.TryGetEntity(pParameters, out var entity, out var memo))
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetEntity);
        InfoResponse? r = await TerbinExecutableManagerCompound.DispatchAsync(pHead, (byte)CodeTerbinProtocol.Update, entity, memo);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Deleted)]
    public static async Task<InfoResponse?> Deleted(Header pHead, byte[] pParameters)
    {
        if (!CompoundExecutableDispatcher.TryGetEntity(pParameters, out var entity, out var memo))
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetEntity);
        InfoResponse? r = await TerbinExecutableManagerCompound.DispatchAsync(pHead, (byte)CodeTerbinProtocol.Deleted, entity, memo);
        return r;
    }







    [TerbinExecutable((byte)CodeTerbinProtocol.Response)]
    public static async Task<InfoResponse?> Response(Header pHead, byte[] pParameters)
    {
        _communicator?.GiveResponse(new PacketRequest(pHead: pHead, (byte)CodeTerbinProtocol.Response, pParameters));
        return null;
    }


    // ya ni me acuerdo para que era.
    [TerbinExecutable((byte)CodeTerbinProtocol.Cancel)]
    public static async Task<InfoResponse?> Cancel(Header pHead, byte[] pParameters)
    {

        throw new NotImplementedException("Ñe");
    }
}