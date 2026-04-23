using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;

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
/// Manager principal unificado que enruta tanto acciones simples como acciones compuestas (CRUD).
/// </summary>
public static class TerbinExecutableManager : IExecutableDispatcher
{
    //private static readonly ConcurrentDictionary<byte, IExecutableDispatcher> _dispatchers = new();
    private static readonly ConcurrentBag<IExecutableDispatcher> _dispatchers = new();

    public static void RegisterSingle(byte pAction, TerbinExecutableDelegate pHandler)
    {
        _dispatchers[pAction] = new SingleExecutableDispatcher(pHandler);
    }

    public static void RegisterSubAction(byte pAction, byte pSubAction, TerbinExecutableDelegate pHandler)
    {
        if (!_dispatchers.TryGetValue(pAction, out var dispatcher) || dispatcher is not CompoundExecutableDispatcher subActionDispatcher)
        {
            subActionDispatcher = new CompoundExecutableDispatcher();
            _dispatchers[pAction] = subActionDispatcher;
        }

        subActionDispatcher.Register(pSubAction, pHandler);
    }

    public static bool Unregister(byte pAction) => _dispatchers.TryRemove(pAction, out _);

    public static async Task<InfoResponse?> DispatchAsync(PacketRequest pCapsule)
    {
        if (!_dispatchers.TryGetValue(pCapsule.ActionMethod, out var dispatcher))
        {
            TerbinExecutableHelper.TryReleaseMemory(pCapsule.Head.IdMemory);
            return InfoResponse.Create(pCapsule.Head.IdRequest, CodeStatus.ActionNotFound);
        }

        if (TerbinExecutableHelper.TryGetMemoryStream(pCapsule, out var memo) is var r && r != TerbinErrorCode.None)
        {
            var error = (r == TerbinErrorCode.MemoryReleaseFailed) ? CodeStatus.ErrorReleaseMemory : CodeStatus.ErrorGetPaylaodMemory;
            return InfoResponse.Create(pCapsule.Head.IdRequest, error);
        }

        if (pCapsule.Head.Status == CodeStatus.CheckExecution)
            return InfoResponse.CreateSucces(pCapsule.Head.IdRequest);

        if (pCapsule.ActionMethod == (byte)CodeTerbinProtocol.Response)
        {
            _ = dispatcher.DispatchAsync(pCapsule.Head, memo).ConfigureAwait(false);
            return null;
        }
        else
        {
            return await dispatcher.DispatchAsync(pCapsule.Head, memo).ConfigureAwait(false);
        }
    }

    public static void RegisterFromAssembly(Assembly pAssembly)
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var attrs = method.GetCustomAttributes(inherit: false);
                if (attrs.Length <= 0) continue;

                var parameters = method.GetParameters();
                if (!TerbinExecutableHelper.IsFirmParameters(parameters))
                    continue;

                if (!TerbinExecutableHelper.IsFirmReturn(method))
                    continue;
               

                // Crea delegate fuertemente tipado (evita reflexión por llamada)
                var del = (Func<Header, byte[], Task<InfoResponse?>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], Task<InfoResponse?>>), method);

                foreach (var attr in attrs)
                {
                    Register(attr.Action, (h, b) => del(h, b));
                }
                
                /*
                var del = (TerbinExecutableHandler)Delegate.CreateDelegate(typeof(TerbinExecutableHandler), method);

                foreach (var attr in attrs)
                {
                    if (attr is TerbinExecutableAttribute simpleAttr)
                    {
                        RegisterSingle(simpleAttr.Action, del);
                    }
                    else if (attr is TerbinExecutableCompoundAttribute crudAttr)
                    {
                        RegisterSubAction(crudAttr.Action, crudAttr.Entity, del);
                    }
                }*/
            }
        }
    }
}
