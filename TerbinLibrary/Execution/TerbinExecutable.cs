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

// TODO: AllowMultiple in true.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinExecutableAttribute : Attribute
{
    public byte Action { get; } // CodeAction

    // TODO: guardar la lista de types de la funcion.
    // TODO: funcion estatica que serialice/deserialice a los tipos y cantidades guardadas.

    public TerbinExecutableAttribute(byte pAction) => Action = pAction;
}


public sealed class ExecutableDispatcher
{
    private static readonly ConcurrentDictionary<byte, ExecutableHandler> _handlers = new();


    public static void Register(byte pAction, ExecutableHandler pHandler)
    {
        if (pHandler == null) throw new ArgumentNullException(nameof(pHandler));
        _handlers[pAction] = pHandler;
    }

    public static bool Unregister(byte pAction) => _handlers.TryRemove(pAction, out _);


    public static async Task<PacketRequest?> DispatchAsync(PacketRequest pCapsule) // Aqui es donde se ejecuta, manda cojones.
    {
        if (!_handlers.TryGetValue(pCapsule.ActionMethod, out var handler))
        {
            pCapsule.Head.Status = CodeStatus.ActionNotFound;
            TerbinExecutableHelper.TryReleaseMemory(pCapsule.Head.IdMemory);
            pCapsule.ClearPLD();
            return pCapsule;
        }

        // TODO: Controlar liberar memoria despues de ejecutar.
        if (TerbinExecutableHelper.TryGetMemoryStream(pCapsule, out var memo) is var r && r != TerbinErrorCode.None)
        {
            pCapsule.Head.Status = CodeStatus.ErrorGetPaylaodMemory;
            pCapsule.ClearPLD();
            return pCapsule;
        }
        
        try
        {
            Console.WriteLine($"[DispatchAsync] id: {pCapsule.Head.IdMemory}, Order: {pCapsule.Head.OrderRequest}, L: {memo.Length}"); //Encoding.UTF8.GetString(memo)
            return await handler(pCapsule.Head, memo)
                .ConfigureAwait(false); // Para no cortar ejecucion al intentar terminar.
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ExecutableDispatcher>DispatchAsync] ExceptionError->  {e.Message}");
            pCapsule.Head.Status = CodeStatus.ExecutionError;
            return pCapsule;
        }
    }

    [Obsolete]
    private static TerbinErrorCode tryGetMemoryStream(PacketRequest pCapsule, out byte[] pMemory)
    {
        var error = TerbinExecutableHelper.TryGetMemoryStream(pCapsule, out pMemory);
        if (error != TerbinErrorCode.None)
            TerbinExecutableHelper.TryReleaseMemory(pCapsule.Head.IdMemory);
        return error;
    }

    // Escanea métodos estáticos con [TerbinCommand(1)] y firma Task<Capsule>(Header, byte[])
    // TODO: separa de alguna manera al helper.
    public static void RegisterFromAssembly(Assembly pAssembly)
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (MethodInfo? method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var attr = method.GetCustomAttribute<TerbinExecutableAttribute>(inherit: false);
                if (attr == null) continue;

                ParameterInfo[]? parameters = method.GetParameters();
                if (!TerbinExecutableHelper.IsFirmParameters(parameters))
                    continue;

                if (!TerbinExecutableHelper.IsFirmReturn(method))
                    continue;

                // Crea delegate fuertemente tipado (evita reflexión por llamada)
                var del = (Func<Header, byte[], Task<PacketRequest?>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], Task<PacketRequest?>>), method);

                Register(attr.Action, (h, b) => del(h, b));
            }
        }
    }
}
