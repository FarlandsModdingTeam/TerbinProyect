using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TerbinLibrary.Communication.Packets;
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
    byte[] Action { get; }
    int Leght { get; }
    Type Dispatcher { get; }
}
public interface IExecutableDispatcher
{
    void Register(IExecutableAttribute pAttribute, TerbinExecutableDelegate pHandler);
    //Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload);
    //Task<InfoResponse?> DispatchAsync(PacketRequest pCapsule);
    Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload, IEquatable<IEnumerable<byte>> pActions);
    void RegisterFromAssembly(Assembly pAssembly);
}


[Obsolete]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinExecutable_ObsoleteAttribute(byte pAction) : Attribute, IExecutableAttribute
{
    public byte[] Action { get; } = new byte[] { pAction };
    public int Leght => Action.Length;
    public Type Dispatcher => typeof(ExecutableDispatcherSimple);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinExecutableAttribute : Attribute, IExecutableAttribute
{
    public byte[] Action { get; }
    public int Leght => Action.Length;
    public Type Dispatcher => typeof(ExecutableDispatcher);

    public TerbinExecutableAttribute(params byte[] pAction)
    {
        this.Action = pAction;
    }

    public TerbinExecutableAttribute(params object[] pAction)
    {
        if (pAction?.Length > byte.MaxValue)
            throw new OverflowException($"Actionre overflow byte max");
#pragma warning disable CS8604 // Posible argumento de referencia nulo
        this.Action = Serialineitor.ConvertToByte(pAction); // Peta adentro.
#pragma warning restore CS8604 // Posible argumento de referencia nulo
    }
}
