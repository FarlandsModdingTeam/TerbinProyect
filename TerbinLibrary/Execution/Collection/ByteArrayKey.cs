using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Execution.Collection;
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


public readonly struct ByteArrayKey : IEquatable<ByteArrayKey>, IEquatable<IEnumerable<byte>>
{
    private readonly byte[] _data;

    public ByteArrayKey(byte[] pData)
    {
        _data = pData ?? throw new ArgumentNullException(nameof(pData));
    }

    public override bool Equals(object? obj)
    {
        if (obj is ByteArrayKey key)
            return Equals(key);
        if (obj is IEnumerable<byte> enumerable)
            return Equals(enumerable);
        return false;
    }

    public bool Equals(ByteArrayKey pOther) => _data.SequenceEqual(pOther._data);
    public bool Equals(IEnumerable<byte>? pOther)
    {
        if (pOther == null) return false;
        return _data.SequenceEqual(pOther);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < _data.Length; i++)
                hash = hash * 31 + _data[i];
            return hash;
        }
    }

    public static bool operator ==(ByteArrayKey pLeft, ByteArrayKey pRight) => pLeft.Equals(pRight);
    public static bool operator !=(ByteArrayKey pLeft, ByteArrayKey pRight) => !pLeft.Equals(pRight);
}
