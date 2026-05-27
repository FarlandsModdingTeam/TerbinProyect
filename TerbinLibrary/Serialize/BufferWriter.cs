using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TerbinLibrary.Protocol;
using TerbinLibrary.TerbinServiceHelper.Consoles;

namespace TerbinLibrary.Serialize;
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

// TODO: Que estos si permitan ampliar automaticamente y adaptarlos a BufferErrorCode.
/// <summary>
/// ___________________( Español )___________________<br />
/// Clase utilitaria para la escritura de datos en búferes de memoria secuencial.<br />
/// Proporciona métodos para añadir tipos no administrados y arreglos de forma eficiente utilizando Spans.<br />
/// ___________________( English )___________________<br />
/// Utility class for writing data to sequential memory buffers.<br />
/// Provides methods to efficiently add unmanaged types and arrays using Spans.<br />
/// </summary>
public class BufferWriter
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Escribe un arreglo de tipo no administrado dentro de un Span de bytes subyacente y actualiza el desplazamiento.<br />
    /// Notas: Valida de forma temprana si el búfer tiene tamaño suficiente, previniendo sobreescritura incorrecta.<br />
    /// ___________________( English )___________________<br />
    /// Writes an unmanaged array inside an underlying byte Span and updates the offset.<br />
    /// Notes: Fast-fails by validating if the buffer has enough size, preventing incorrect overwriting.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Espacio de memoria de destino a escribir. <br />En: Destination memory space to write into.</param>
    /// <param name="pOffset">Es: Puntero o marca de desplazamiento actual en el búfer. <br />En: Pointer or offset marker in the buffer.</param>
    /// <param name="pArray">Es: El arreglo desprotegido de valores. <br />En: The raw array of values.</param>
    public static void AddArray<T>(Span<byte> pBuffer, ref int pOffset, T[] pArray)
        where T : unmanaged
    {
        if (pArray?.Length > ThreeQuartersInt.MaxValue)
            throw new InvalidOperationException("Array surpasses ThreeQuartersInt max");

        // Validar que hay al menos 3 bytes para escribir la longitud
        if ((pBuffer.Length - pOffset) < TerbinProtocol.LENGTH_ARRAY)
            throw new ArgumentOutOfRangeException(nameof(pBuffer),
                "There is not enough space in the buffer to write the length of the array.");

        //BitConverter.TryWriteBytes(pBuffer[pOffset..], Serialineitor.GetArraySize<T>(pArray?.Length ?? 0));
        ThreeQuartersInt lengthStruct = Serialineitor.GetArraySize<T>(pArray?.Length ?? 0);
        MemoryMarshal.Write(pBuffer[pOffset..], in lengthStruct);
        pOffset += TerbinProtocol.LENGTH_ARRAY;

        Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());

        // Validar que hay suficiente espacio en el búfer para los bytes del array
        if (pBuffer.Length - pOffset < bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(pBuffer),
                $"The buffer is too small. More are needed {bytes.Length} bytes, but only bytes remain {pBuffer.Length - pOffset}.");

        bytes.CopyTo(pBuffer[pOffset..]);
        pOffset += bytes.Length;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Escribe un valor genérico unmanaged al búfer actualizando su desplazamiento por referencia.<br />
    /// ___________________( English )___________________<br />
    /// Writes a generic unmanaged value to the buffer updating its reference offset.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Bloque de memoria Span a intervenir. <br />En: Span memory block to tap into.</param>
    /// <param name="pOffset">Es: Valor del desplazamiento sobre dónde se escriben los siguientes bytes. <br />En: Current bytes shift value marker over where to write.</param>
    /// <param name="pValue">Es: Objeto puro pre-serializado. <br />En: Pure object pre-serialized.</param>
    public static void Add<T>(Span<byte> pBuffer, ref int pOffset, T pValue)
        where T : unmanaged
    {
        MemoryMarshal.Write(pBuffer[pOffset..], in pValue); // ref
        pOffset += Unsafe.SizeOf<T>();
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Acopla una estructura implícitamente serializable al bloque de memoria.<br />
    /// ___________________( English )___________________<br />
    /// Glues an explicitly serializable structure over the memory block.<br />
    /// </summary>
    /// <param name="pBuffer">Es: El búfer original objetivo. <br />En: Targeted originating buffer.</param>
    /// <param name="pOffset">Es: Indicador del padding actual de lectura escritura. <br />En: Indicator of currently paddign read/write setup marker.</param>
    /// <param name="pStruct">Es: La estructura a ser copiada. <br />En: Structure struct to be copied over.</param>
    public static void AddStruct<T>(Span<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        byte[] strucBytes = Serialineitor.SerializeStructRaw(pStruct);
        strucBytes.CopyTo(pBuffer[pOffset..]);
        pOffset += strucBytes.Length;
    }


    // ¿Para que servia?
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Asegura y asigna capacidad dentro de un arreglo y escribe el contenido proporcionado allí.<br />
    /// Notas: Actualmente se re-asigna mediante una reestructuración de la base 2 del array total.<br />
    /// ___________________( English )___________________<br />
    /// Secures and allocates capacity inside an array and copies over the provided content.<br />
    /// Notes: Currently reallocates thru a base-2 reconfiguration of the total overall array.<br />
    /// </summary>
    /// <param name="buffer">Es: El búfer primigenio expandible. <br />En: Primal expandable core buffer matrix.</param>
    /// <param name="offset">Es: El marcador donde anexar información. <br />En: Sticking info marker target pointer place.</param>
    /// <param name="value">Es: El valor en bruto. <br />En: Raw core setup unmanaged value pointer field representation.</param>
    public static void EnsureAdd<T>(ref byte[] buffer, int offset, T value) where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (buffer.Length - offset < size)
        {
            // Crear uno más grande y copiar el contenido
            var newBuffer = new byte[buffer.Length * 2 + size]; // ¿El 2 debria actualizarlo a 3?
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
            buffer = newBuffer;
        }
        // Escribir el valor
        MemoryMarshal.Write(buffer.AsSpan(offset), in value);
    }
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Agrega funcionalidades fluidas (extensiones) extra para Spans a fin de proveer simplificación semántica.<br />
/// ___________________( English )___________________<br />
/// Appends fluid functional features (extensions) to Spans acting to grant syntax-sugar formatting semantic layout simplification.<br />
/// </summary>
public static class BufferWriterExtension
{
    // NOTA: Mi todo es fisicamente implosible, XD
    // TODO: darles una vuelta a los sin offset para que:
    // crean un nuevo Span donde pongan lo nuevo y luego sobreEscriban el antiguo Span con el nuevo.
    // Usar: Buffer.BlockCopy
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Permite acoplar y escribir cortando (slicing) el Span progresivamente, minimizando tamaños devueltos de forma transparente.<br />
    /// ___________________( English )___________________<br />
    /// Slices seamlessly slicing written sections progressively cutting returning target chunks seamlessly.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Bloque span mutado por cortes sucesivos. <br />En: Successively-sliced mutated chained ref span.</param>
    /// <param name="pValue">Es: Dato no gestionado a agregar. <br />En: Unmanaged attached raw variable detail.</param>
    /// <returns>Es: Un indicador de estado y validación. <br />En: Status feedback success or overflow signal error.</returns>
    public static BufferErrorCode Write<T>(this ref Span<byte> pBuffer, T pValue)
        where T : unmanaged
    {
        if (pBuffer.Length < Unsafe.SizeOf<T>())
            return BufferErrorCode.BufferSmall;

        MemoryMarshal.Write(pBuffer, in pValue);

        pBuffer = pBuffer[Unsafe.SizeOf<T>()..];
        return BufferErrorCode.Succes;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Facilita escribir cortes secuenciados transparentes al mutar progresivamente un Span sumándole arrays empaquetados.<br />
    /// ___________________( English )___________________<br />
    /// Facilitates seamless transparent progressively sliding seq chunk slice by packaging chained arrays up.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Mutable apuntador secuencial limitante Span . <br />En: Contiguous restricting shrinking seq Span target.</param>
    /// <param name="pArray">Es: Parámetros serializados no empaquetados. <br />En: Open serializable package non managed target parameters.</param>
    /// <returns>Es: Valor de evaluación si faltó tamaño buffer. <br />En: Size overflow buffer lack validation target state readout score.</returns>
    public static BufferErrorCode WriteArray<T>(this ref Span<byte> pBuffer, T[] pArray)
        where T : unmanaged
    {
        if (pArray?.Length > ThreeQuartersInt.MaxValue)
            return BufferErrorCode.SurpassesMax;

        pBuffer.Write(Serialineitor.GetArraySize<T>(pArray?.Length ?? 0));

        if (pArray != null && pArray.Length > 0)
        {
            Span<byte> bytes = MemoryMarshal.AsBytes(pArray.AsSpan());

            if (pBuffer.Length < bytes.Length)
                return BufferErrorCode.BufferSmall;

            bytes.CopyTo(pBuffer);

            pBuffer = pBuffer[bytes.Length..];
        }
        return BufferErrorCode.Succes;
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Interseca escribiendo por encima del Span restando y reemplazando con el valor de la estructura.<br />
    /// ___________________( English )___________________<br />
    /// Intersects over typing subtracting overriding shrinking size value off target given matching bounding struct pattern representation.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Limite ref de solo escritura modificable remante span. <br />En: Ref pointer bound shifting mutating chunk remnant span sequence string limit setup.</param>
    /// <param name="pStruct">Es: Molde a empaquetar por interface IStructSerializable. <br />En: IStructSerializable formatting mold object packager form target layout source parameter.</param>
    /// <returns>Es: Flag de error u suceso apropiado exitoso. <br />En: Valid setup formatting event correct feedback status.</returns>
    public static BufferErrorCode WriteStruct<T>(this ref Span<byte> pBuffer, T pStruct)
        where T : struct, IStructSerializable
    {
        byte[] strucBytes = Serialineitor.SerializeStructRaw(pStruct);

        if (pBuffer.Length < strucBytes.Length)
            return BufferErrorCode.BufferSmall;

        strucBytes.CopyTo(pBuffer);

        pBuffer = pBuffer[strucBytes.Length..];
        return BufferErrorCode.Succes;
    }


    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Extensión transparente análoga sin reducir el Span que usa puntero al desplazamiento.<br />
    /// ___________________( English )___________________<br />
    /// Transparent analogue syntax format without subtracting Span targeting offset marker layout mapping ref struct marker instead offset values directly bypassing shrink overhead layout.<br />
    /// </summary>
    /// <param name="pBuffer">Es: Buffer general global inmodificado de Span tamaño extendido. <br />En: Size preserving static unchanging generic global total Span limit capacity form factor chunk chain reference wrapper value pointer sequence array list payload setup element format factor context scope constraint layout bound factor parameter field representation matrix sequence.</param>
    /// <param name="pOffset">Es: Avanza actualizando donde acaba el paquete escrito. <br />En: Proceeds to log and push ahead ending written parameter chunk matrix scope limit marker wrapper location element layout pointer format string offset factor representation.</param>
    /// <param name="pArray">Es: Valor conjunto no administrado alocador target a incrustar y registrar por buffer map de matriz de tamaño array. <br />En: Format map target raw sequence string chunk array layout allocator unmanaged embedded matrix list field constraint payload element setup data block value setup source property form vector variable marker configuration element struct format.</param>
    public static void WriteArray<T>(this Span<byte> pBuffer, ref int pOffset, T[] pArray)
        where T : unmanaged
    {
        BufferWriter.AddArray<T>(pBuffer, ref pOffset, pArray);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Agrega una instancia pura desprotegida por el offset al span general original sin mutarlo. <br />
    /// ___________________( English )___________________<br />
    /// Plugs primitive layout map core off bounds by given padding marker on standard static unmodified preserving generic size span field payload string. <br />
    /// </summary>
    /// <param name="pBuffer">Es: Capacidad general reservada en el contexto original inmutable. <br />En: Immutably preserved capacity context layout bounds size span string format payload reference pointer limit source mapping.</param>
    /// <param name="pOffset">Es: Avance marcador en referencia. <br />En: Referenced shifting padding pointer marker factor element.</param>
    /// <param name="pValue">Es: El valor empaquetado no manejado objetivo. <br />En: Target managed plain chunk constraint primitive form parameter.</param>
    public static void Write<T>(this Span<byte> pBuffer, ref int pOffset, T pValue)
        where T : unmanaged
    {
        BufferWriter.Add<T>(pBuffer, ref pOffset, pValue);
    }
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Vincula extension referenciada por indice al offset para tipos estructurados con interface especial. <br />
    /// ___________________( English )___________________<br />
    /// Tethers specific index pointing shift markers mapping custom interfaced structure chunk formats directly mapping. <br />
    /// </summary>
    /// <param name="pBuffer">Es: Vector Span limitante en entorno matriz original principal persistente. <br />En: Bounded general underlying static preserving core Span map limit.</param>
    /// <param name="pOffset">Es: Señal referencia offset apuntador del padding tamaño. <br />En: Padding sequence shifting pointing reference map string flag offset value marker pointer format variable source param locator locator flag structure variable ref index setup.</param>
    /// <param name="pStruct">Es: Layout envoltorio serializable estático. <br />En: Serializable map wrapping form factor pattern element interface generic mapping format bounds.</param>
    public static void WriteStruct<T>(this Span<byte> pBuffer, ref int pOffset, T pStruct)
        where T : struct, IStructSerializable
    {
        BufferWriter.AddStruct<T>(pBuffer, ref pOffset, pStruct);
    }
}