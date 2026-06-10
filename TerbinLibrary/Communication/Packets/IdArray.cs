using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Execution.Collection;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.Useful;

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


/// <summary>
/// ___________________( Español )___________________<br />
/// Estructura que representa un arreglo de identificadores (bytes) utilizado para métodos de acción.<br />
/// Provee capacidades de serialización, colección y comparación.<br />
/// Notas: Puede lanzar excepciones si el tamaño del arreglo supera 255 (byte max).<br />
/// Tips: Se integra fácilmente con utilidades de serialización.<br />
/// ___________________( English )___________________<br />
/// Structure representing an array of identifiers (bytes) used as action methods.<br />
/// Provides serialization, collection, and equality comparison capabilities.<br />
/// Notes: It can throw exceptions if the array size exceeds 255 (byte max).<br />
/// Tips: Easily integrates with serialization utilities.<br />
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct IdArray : IStructSerializable, ICollection, ICollection<byte>, IEnumerable<byte>, IEquatable<IdArray>, IEquatable<IEnumerable<byte>>
{
    private byte[] _actionMethod;
    private readonly object _lock = new();

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el número de identificadores en el arreglo subyacente.<br />
    /// ___________________( English )___________________<br />
    /// Gets the number of identifiers in the underlying array.<br />
    /// </summary>
    public readonly int Count => _actionMethod.Length;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Indica si la colección es de solo lectura. Siempre devuelve falso.<br />
    /// ___________________( English )___________________<br />
    /// Indicates whether the collection is read-only. Always returns false.<br />
    /// </summary>
    public readonly bool IsReadOnly => false;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene o establece el arreglo de identificadores (método o acción).<br />
    /// ___________________( English )___________________<br />
    /// Gets or sets the identifiers array (action or method).<br />
    /// </summary>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Indica si el acceso a la colección está sincronizado (seguro para subprocesos).<br />
    /// ___________________( English )___________________<br />
    /// Indicates whether access to the collection is synchronized (thread-safe).<br />
    /// </summary>
    public readonly bool IsSynchronized => false;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene un objeto que puede usarse para sincronizar el acceso a la colección.<br />
    /// ___________________( English )___________________<br />
    /// Gets an object that can be used to synchronize access to the collection.<br />
    /// </summary>
    public readonly object SyncRoot => _lock;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene o establece el identificador en un índice específico.<br />
    /// ___________________( English )___________________<br />
    /// Gets or sets the identifier at a specific index.<br />
    /// </summary>
    /// <param name="pIndex">Es: El índice del elemento a obtener o establecer.<br />En: The index of the element to get or set.</param>
    public readonly byte this[byte pIndex]
    {
        get => _actionMethod[pIndex];
        set => _actionMethod[pIndex] = value;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de la estructura con un arreglo de identificadores en bytes cruzados mediante parámetros (params).<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of the structure with a crossed identifiers byte array passing through parameters (params).<br />
    /// </summary>
    /// <param name="pAction">Es: Los identificadores de acción.<br />En: The action identifiers.</param>
    public IdArray(params byte[] pAction)
    {
        ArgumentNullException.ThrowIfNull(pAction);
        if (pAction.Length > byte.MaxValue)
            throw new OverflowException($"Actionre overflow byte max");
        this._actionMethod = pAction;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Inicializa una nueva instancia de la estructura desde un arreglo de objetos.<br />
    /// Permite convertir valores como Enums y Bytes de manera segura.<br />
    /// ___________________( English )___________________<br />
    /// Initializes a new instance of the structure from an array of objects.<br />
    /// Allows converting values like Enums and Bytes safely.<br />
    /// </summary>
    /// <param name="pAction">Es: Los objetos a convertir a identificadores.<br />En: The objects to convert to identifiers.</param>
    public IdArray(params object[] pAction)
    {
        if (pAction?.Length > byte.MaxValue)
            throw new OverflowException($"Actionre overflow byte max");
#pragma warning disable CS8604 // Posible argumento de referencia nulo
        this._actionMethod = Serialineitor.CastToByte(pAction); // Peta adentro.
#pragma warning restore CS8604 // Posible argumento de referencia nulo
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Compara este objeto con otro para determinar si son iguales.<br />
    /// ___________________( English )___________________<br />
    /// Compares this object with another to determine equality.<br />
    /// </summary>
    /// <param name="obj">Es: El objeto con el que se va a comparar.<br />En: The object to compare with.</param>
    public readonly override bool Equals(object? obj)
    {
        if (obj is IdArray key)
            return Equals(key);
        if (obj is IEnumerable<byte> enumerable)
            return Equals(enumerable);
        return false;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Verifica la igualdad de las secuencias de bytes entre esta instancia y otra.<br />
    /// ___________________( English )___________________<br />
    /// Verifies the byte sequences equality between this instance and another.<br />
    /// </summary>
    /// <param name="pOther">Es: La otra estructura IdArray que se comparará.<br />En: The other IdArray structure to compare.</param>
    public readonly bool Equals(IdArray pOther) => _actionMethod.SequenceEqual(pOther._actionMethod);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Verifica la igualdad con una secuencia externa de bytes.<br />
    /// ___________________( English )___________________<br />
    /// Verifies equality against an external byte sequence.<br />
    /// </summary>
    /// <param name="pOther">Es: La colección enumerada de bytes.<br />En: The enumerable byte collection.</param>
    public readonly bool Equals(IEnumerable<byte>? pOther)
    {
        if (pOther == null) return false;
        return _actionMethod.SequenceEqual(pOther);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Devuelve un enumerador que itera a través de los bytes.<br />
    /// ___________________( English )___________________<br />
    /// Returns an enumerator that iterates through the bytes.<br />
    /// </summary>
    public readonly IEnumerator<byte> GetEnumerator()
    {
        return ((IEnumerable<byte>)_actionMethod).GetEnumerator();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Sirve como función hash por defecto basada en el contenido actual del arreglo.<br />
    /// ___________________( English )___________________<br />
    /// Serves as the default hash function based on the current contents of the array.<br />
    /// </summary>
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Devuelve un enumerador que itera a través de la colección.<br />
    /// ___________________( English )___________________<br />
    /// Returns an enumerator that iterates through the collection.<br />
    /// </summary>
    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return _actionMethod.GetEnumerator();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Sobrescribe el método de acción de esta estructura.<br />
    /// ___________________( English )___________________<br />
    /// Overwrites the action method of this structure.<br />
    /// </summary>
    /// <param name="pActionMethod">Es: Los identificadores a establecer.<br />En: The identifiers to set.</param>
    public void SetAction(params byte[] pActionMethod)
    {
        ActionMethod = pActionMethod;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el tamaño requerido (en bytes) para serializar la estructura.<br />
    /// ___________________( English )___________________<br />
    /// Gets the size required (in bytes) to serialize the structure.<br />
    /// </summary>
    public readonly int GetSize() => (_actionMethod?.Length ?? 0) + 1;

    //public readonly ushort GetSize()
    //{
    //    Console.Log($"Leng: {_actionMethod.Length} + 1 = {_actionMethod.Length+1}");
    //    return (ushort)(_actionMethod.Length + 1);
    //}

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Serializa el contenido del arreglo y su longitud al rellenar los datos sobre la variable (pBuffer).<br />
    /// ___________________( English )___________________<br />
    /// Serializes the array's contents and length filling data upon the designated buffer (pBuffer).<br />
    /// </summary>
    /// <param name="pBuffer">Es: El búfer donde se escribirán los datos.<br />En: The buffer where the data will be written.</param>
    public readonly void WriteTo(Span<byte> pBuffer)
    {
        if (_actionMethod.Length > byte.MaxValue)
            throw new OverflowException("Over Size Action Method");
        int offset = 0;
        pBuffer.Write<byte>(ref offset, (byte)_actionMethod.Length);
        Span<byte> bytes = Serialineitor.SerializeArrayRaw<byte>(_actionMethod).AsSpan();
        bytes.CopyTo(pBuffer[offset..]);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Deserializa desde un búfer el arreglo y sus elementos.<br />
    /// ___________________( English )___________________<br />
    /// Deserializes the array and its elements from a buffer.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Búfer de solo lectura que contiene los datos.<br />En: Read-only buffer containing the data.</param>
    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        byte length;
        length = pBuffer.Read<byte>();
        _actionMethod = Serialineitor.DeserializeArrayRaw<byte>(pBuffer, length);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Agrega un nuevo byte al final del arreglo actual (se redimensiona consecuentemente).<br />
    /// ___________________( English )___________________<br />
    /// Adds a new byte at the end of the current array (resizing it consequently).<br />
    /// </summary>
    /// <param name="pItem">Es: El elemento a añadir.<br />En: The element to add.</param>
    public void Add(byte pItem)
    {
        int length = _actionMethod?.Length ?? 0;
        if (length + 1 > byte.MaxValue)
            throw new OverflowException("Actionre overflow byte max");

        Array.Resize(ref _actionMethod, length + 1);
        _actionMethod[length] = pItem;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Vacía o reinicia el contenido del arreglo estableciendo los valores por defecto.<br />
    /// ___________________( English )___________________<br />
    /// Clears or resets the array's content by setting default values.<br />
    /// </summary>
    public readonly void Clear()
    {
        Array.Clear(_actionMethod);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Determina si un elemento específico se encuentra en el arreglo.<br />
    /// ___________________( English )___________________<br />
    /// Determines whether a specific element is found in the array.<br />
    /// </summary>
    /// <param name="pItem">Es: El byte a buscar.<br />En: The byte to search for.</param>
    public readonly bool Contains(byte pItem)
    {
        return _actionMethod.Contains(pItem);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Copia los elementos del arreglo origen a uno de destino, a partir de un índice.<br />
    /// ___________________( English )___________________<br />
    /// Copies the elements of the source array to a destination one, starting at an index.<br />
    /// </summary>
    /// <param name="pArray">Es: Arreglo de destino.<br />En: Destination array.</param>
    /// <param name="pIndex">Es: El índice en el arreglo de destino.<br />En: The index in the destination array.</param>
    public readonly void CopyTo(byte[] pArray, int pIndex)
    {
        _actionMethod.CopyTo(pArray, pIndex);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Copia los identificadores en un arreglo no específico o base.<br />
    /// ___________________( English )___________________<br />
    /// Copies identifiers into a non-specific or base array.<br />
    /// </summary>
    /// <param name="pArray">Es: El arreglo general.<br />En: The general array destination.</param>
    /// <param name="pIndex">Es: La posición inicial donde se empieza a copiar.<br />En: The starting position where copying starts.</param>
    public void CopyTo(Array pArray, int pIndex)
    {
        ArgumentNullException.ThrowIfNull(pArray);
        if (pArray.Rank != 1)
            throw new ArgumentException("Array must be one-dimensional.", nameof(pArray));
        ArgumentOutOfRangeException.ThrowIfNegative(pIndex);
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

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Elimina de forma segura la primera ocurrencia de un objeto en particular.<br />
    /// ___________________( English )___________________<br />
    /// Safely removes the first occurrence of a particular object.<br />
    /// </summary>
    /// <param name="pItem">Es: El fragmento de información a ser eliminado.<br />En: The chunk of info to be removed.</param>
    public bool Remove(byte pItem)
    {
        //return Operate(b => b == pItem, _ => 0);
        for (int i = 0; i < _actionMethod.Length; i++)
        {
            if (_actionMethod[i] == pItem)
            {
                Array.Copy(_actionMethod, i + 1, _actionMethod, i, _actionMethod.Length - i - 1);
                Array.Resize(ref _actionMethod, _actionMethod.Length - 1);
                return true;
            }
        }
        return false;
    }

    // Esto ya esta inventado y se llama Linq, pero esta chulo hacerlo uno mismo.
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Ejecuta una transformación sobre el primer elemento que cumpla una precondición y luego termina.<br />
    /// Notas: Esto ya está inventado y se llama Linq, pero está chulo hacerlo uno mismo.<br />
    /// ___________________( English )___________________<br />
    /// Executes a transformation over the first element that meets a precondition and then stops.<br />
    /// Notes: This is already invented and is called Linq, but it is cool to do it yourself.<br />
    /// </summary>
    /// <param name="pMonk">Es: El predicado para evaluar cada elemento.<br />En: The predicate to evaluate each element.</param>
    /// <param name="pTransform">Es: La función de retorno de la sustitución del valor nuevo.<br />En: The return function of the new value replacement.</param>
    public bool Operate(Predicate<byte> pMonk, Func<byte, byte> pTransform)
    {
        for (int i = 0; i < _actionMethod.Length; i++)
        {
            byte item = _actionMethod[i];
            if (pMonk(item))
            {
                _actionMethod[i] = pTransform(item);
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Itera sobre los elementos del arreglo y aplica una transformación a aquellos que cumplan con la condición.<br />
    /// Modifica los valores directamente en el arreglo interno.<br />
    /// Notas: Asegúrate de que los delegados proporcionados no lancen excepciones inesperadas, ya que interrumpirán el ciclo.<br />
    /// Tips: Ideal para realizar aplicar filtros y modificaciones en bloque de manera eficiente.<br />
    /// ___________________( English )___________________<br />
    /// Iterates over the array elements and applies a transformation to those that meet the condition.<br />
    /// Modifies the values directly within the internal array.<br />
    /// Notes: Ensure that the provided delegates do not throw unexpected exceptions, as they will interrupt the loop.<br />
    /// Tips: Ideal for applying bulk filters and modifications efficiently.<br />
    /// </summary>
    /// <param name="pMonk">Es: El predicado que evalúa cada elemento para determinar si debe ser transformado. <br />En: The predicate that evaluates each element to determine if it should be transformed.</param>
    /// <param name="pTransform">Es: La función de transformación que se aplica a los elementos que cumplen la condición. <br />En: The transformation function applied to the elements that meet the condition.</param>
    public void OperateInfinite(Predicate<byte> pMonk, Func<byte, byte> pTransform)
    {
        for (int i = 0; i < _actionMethod.Length; i++)
        {
            byte item = _actionMethod[i];
            if (pMonk(item))
                _actionMethod[i] = pTransform(item);
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Devuelve una representación en cadena del arreglo (incluyendo bloqueos internos y representaciones operativas).<br />
    /// ___________________( English )___________________<br />
    /// Retrieves a string format describing the array (including thread-safety scopes and operable displays).<br />
    /// </summary>
    public override string ToString()
    {
        //bool isLockedByOther = false;
        //if (Monitor.IsEntered(_lock))
        //    isLockedByOther = false;
        //else
        //{
        //    if (Monitor.TryEnter(_lock))
        //        Monitor.Exit(_lock);
        //    else
        //        isLockedByOther = true;
        //}
        return $"(Action: [{ProgressUtil.DebugTerbinLibrary.ArrayToString(_actionMethod)}])";
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Comprueba la igualdad estructural o sintáctica entre ambos objetos <c>IdArray</c>.<br />
    /// ___________________( English )___________________<br />
    /// Verifies syntactical or structural equality amid both <c>IdArray</c> objects.<br />
    /// </summary>
    public static bool operator ==(IdArray pLeft, IdArray pRight) => pLeft.Equals(pRight);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Descarta la igualdad entre comparaciones de estructuras dadas.<br />
    /// ___________________( English )___________________<br />
    /// Drops structural equality over compared item structures.<br />
    /// </summary>
    public static bool operator !=(IdArray pLeft, IdArray pRight) => !pLeft.Equals(pRight);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Facilita adaptaciones implícitas hacia instancias de <c>IdArray</c> a partir de arreglos de bytes.<br />
    /// ___________________( English )___________________<br />
    /// Smooths out implicit adaptations towards <c>IdArray</c> instances stemming from byte arrays.<br />
    /// </summary>
    public static implicit operator IdArray(byte[] pData) => new IdArray(pData);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea conversiones desde llaves relacionales a nuevos segmentos de esta estructura.<br />
    /// ___________________( English )___________________<br />
    /// Forges transparent implicit adaptations bridging associative arrays/keys info into this structure.<br />
    /// </summary>
    public static implicit operator IdArray(ByteArrayKey pData) => new IdArray(pData);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversiones implícitas desde <c>IdArray</c> hacia <c>ByteArrayKey</c>.<br />
    /// ___________________( English )___________________<br />
    /// Exposes straightforward adaptations.bounding from <c>IdArray</c> structures towards <c>ByteArrayKey</c> layouts.<br />
    /// </summary>
    public static implicit operator ByteArrayKey(IdArray pData) => new ByteArrayKey(pData);

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Habilita conversiones inversas como matriz de bytes cruda para uso directo en código externo o utilidades.<br />
    /// ___________________( English )___________________<br />
    /// Enables raw outward byte array fallbacks mapped for explicit or direct utility facing external bounds.<br />
    /// </summary>
    public static implicit operator byte[](IdArray pKey) => pKey._actionMethod;
}
