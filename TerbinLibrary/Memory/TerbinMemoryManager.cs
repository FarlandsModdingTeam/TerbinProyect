using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Id;

namespace TerbinLibrary.Memory;/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */


/// <summary>
/// Gestor estático para la administración de contenedores de memoria de Terbin en concurrencia.
/// </summary>
public static class TerbinMemoryManager
{
    private static readonly ConcurrentDictionary<byte, TerbinMemory> _containers = new();

    public static ConcurrentDictionary<byte, TerbinMemory> Containers => _containers;



    /// <summary>
    /// Obtiene el identificador de un contenedor disponible. Si no hay ninguno libre, crea uno nuevo.
    /// </summary>
    /// <returns>El identificador en formato byte del contenedor de memoria.</returns>
    public static byte GetFreeStore()
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

    /// <summary>
    /// Crea internamente un nuevo contenedor de memoria y lo añade a la colección.
    /// </summary>
    /// <returns>Una tupla indicando si la operación fue exitosa y el identificador generado.</returns>
    private static (bool succes, byte id) createStore()
    {
        byte id = MiniID.NewB;
        return (_containers.TryAdd(id, new TerbinMemory { Id = id }), id);
    }

    /// <summary>
    /// Almacena un fragmento de datos en el contenedor de memoria especificado.
    /// </summary>
    /// <param name="pIdMemory">Identificador del contenedor de memoria.</param>
    /// <param name="pOrder">El orden que corresponde a este fragmento de datos.</param>
    /// <param name="pData">El arreglo de bytes a almacenar.</param>
    public static void Store(byte pIdMemory, ushort pOrder, byte[] pData)
    {
        var container = _containers.GetOrAdd(pIdMemory, id => new TerbinMemory { IdRequest = id });
        container.AddFragment(pOrder, pData);
    }

    /// <summary>
    /// Sobrescribe de manera forzada el contenido de un contenedor de memoria con nuevos datos.
    /// </summary>
    /// <param name="pIdMemory">Identificador del contenedor de memoria a sobrescribir.</param>
    /// <param name="pOrder">El orden que corresponde a este nuevo fragmento de datos.</param>
    /// <param name="pData">El nuevo arreglo de bytes a almacenar.</param>
    public static void OverwriteStore(byte pIdMemory, ushort pOrder, byte[] pData)
    {
        var newContainer = new TerbinMemory { IdRequest = pIdMemory };
        newContainer.AddFragment(pOrder, pData);
        _containers.AddOrUpdate(pIdMemory, newContainer, (_, _) => newContainer);
    }

    /// <summary>
    /// Intenta obtener los datos completos almacenados en un contenedor de memoria.
    /// </summary>
    /// <param name="pIdMemory">Identificador del contenedor de memoria.</param>
    /// <param name="pData">Parámetro de salida con los datos resultantes.</param>
    /// <returns>Una tupla con el éxito de la operación y el posible error asociado.</returns>
    public static (bool succes, TerbinErrorCode typeError) TryGetResult(byte pIdMemory, out byte[] pData)
    {
        if (_containers.TryGetValue(pIdMemory, out var container))
        {
            if (container.TryGetFullData(out pData) is var r && r.succes)
                return (true, TerbinErrorCode.None);
            else
                return (false, r.typeError);
        }
        
        pData = [];
        // ¿no habia una excepcion de intentar hacceder a null?
        return (false, TerbinErrorCode.ValueOutOfRange);
    }

    /// <summary>
    /// Intenta obtener la instancia del contenedor de memoria asociado al identificador proporcionado.
    /// </summary>
    /// <param name="pIdMemory">Identificador del contenedor a buscar.</param>
    /// <param name="pMemory">Parámetro de salida con el contenedor de memoria encontrado.</param>
    /// <returns>True si se encontró el contenedor, False en caso contrario.</returns>
    public static bool TryGetMemory(byte pIdMemory, out TerbinMemory? pMemory)
    {
        bool success = _containers.TryGetValue(pIdMemory, out var memory);
        pMemory = memory;
        return success;
    }

    /// <summary>
    /// Marca el contenedor de memoria especificado como libre limpiando su estado interno.
    /// </summary>
    /// <param name="pIdMemory">Identificador del contenedor a liberar.</param>
    /// <returns>True si el contenedor existía y se liberó correctamente, False si no se encontró.</returns>
    public static bool Release(byte pIdMemory)
    {
        if (_containers.TryGetValue(pIdMemory, out var value))
        {
            value.Release();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Elimina permanentemente el contenedor de memoria del registro manejado.
    /// </summary>
    /// <param name="pIdMemory">Identificador del contenedor a eliminar.</param>
    /// <returns>True si fue eliminado, False si no existía.</returns>
    public static bool Remove(byte pIdMemory) => _containers.TryRemove(pIdMemory, out _);
}