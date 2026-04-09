using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Memory;
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

public class TerbinRAM
{
    public byte Id
    {
        get => field;
        set => field = value;
    } = (byte)CodeTerbinMemory.NotAsign;

    public ushort IdRequest
    {
        get => field;
        set => field = value;
    } = (ushort)CodeTerbinMemory.NotAsign;

    public bool IsOcupated => IdRequest != (ushort)CodeTerbinMemory.NotAsign;

    private volatile bool _isComplete;
    public bool IsComplete
    {
        get => _isComplete;
        internal set => _isComplete = value;
    }

    private readonly Dictionary<ushort, byte[]> _fragments = new();
    private int _totalSize = 0;

    public void AddFragment(ushort pOrder, byte[] pData, bool pIsFinal)
    {
        lock (_fragments)
        {
            if (!_fragments.ContainsKey(pOrder))
            {
                _fragments.Add(pOrder, pData);
                _totalSize += pData.Length;
            }
            if (pIsFinal) _isComplete = true;
        }
    }

    public byte[] GetFullData()
    {
        KeyValuePair<ushort, byte[]>[] fragmentsCopy;
        int totalSizeCopy;
        lock (_fragments)
        {
            fragmentsCopy = _fragments.ToArray();
            totalSizeCopy = _totalSize;
        }

        Array.Sort(fragmentsCopy, (a, b) => a.Key.CompareTo(b.Key));

        var result = new byte[totalSizeCopy];
        int offset = 0;
        foreach (var f in fragmentsCopy)
        {
            Buffer.BlockCopy(f.Value, 0, result, offset, f.Value.Length);
            offset += f.Value.Length;
        }
        return result;
    }

    public void Release()
    {
        IdRequest = (byte)CodeTerbinMemory.NotAsign;

        lock (_fragments)
        {
            _fragments.Clear();
            _totalSize = 0;
        }

        IsComplete = false;
    }
}