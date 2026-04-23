using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using TerbinLibrary.Communication;
using TerbinLibrary.Memory;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Execution;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayorculas = publica.
  empieza: menorculas = privada.
 */

/// <summary>
/// 
/// </summary>
public sealed class SingleExecutableDispatcher : IExecutableDispatcher
{
    private readonly ConcurrentDictionary<byte, TerbinExecutableDelegate> _handlers = new();

    public SingleExecutableDispatcher(TerbinExecutableDelegate pHandler)
    {
        _handler = pHandler ?? throw new ArgumentNullException(nameof(pHandler));
    }

    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload)
    {
        try
        {
            return await _handler(pHead, pPayload).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SingleExecutableDispatcher>DispatchAsync] ExceptionError-> {e.Message}");
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionError);
        }
    }
}

/// <summary>
/// 
/// </summary>
public sealed class CompoundExecutableDispatcher : IExecutableDispatcher
{
    private readonly ConcurrentDictionary<byte, TerbinExecutableDelegate> _handlers = new();

    public void Register(byte pSubAction, TerbinExecutableDelegate pHandler)
    {
        if (pHandler is null) throw new ArgumentNullException(nameof(pHandler));
        _handlers[pSubAction] = pHandler;
    }

    public bool Unregister(byte pSubAction) => _handlers.TryRemove(pSubAction, out _);

    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload)
    {
        if (!tryGetEntity(pPayload, out var entity, out var memo))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorGetPaylaodMemory);
        }

        if (!_handlers.TryGetValue(entity, out var handler))
        {
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.SubActionNotFound);
        }

        try
        {
            return await handler(pHead, memo).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SubActionExecutableDispatcher>DispatchAsync] ExceptionError-> {e.Message}");
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionError);
        }
    }

    private static bool tryGetEntity(byte[] pPayload, out byte pEntity, out byte[] pMemory)
    {
        if (pPayload == null || pPayload.Length == 0)
        {
            pEntity = 0;
            pMemory = Array.Empty<byte>();
            return false;
        }

        pEntity = pPayload[0];
        int bodyLength = pPayload.Length - 1;
        pMemory = new byte[bodyLength];

        if (bodyLength > 0)
            Array.Copy(pPayload, 1, pMemory, 0, bodyLength);

        return true;
    }
}

