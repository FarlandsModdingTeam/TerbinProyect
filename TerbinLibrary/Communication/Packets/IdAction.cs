using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Serialize;

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


public struct IdAction : IStructSerializable
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

    public readonly ushort GetSize() => (ushort)(_actionMethod.Length + 1);

    public void WriteTo(Span<byte> pBuffer)
    {
        if (_actionMethod.Length > byte.MaxValue)
            throw new OverflowException("Over Size Action Method");
        int offset = 0;
        pBuffer.Write<byte>(ref offset, (byte)_actionMethod.Length);
        Span<byte> bytes = Serialineitor.SerializeArrayRaw<byte>(_actionMethod, _actionMethod.Length).AsSpan();
        bytes.CopyTo(pBuffer[offset..]);
    }

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        byte length;
        length = pBuffer.Read<byte>();
        _actionMethod = Serialineitor.DeserializeArrayRaw<byte>(pBuffer, length);
    }
}
