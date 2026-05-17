using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Execution.Collection;
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


public struct IdArray : IStructSerializable, IEnumerable<byte>, IEquatable<IdArray>, IEquatable<IEnumerable<byte>>
{
    private byte[] _actionMethod;

    public IdArray(params byte[] pAction)
    {
        ArgumentNullException.ThrowIfNull(pAction);
        if (pAction.Length > byte.MaxValue)
            throw new OverflowException($"Actionre overflow byte max");
        this._actionMethod = pAction;
    }

    public override bool Equals(object? obj)
    {
        if (obj is IdArray key)
            return Equals(key);
        if (obj is IEnumerable<byte> enumerable)
            return Equals(enumerable);
        return false;
    }

    public bool Equals(IdArray pOther) => _actionMethod.SequenceEqual(pOther._actionMethod);
    public bool Equals(IEnumerable<byte>? pOther)
    {
        if (pOther == null) return false;
        return _actionMethod.SequenceEqual(pOther);
    }

    public IEnumerator<byte> GetEnumerator()
    {
        return ((IEnumerable<byte>)_actionMethod).GetEnumerator();
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < _actionMethod.Length; i++)
                hash = hash * 31 + _actionMethod[i];
            return hash;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _actionMethod.GetEnumerator();
    }


    public byte[] ActionMethod
    {
        readonly get => _actionMethod;
        set
        {
            if (value.Length > byte.MaxValue)
                throw new OverflowException($"Actionre overflow byte max");
            _actionMethod = value;
        }
    }

    public readonly byte this[byte pIndex]
    {
        get => _actionMethod[pIndex];
        set => _actionMethod[pIndex] = value;
    }

    public void SetAction(params byte[] pActionMethod)
    {
        ActionMethod = pActionMethod;
    }

    public readonly ushort GetSize() => (ushort)(_actionMethod.Length + 1);

    public readonly void WriteTo(Span<byte> pBuffer)
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

    public static bool operator ==(IdArray pLeft, IdArray pRight) => pLeft.Equals(pRight);
    public static bool operator !=(IdArray pLeft, IdArray pRight) => !pLeft.Equals(pRight);

    public static implicit operator IdArray(byte[] pData) => new IdArray(pData);
    public static implicit operator byte[](IdArray pKey) => pKey._actionMethod;
}
