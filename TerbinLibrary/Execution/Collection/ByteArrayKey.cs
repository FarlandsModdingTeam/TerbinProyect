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


/// <summary>
/// ___________________( Español )___________________<br />
/// Estructura de solo lectura que representa una clave basada en un arreglo de bytes.<br />
/// Ideal para usar en colecciones concurrentes (ConcurrentDictionary) comparando el contenido del arreglo en lugar de su referencia.<br />
/// Notas: Garantiza que dos secuencias de bytes iguales tengan el mismo código Hash y por consiguiente se interpreten como iguales.<br />
/// Tips: Aprovecha las conversiones implícitas (byte[]) para trabajar directamente con parámetros nativos sin instanciar la estructura a mano.<br />
/// ___________________( English )___________________<br />
/// Read-only structure representing a byte array based key.<br />
/// Ideal for use in concurrent collections (ConcurrentDictionary), comparing the array content instead of its reference.<br />
/// Notes: Ensures that two equal byte sequences yield the same Hash code and are therefore interpreted as equal.<br />
/// Tips: Take advantage of the implicit conversions (byte[]) to work directly with native parameters without instantiating the struct manually.<br />
/// </summary>
public readonly struct ByteArrayKey : IEnumerable<byte>, IEquatable<ByteArrayKey>, IEquatable<IEnumerable<byte>>
{
    private readonly byte[] _data;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de la clave de bytes copiando el arreglo proporcionado de forma segura.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of the byte key by safely cloning the provided array.<br />
    /// </summary>
    /// <param name="pData">Es: El arreglo de bytes de entrada. <br />En: The input byte array.</param>
    public ByteArrayKey(params byte[] pData)
    {
        _data = (byte[])pData.Clone() ?? throw new ArgumentNullException(nameof(pData));
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Determina si un objeto genérico especificado es igual a la instancia actual revisando su tipo subyacente.<br />
    /// ___________________( English )___________________<br />
    /// Determines whether a specified generic object is equal to the current instance by checking its underlying type.<br />
    /// </summary>
    /// <param name="obj">Es: El objeto a comparar. <br />En: The object to compare.</param>
    public override bool Equals(object? obj)
    {
        if (obj is ByteArrayKey key)
            return Equals(key);
        if (obj is IEnumerable<byte> enumerable)
            return Equals(enumerable);
        return false;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Determina si la clave actual es igual a otra evaluando que ambas secuencias de bytes sean idénticas.<br />
    /// ___________________( English )___________________<br />
    /// Determines whether the current key is equal to another by verifying that both byte sequences are identical.<br />
    /// </summary>
    /// <param name="pOther">Es: La otra clave a comparar. <br />En: The other key to compare.</param>
    public bool Equals(ByteArrayKey pOther) => _data.SequenceEqual(pOther._data);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Determina si la clave actual es igual a una secuencia genérica de bytes (IEnumerable).<br />
    /// ___________________( English )___________________<br />
    /// Determines whether the current key is equal to a generic byte sequence (IEnumerable).<br />
    /// </summary>
    /// <param name="pOther">Es: La secuencia de bytes a comparar. <br />En: The byte sequence to compare.</param>
    public bool Equals(IEnumerable<byte>? pOther)
    {
        if (pOther == null) return false;
        return _data.SequenceEqual(pOther);
    }

    public IEnumerator<byte> GetEnumerator()
    {
        return ((IEnumerable<byte>)_data).GetEnumerator();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Genera el código hash de la instancia multiplicando los valores con una base algorítmica.<br />
    /// ___________________( English )___________________<br />
    /// Generates the hash code of the instance by multiplying the values with an algorithmic base.<br />
    /// </summary>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Conversión implícita de un arreglo de bytes tradicional a nuestra estructura `ByteArrayKey`.<br />
    /// ___________________( English )___________________<br />
    /// Implicit conversion from a traditional byte array to our `ByteArrayKey` struct.<br />
    /// </summary>
    /// <param name="pData">Es: El arreglo de bytes nativo. <br />En: The native byte array.</param>
    public static implicit operator ByteArrayKey(byte[] pData) => new ByteArrayKey(pData);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Conversión implícita de una estructura `ByteArrayKey` al arreglo base nativo (byte[]).<br />
    /// ___________________( English )___________________<br />
    /// Implicit conversion from a `ByteArrayKey` struct to the base native array (byte[]).<br />
    /// </summary>
    /// <param name="pKey">Es: La instancia envuelta en la llave. <br />En: The instance wrapped by the key.</param>
    public static implicit operator byte[](ByteArrayKey pKey) => pKey._data;
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Extensiones para simplificar el uso del diccionario concurrente aplicando claves basadas en ByteArrayKey de forma transparente.<br />
/// ___________________( English )___________________<br />
/// Extensions to simplify working with ConcurrentDictionary, transparently applying keys based on ByteArrayKey.<br />
/// </summary>
public static class ByteArrayKeyExtensions
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta obtener el valor asociado a los bytes especificados como clave aprovechando su conversión implícita.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to retrieve the value associated with the specified bytes as a key, effectively leveraging implicit conversion.<br />
    /// </summary>
    /// <param name="pDictionary">Es: El diccionario desde donde realizar la obtención. <br />En: The dictionary to retrieve from.</param>
    /// <param name="pKey">Es: Arreglo de bytes que será tratado como clave principal. <br />En: Byte array to be handled as primary key.</param>
    /// <param name="pValue">Es: Valor extraído si se logra concretar la operación. <br />En: The extracted value if the operation succeeds.</param>
    public static bool TryGetValue<T>(
        this ConcurrentDictionary<ByteArrayKey, T> pDictionary,
        byte[] pKey,
        out T pValue)
    {
#pragma warning disable CS8601 // Posible asignación de referencia nula
        return pDictionary.TryGetValue(new ByteArrayKey(pKey), out pValue);
#pragma warning restore CS8601 // Posible asignación de referencia nula
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta añadir un par clave-valor, manejando la clave inicial como un arreglo de bytes.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to add a key-value pair, internally handling the initial key as a byte array.<br />
    /// </summary>
    /// <param name="pDictionary">Es: El diccionario donde será anexado. <br />En: The dictionary where it will be attached.</param>
    /// <param name="pKey">Es: Arreglo de bytes convertido en clave indexable. <br />En: Byte array implicitly cast as indexable key.</param>
    /// <param name="pValue">Es: El valor real que será relacionado con la clave provista. <br />En: The actual value referencing the provided key.</param>
    public static bool TryAdd<T>(
        this ConcurrentDictionary<ByteArrayKey, T> pDictionary,
        byte[] pKey,
        T pValue)
    {
        return pDictionary.TryAdd(new ByteArrayKey(pKey), pValue);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta eliminar un elemento indexado por un arreglo de bytes y devuelve el valor que fue extraído.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to remove an item indexed by a byte array and returns the extracted value.<br />
    /// </summary>
    /// <param name="pDictionary">Es: El diccionario a afectar con la extracción. <br />En: The dictionary subject to extraction.</param>
    /// <param name="pKey">Es: Llave a modo de arreglo de bytes para ubicar al elemento. <br />En: Byte array key to resolve the item.</param>
    /// <param name="pValue">Es: Referencia externa al valor suprimido de manera exitosa. <br />En: Extern reference mapped string for successfully suppressed value.</param>
    public static bool TryRemove<T>(
        this ConcurrentDictionary<ByteArrayKey, T> pDictionary,
        byte[] pKey,
        out T pValue)
    {
#pragma warning disable CS8601 // Posible asignación de referencia nula
        return pDictionary.TryRemove(new ByteArrayKey(pKey), out pValue);
#pragma warning restore CS8601 // Posible asignación de referencia nula
    }
}