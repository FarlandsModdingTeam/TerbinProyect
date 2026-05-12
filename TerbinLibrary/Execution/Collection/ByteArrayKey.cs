using System;
using System.Collections;
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


public readonly struct ByteArrayKey : IEnumerable<byte>, IEquatable<ByteArrayKey>, IEquatable<IEnumerable<byte>>
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

    public IEnumerator<byte> GetEnumerator()
    {
        return ((IEnumerable<byte>)_data).GetEnumerator();
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    public static bool operator ==(ByteArrayKey pLeft, ByteArrayKey pRight) => pLeft.Equals(pRight);
    public static bool operator !=(ByteArrayKey pLeft, ByteArrayKey pRight) => !pLeft.Equals(pRight);
    public static implicit operator ByteArrayKey(byte[] pData) => new ByteArrayKey(pData);
}

public static class ByteArrayKeyExtensions
{
    public static bool TryGetValue<T>(
        this ConcurrentDictionary<ByteArrayKey, T> pDictionary,
        byte[] pKey,
        out T pValue)
    {
        return pDictionary.TryGetValue(new ByteArrayKey(pKey), out pValue);
    }

    public static bool TryAdd<T>(
        this ConcurrentDictionary<ByteArrayKey, T> pDictionary,
        byte[] pKey,
        T pValue)
    {
        return pDictionary.TryAdd(new ByteArrayKey(pKey), pValue);
    }

    public static bool TryRemove<T>(
        this ConcurrentDictionary<ByteArrayKey, T> pDictionary,
        byte[] pKey,
        out T pValue)
    {
        return pDictionary.TryRemove(new ByteArrayKey(pKey), out pValue);
    }
}