using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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


[StructLayout(LayoutKind.Sequential)]
public struct IdArray : IStructSerializable, ICollection, ICollection<byte>, IEnumerable<byte>, IEquatable<IdArray>, IEquatable<IEnumerable<byte>>
{
    private byte[] _actionMethod;
    private object _lock = new();

    public readonly int Count => _actionMethod.Length;
    public readonly bool IsReadOnly => false;

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

    public readonly bool IsSynchronized => false;

    public readonly object SyncRoot => _lock;

    public readonly byte this[byte pIndex]
    {
        get => _actionMethod[pIndex];
        set => _actionMethod[pIndex] = value;
    }


    public IdArray(params byte[] pAction)
    {
        ArgumentNullException.ThrowIfNull(pAction);
        if (pAction.Length > byte.MaxValue)
            throw new OverflowException($"Actionre overflow byte max");
        this._actionMethod = pAction;
    }

    public readonly override bool Equals(object? obj)
    {
        if (obj is IdArray key)
            return Equals(key);
        if (obj is IEnumerable<byte> enumerable)
            return Equals(enumerable);
        return false;
    }

    public readonly bool Equals(IdArray pOther) => _actionMethod.SequenceEqual(pOther._actionMethod);
    public readonly bool Equals(IEnumerable<byte>? pOther)
    {
        if (pOther == null) return false;
        return _actionMethod.SequenceEqual(pOther);
    }

    public readonly IEnumerator<byte> GetEnumerator()
    {
        return ((IEnumerable<byte>)_actionMethod).GetEnumerator();
    }

    public override readonly int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < _actionMethod.Length; i++)
                hash = hash * 31 + _actionMethod[i];
            return hash;
        }
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return _actionMethod.GetEnumerator();
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

    public void Add(byte pItem)
    {
        int length = _actionMethod?.Length ?? 0;
        if (length + 1 > byte.MaxValue)
            throw new OverflowException("Actionre overflow byte max");

        Array.Resize(ref _actionMethod, length + 1);
        _actionMethod[length] = pItem;
    }

    public readonly void Clear()
    {
        Array.Clear(_actionMethod);
    }

    public readonly bool Contains(byte pItem)
    {
        return _actionMethod.Contains(pItem);
    }

    public readonly void CopyTo(byte[] pArray, int pIndex)
    {
        _actionMethod.CopyTo(pArray, pIndex);
    }

    public void CopyTo(Array pArray, int pIndex)
    {
        if (pArray is null)
            throw new ArgumentNullException(nameof(pArray));
        if (pArray.Rank != 1)
            throw new ArgumentException("Array must be one-dimensional.", nameof(pArray));
        if (pIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(pIndex));
        if (pArray.Length - pIndex < _actionMethod.Length)
            throw new ArgumentException("Destination array is not long enough.");

        if (pArray is byte[] byteArray)
        {
            _actionMethod.CopyTo(byteArray, pIndex);
            return;
        }

        for (int i = 0; i < _actionMethod.Length; i++)
            pArray.SetValue(_actionMethod[i], pIndex + i);
    }

    public bool Remove(byte pItem)
    {
        return Operate(b => b == pItem, (b) => { return new byte(); });
    }

    public bool Operate(Predicate<byte> pMonk, Func<byte, byte?> pTransform)
    {
        for (int i = 0; i < _actionMethod.Length; i++)
        {
            if (pMonk(_actionMethod[i]))
            {
                _actionMethod[i] = pTransform(_actionMethod[i]) ?? _actionMethod[i];
                return true;
            }
        }
        return false;
    }
    public void OperateInfinite(Predicate<byte> pMonk, Func<byte, byte?> pTransform)
    {
        for (int i = 0; i < _actionMethod.Length; i++)
        {
            if (pMonk(_actionMethod[i]))
                _actionMethod[i] = pTransform(_actionMethod[i]) ?? _actionMethod[i];
        }
    }


    public static bool operator ==(IdArray pLeft, IdArray pRight) => pLeft.Equals(pRight);
    public static bool operator !=(IdArray pLeft, IdArray pRight) => !pLeft.Equals(pRight);

    public static implicit operator IdArray(byte[] pData) => new IdArray(pData);
    public static implicit operator IdArray(ByteArrayKey pData) => new IdArray(pData);
    public static implicit operator byte[](IdArray pKey) => pKey._actionMethod;
}
