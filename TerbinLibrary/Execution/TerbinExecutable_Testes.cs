using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Extension;
using TerbinLibrary.Memory;
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

public interface IExecutableAttribute
{

}

public interface IExecutableDispatcher
{
    // public void Register(byte pEntity, ExecutableHandler pHandler);
    // public bool Unregister(byte pEntity);
}


// TODO: AllowMultiple in true.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinExecutableAttribute : Attribute, IExecutableAttribute
{
    public byte Action { get; } // CodeAction

    // TODO: guardar la lista de types de la funcion.
    // TODO: funcion estatica que serialice/deserialice a los tipos y cantidades guardadas.

    public TerbinExecutableAttribute(byte pAction) => Action = pAction;
}


[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinCRUDAttribute : Attribute, IExecutableAttribute
{
    /// <summary>
    /// Create, Read, Update, Deleted
    /// </summary>
    public CodeTerbinProtocol Action { get; }
    public byte Entity { get; }

    public TerbinCRUDAttribute(CodeTerbinProtocol pAction, byte pEntity)
    {
        Action = pAction;
        Entity = pEntity;
    }
}
public sealed class TerbinExecutableDispatcher_Testing : IExecutableDispatcher
{
    private readonly ConcurrentDictionary<byte, ExecutableHandler> _handlers = new();

    public byte Action { get; }

    public TerbinExecutableDispatcher_Testing(byte pAction)
    {
        Action = pAction;
    }

    public void Register(byte pEntity, ExecutableHandler pHandler)
    {
        if (pHandler is null) throw new ArgumentNullException(nameof(pHandler));
        _handlers[pEntity] = pHandler;
    }

    public bool Unregister(byte pEntity) => _handlers.TryRemove(pEntity, out _);

    // TODO: ya tengo el pAction, como tal no hay que pasarlo, (areglar).
    public async Task<InfoResponse?> DispatchAsync(Header pHead, byte pAction, byte[] pPayload)
    {
        tryGetEntity(pPayload, out var entity, out var memo);

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
            Console.WriteLine($"[TerbinExecutableCRUDDispatcher>DispatchAsync] ExceptionError-> {e.Message}");
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ExecutionError);
        }
    }

    private static bool tryGetEntity(byte[] pPayload, out byte pEntity, out byte[] pMemory)
    {
        pEntity = pPayload[0];
        int bodyLength = pPayload.Length - 1;
        pMemory = new byte[bodyLength];
        if (bodyLength > 0)
            Array.Copy(pPayload, 1, pMemory, 0, bodyLength);
        return true;
    }
}


public sealed class ExecutableDispatcher : IExecutableDispatcher
{
    private static readonly ConcurrentDictionary<byte, ExecutableHandler> _handlers = new();


    public static void Register(byte pAction, ExecutableHandler pHandler)
    {
        if (pHandler == null) throw new ArgumentNullException(nameof(pHandler));
        _handlers[pAction] = pHandler;
    }

    public static bool Unregister(byte pAction) => _handlers.TryRemove(pAction, out _);


    public static async Task<InfoResponse?> DispatchAsync(PacketRequest pCapsule)
    {
        if (!_handlers.TryGetValue(pCapsule.ActionMethod, out var handler))
        {
            TerbinExecutableHelper.TryReleaseMemory(pCapsule.Head.IdMemory);
            return InfoResponse.Create(pCapsule.Head.IdRequest, CodeStatus.ActionNotFound);
        }

        if (TerbinExecutableHelper.TryGetMemoryStream(pCapsule, out var memo) is var r && r != TerbinErrorCode.None)
        {
            var error = (r == TerbinErrorCode.MemoryReleaseFailed) ? CodeStatus.ErrorReleaseMemory : CodeStatus.ErrorGetPaylaodMemory;
            return InfoResponse.Create(pCapsule.Head.IdRequest, error);
        }

        try
        {
            Console.WriteLine($"[DispatchAsync] id: {pCapsule.Head.IdMemory}, Order: {pCapsule.Head.OrderRequest}, L: {memo.Length}"); //Encoding.UTF8.GetString(memo)
            if (pCapsule.Head.Status == CodeStatus.CheckExecution)
                return InfoResponse.CreateSucces(pCapsule.Head.IdRequest);

            if (pCapsule.ActionMethod == (byte)CodeTerbinProtocol.Response)
            {
                _ = handler(pCapsule.Head, memo).ConfigureAwait(false);
                return null; // Por si alguien hace el bruto.
            }
            else
                return await handler(pCapsule.Head, memo).ConfigureAwait(false);

            //.ConfigureAwait(false); // Para no cortar ejecucion al intentar terminar.
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ExecutableDispatcher>DispatchAsync] ExceptionError->  {e.Message}");
            return InfoResponse.Create(pCapsule.Head.IdRequest, CodeStatus.ExecutionError);
        }
    }

    // TODO: separa de alguna manera al helper.
    public static void RegisterFromAssembly(Assembly pAssembly)
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (MethodInfo? method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var attrs = method.GetCustomAttributes<TerbinExecutableAttribute>(inherit: false);
                if (!attrs.Any()) continue;

                ParameterInfo[]? parameters = method.GetParameters();
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
            }
        }
    }
}
