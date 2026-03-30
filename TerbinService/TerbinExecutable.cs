using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication;

namespace TerbinService;
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
    public CodeAction Action { get; }

    // TODO: guardar la lista de types de la funcion.
    // TODO: funcion estatica que serialice/deserialice a los tipos y cantidades guardadas.

    public TerbinExecutableAttribute(CodeAction pAction) => Action = pAction;
    public TerbinExecutableAttribute(byte pAction) => Action = (CodeAction)pAction;
}

// NOTA: ¿Para que sirve esto?.
public delegate Task<Capsule> ExecutableHandler(Header Action, byte[] parameters);


public sealed class ExecutableDispatcher
{
    private readonly ConcurrentDictionary<CodeAction, ExecutableHandler> _handlers = new();


    public void Register(CodeAction pAction, ExecutableHandler pHandler)
    {
        if (pHandler == null) throw new ArgumentNullException(nameof(pHandler));
        _handlers[pAction] = pHandler;
    }

    public bool Unregister(CodeAction pAction) => _handlers.TryRemove(pAction, out _);


    public async Task<Capsule> DispatchAsync(Capsule capsule) // Aqui es donde se ejecuta, manda cojones.
    {
        if (!_handlers.TryGetValue(capsule.ActionMethod, out var handler))
        {
            capsule.Head.Status = CodeStatus.NotFound;
            return capsule;
        }

        try
        {
            return await handler(capsule.Head, capsule.Parameters ?? Array.Empty<byte>())
                .ConfigureAwait(false); // ¿?
        }
        catch
        {
            capsule.Head.Status = CodeStatus.NotAsign;
            return capsule;
        }
    }


    // Escanea métodos estáticos con [TerbinCommand(1)] y firma Task<Capsule>(Header, byte[])
    public void RegisterFromAssembly(Assembly pAssembly)
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
                    parameters[1].ParameterType != typeof(byte[]))
                    continue;

                if (method.ReturnType != typeof(Task<Capsule>))
                    continue;

                // Crea delegate fuertemente tipado (evita reflexión por llamada)
                var del = (Func<Header, byte[], Task<Capsule>>)Delegate.CreateDelegate(
                    typeof(Func<Header, byte[], Task<Capsule>>), method);

                Register(attr.Action, (h, b) => del(h, b));
            }
        }
    }
}
