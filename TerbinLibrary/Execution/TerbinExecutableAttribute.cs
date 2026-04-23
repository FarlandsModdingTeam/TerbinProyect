using System;
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
    void RegisterFromAssembly(Assembly pAssembly);
}



[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinExecutableAttribute(byte pAction) : Attribute, IExecutableAttribute
{
    public byte[] Action { get; } = new byte[] { pAction };
    public int Leght => Action.Length;
    public Type Dispatcher => typeof(SimpleExecutableDispatcher);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinExecutableCompoundAttribute(params byte[] pAction) : Attribute, IExecutableAttribute
{
    public byte[] Action { get; } = pAction;
    public int Leght => Action.Length;
    public Type Dispatcher => typeof(CompoundExecutableDispatcher);
}
