using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Id;

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

// TODO: Solicitar memoria
// ├─Solicita.
// ├─Crea o asigna memoria.
// └─Devuelve Id de memoria.

// TODO: Sobre escribe datos.
// TODO: Añade datos.
// TODO: Limpia (pero desde Containers).
public static class TerbinMemory
{
    private static readonly ConcurrentDictionary<byte, TerbinRAM> _containers = new();


    public static byte GetStore()
    {
        byte? idContainer = null;
        foreach (var item in _containers)
        {
            if (!item.Value.IsOcupated)
            {
                idContainer = item.Key;
                break;
            }
        }
        if (idContainer != null)
            return idContainer.Value;
        return createStore().id;
    }

    private static (bool succes, byte id) createStore()
    {
        byte id = MiniID.NewB;
        return (_containers.TryAdd(id, new TerbinRAM { Id = id }), id);
    }

    public static void Store(byte pIdMemory, ushort pOrder, byte[] pData)
    {
        var container = _containers.GetOrAdd(pIdMemory, id => new TerbinRAM { IdRequest = id });
        container.AddFragment(pOrder, pData);
    }


    public static (bool succes, ErrorFlags typeError) TryGetResult(byte pIdMemory, out byte[] pData)
    {
        if (_containers.TryGetValue(pIdMemory, out var container))
        {
            if (container.TryGetFullData(out pData) is var r && r.succes)
                return (true, 0);
            else
                return (false, r.typeError);
        }
        pData = [];
        // no habia una excepcion de intentar hacceder a null?
        return (false, ErrorFlags.NullParameter);
    }


    public static bool Release(byte pIdMemory)
    {
        if (_containers.TryGetValue(pIdMemory, out var value))
        {
            value.Release();
            return true;
        }
        return false;
    }

    public static bool Remove(byte pIdMemory) => _containers.TryRemove(pIdMemory, out _);
}