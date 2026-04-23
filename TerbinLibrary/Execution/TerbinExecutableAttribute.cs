using System;
using System.Collections.Generic;
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
    int Leght { get; }
}
public interface IExecutableDispatcher
{
    void RegisterSingle(TerbinExecutableHandler pHandler, params byte[] pActions);
    Task<InfoResponse?> DispatchAsync(Header pHead, byte[] pPayload);
}



[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinExecutableAttribute : Attribute, IExecutableAttribute
{
    public byte[] Action { get; }

    public int Leght => Action.Length;

    public TerbinExecutableAttribute(params byte[] pAction) => Action = pAction;
}

/*
[Obsolete]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TerbinExecutableCompoundAttribute : Attribute, IExecutableAttribute
{
    public byte Action { get; }
    public byte Entity { get; }


    public TerbinExecutableCompoundAttribute(byte pAction, byte pEntity)
    {
        Action = pAction;
        Entity = pEntity;
    }
}
/