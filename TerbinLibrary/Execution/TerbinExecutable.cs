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


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TerbinExecutableAttribute : Attribute
{
    public byte Action { get; } // CodeAction

    // TODO: guardar la lista de types de la funcion.
    // TODO: funcion estatica que serialice/deserialice a los tipos y cantidades guardadas.

    public TerbinExecutableAttribute(byte pAction) => Action = pAction;
}

// NOTA: ¿Para que sirve esto?.
public delegate Task<PacketRequest> ExecutableHandler(Header pHead, MemoryStream pParameters);


public sealed class ExecutableDispatcher
{
    private static readonly ConcurrentDictionary<byte, ExecutableHandler> _handlers = new();


    public static void Register(byte pAction, ExecutableHandler pHandler)
    {
        if (pHandler == null) throw new ArgumentNullException(nameof(pHandler));
        _handlers[pAction] = pHandler;
    }

    public static bool Unregister(byte pAction) => _handlers.TryRemove(pAction, out _);


    public static async Task<PacketRequest> DispatchAsync(PacketRequest pCapsule) // Aqui es donde se ejecuta, manda cojones.
    {
        if (!_handlers.TryGetValue(pCapsule.ActionMethod, out var handler))
        {
            pCapsule.Head.Status = CodeStatus.ActionNotFound;
            return pCapsule;
        }

        try
        {
            return await handler(pCapsule.Head, new MemoryStream(pCapsule.Payload ?? []))
                .ConfigureAwait(false); // Para no cortar ejecucion al intentar terminar.
        }
        catch
        {
            pCapsule.Head.Status = CodeStatus.GenericWorkerError;
            return pCapsule;
        }
    }

    public static void RegisterAll()
    {
        ExecutableDispatcher.RegisterFromAssembly(Assembly.GetExecutingAssembly());
    }

    // Escanea métodos estáticos con [TerbinCommand(1)] y firma Task<Capsule>(Header, byte[])
    public static void RegisterFromAssembly(Assembly pAssembly)
    {
        foreach (var type in pAssembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attr = method.GetCustomAttribute<TerbinExecutableAttribute>(inherit: false);
                if (attr == null) continue;

                // Requiere: Task<Capsule> M(Header, byte[])
                var parameters = method.GetParameters();
                if (parameters.Length != 2 ||
                    parameters[0].ParameterType != typeof(Header) ||
                    parameters[1].ParameterType != typeof(MemoryStream))
                    continue;

                if (method.ReturnType != typeof(Task<PacketRequest>))
                    continue;

                // Crea delegate fuertemente tipado (evita reflexión por llamada)
                var del = (Func<Header, MemoryStream, Task<PacketRequest>>)Delegate.CreateDelegate(
                    typeof(Func<Header, MemoryStream, Task<PacketRequest>>), method);

                Register(attr.Action, (h, b) => del(h, b));
            }
        }
    }

}
