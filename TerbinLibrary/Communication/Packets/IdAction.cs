using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Communication.Packets;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */


public struct IdAction
{
    private byte[] _actionMethod;

    public byte[] ActionMethod
    {
        get => _actionMethod;
        set
        {
            if (value.Length > byte.MaxValue)
                throw new OverflowException($"Actionre overflow byte max");
            _actionMethod = value;
        }
    }

    public IdAction(params byte[] pAction)
    {
        if (pAction.Length > byte.MaxValue)
            throw new OverflowException($"Actionre overflow byte max");
        this._actionMethod = pAction;
    }

    public void SetAction(params byte[] pActionMethod)
    {
        ActionMethod = pActionMethod;
    }
}
