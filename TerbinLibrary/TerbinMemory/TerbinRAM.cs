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
    }
    = (byte)CodeTerbinMemory.NotAsign;

    public ushort IdRequest
    {
        get => field;
        set => field = value;
    }
    = (ushort)CodeTerbinMemory.NotAsign;

    public bool IsOcupated => IdRequest != (ushort)CodeTerbinMemory.NotAsign;

    private readonly Dictionary<ushort, byte[]> _fragments = new();
    private int _totalSize = 0;

    public event Action? OnAdd;
    public event Action? OnRelease;

    // TODO: comprobar nulos, vacios, etc.
    public void AddFragment(ushort pOrder, byte[] pData)
    {
        lock (_fragments)
        {
            if (!_fragments.ContainsKey(pOrder))
            {
                _fragments.Add(pOrder, pData);
                _totalSize += pData.Length;
            }
        }
        OnAdd?.Invoke();
    }

    // TODO: Comprobar si falta alguna parte intermedia.
    // TODO: Fragmentar/Encapsular.
    [Obsolete]
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

    public (bool succes, TerbinErrorCode typeError) TryGetFullData(out byte[] pData)
    {
        pData = [];

        KeyValuePair<ushort, byte[]>[] fragmentsCopy;
        int totalSizeCopy;
        lock (_fragments)
        {
            fragmentsCopy = _fragments.ToArray();
            totalSizeCopy = _totalSize;
        }

        if (fragmentsCopy.Length == 0)
            return (false, TerbinErrorCode.InvalidLength);

        Array.Sort(fragmentsCopy, (a, b) => a.Key.CompareTo(b.Key));

        // Comprobamos si falta alguna parte de informacio intermedia.
        if (!chechMissing(fragmentsCopy))
            return (false, TerbinErrorCode.OrderMismatch);

        pData = new byte[totalSizeCopy];
        int offset = 0;
        foreach (var f in fragmentsCopy)
        {
            Buffer.BlockCopy(f.Value, 0, pData, offset, f.Value.Length);
            offset += f.Value.Length;
        }
 
        return (true, TerbinErrorCode.None);
    }

    private bool chechMissing(KeyValuePair<ushort, byte[]>[] pFragments)
    {
        for (ushort i = 0; i < pFragments.Length; i++)
        {
            if (pFragments[i].Key != (i + 1))
                return false;
        }
        return true;
    }

    public void Release()
    {
        IdRequest = (byte)CodeTerbinMemory.NotAsign;

        OnAdd = null;
        OnRelease?.Invoke();

        lock (_fragments)
        {
            _fragments.Clear();
            _totalSize = 0;
        }
    }
}