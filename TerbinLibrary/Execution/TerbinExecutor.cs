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
    private static TerbinCommunicator _communicator;

    public static void Init(TerbinCommunicator pCommunicator)
    {
        RegisterAll();
        _communicator = pCommunicator;
    }

    // TODO: perdirle a Luis que sea mi tutor.

    public static void RegisterAll()
    {
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
        //ExecutableDispatcher.RegisterFromAssembly(pAssembly);
        TerbinExecutableCRUDManager.RegisterFromAssembly(Assembly.GetExecutingAssembly());
        //TerbinExecutableCRUDManager.RegisterFromAssembly(pAssembly);
    }

    public static async Task<InfoResponse?> Execution(PacketRequest pRequest)
    {
        var capR = await ExecutableDispatcher.DispatchAsync(pRequest);
        return capR;
    }


    //public void Prueba(byte[]? b = null, char[]? c = null)
    //{
    //    Prueba(c:['1', '2', '3']);
    //}

    [TerbinExecutable((byte)CodeTerbinProtocol.Load)]
    public static async Task<InfoResponse?> Load(Header pHead, byte[] pParameters)
    {
        if (pHead.IdRequest > 0)
        {
            TerbinMemory.Store(pHead.IdMemory, pHead.OrderRequest, pParameters);
        }

        return null;
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Solicit)]
    public static async Task<InfoResponse?> Solicit(Header pHead, byte[] pParameters)
    {
        if (pHead.IdMemory == (byte)CodeTerbinMemory.New)
        {
            byte id = TerbinMemory.GetStore();
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
        InfoResponse? r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Create, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Read)]
    public static async Task<InfoResponse?> Read(Header pHead, byte[] pParameters)
    {
        InfoResponse? r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Read, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Update)]
    public static async Task<InfoResponse?> Update(Header pHead, byte[] pParameters)
    {
        InfoResponse? r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Update, pParameters);
        return r;
    }

    [TerbinExecutable((byte)CodeTerbinProtocol.Deleted)]
    public static async Task<InfoResponse?> Deleted(Header pHead, byte[] pParameters)
    {
        InfoResponse? r = await TerbinExecutableCRUDManager.DispatchAsync(pHead, CodeTerbinProtocol.Deleted, pParameters);
        return r;
    }


    [TerbinExecutable((byte)CodeTerbinProtocol.Response)]
    public static async Task<InfoResponse?> Response(Header pHead, byte[] pParameters)
    {
        _communicator.GiveResponse(new PacketRequest(pHead: pHead, (byte)CodeTerbinProtocol.Response, pParameters));
        return null;
    }


    // ya ni me acuerdo para que era.
    [TerbinExecutable((byte)CodeTerbinProtocol.Cancel)]
    public static async Task<InfoResponse?> Cancel(Header pHead, byte[] pParameters)
    {

        throw new NotImplementedException("Ñe");
    }
}