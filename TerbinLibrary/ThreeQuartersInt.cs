using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace TerbinLibrary;
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


// TODO: 
/*
    : IComparable,
        IConvertible,
        ISpanFormattable,
        IComparable<ThreeQuartersInt>,
        IEquatable<ThreeQuartersInt>,
        IBinaryInteger<ThreeQuartersInt>,
        IMinMaxValue<ThreeQuartersInt>,
        ISignedNumber<ThreeQuartersInt>,
        IUtf8SpanFormattable, // Creo que no, creo
        IBinaryIntegerParseAndFormatInfo<ThreeQuartersInt>
 */


/// <summary>
/// ___________________( Español )___________________<br />
/// Representa un número entero no completamente estandar, optimizado para ocupar exactamente 3 bytes en memoria.<br />
/// Es de gran utilidad para estructuras de datos compactas, reduciendo su huella de memoria con respecto a un entero regular de 32 bits.<br />
/// Notas: Implementa IConvertible para asegurar conversión con otros tipos numéricos básicos.<br />
/// Tips: Recomendado si se manejan listas grandes de elementos que pueden encajar en 24 bits y no rebasan este límite.<br />
/// ___________________( English )___________________<br />
/// Represents a non-strictly standard integer, optimized to occupy exactly 3 bytes in memory.<br />
/// Highly useful for compact data structures, reducing memory footprint over a regular 32-bit integer.<br />
/// Notes: Implements IConvertible to ensure conversion with other basic numerical types.<br />
/// Tips: Recommended when handling large arrays of elements that can fit in 24 bits and do not exceed this limit.<br />
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ThreeQuartersInt : IConvertible, IMinMaxValue<ThreeQuartersInt>
{
    private byte _byte1;
    private byte _byte2;
    private byte _byte3;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// El máximo valor posible que se puede representar con 24 bits (0xFF_FF_FF).<br />
    /// ___________________( English )___________________<br />
    /// The maximum possible value that can be represented with 24 bits (0xFF_FF_FF).<br />
    /// </summary>
    public const int MaxValue = 0xFF_FF_FF;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// El menor valor posible que se puede representar (0x0).<br />
    /// ___________________( English )___________________<br />
    /// The lowest possible value that can be represented (0x0).<br />
    /// </summary>
    public const int MinValue = 0x0;

    static ThreeQuartersInt IMinMaxValue<ThreeQuartersInt>.MaxValue => MaxValue;

    static ThreeQuartersInt IMinMaxValue<ThreeQuartersInt>.MinValue => MinValue;

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Conversión implícita de entero estándar (32 bits) a este tipo de 24 bits.<br />
    /// Notas: Los bits más significativos por encima del bit 24 se perderán silenciosamente.<br />
    /// ___________________( English )___________________<br />
    /// Implicit conversion from a standard (32-bit) integer to this 24-bit type.<br />
    /// Notes: Most significant bytes beyond the 24th bit will be silently lost.<br />
    /// </summary>
    /// <param name="pValue">Es: El valor entero a convertir. <br />En: The integer value to convert.</param>
    public static implicit operator ThreeQuartersInt(int pValue)
    {
        ThreeQuartersInt result = new ThreeQuartersInt();

        result._byte1 = (byte)(pValue & 0xFF);
        result._byte2 = (byte)((pValue >> 8) & 0xFF);
        result._byte3 = (byte)((pValue >> 16) & 0xFF);

        return result;
    }
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Conversión implícita de entero sin signo (32 bits) a este tipo de 24 bits.<br />
    /// ___________________( English )___________________<br />
    /// Implicit conversion from an unsigned (32-bit) integer to this 24-bit type.<br />
    /// </summary>
    /// <param name="pValue">Es: El valor sin signo a convertir. <br />En: The unsigned value to convert.</param>
    public static implicit operator ThreeQuartersInt(uint pValue)
    {
        ThreeQuartersInt result = new ThreeQuartersInt();

        result._byte1 = (byte)(pValue & 0xFF);
        result._byte2 = (byte)((pValue >> 8) & 0xFF);
        result._byte3 = (byte)((pValue >> 16) & 0xFF);

        return result;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Conversión implícita de este tipo de 24 bits a un entero nativo de 32 bits.<br />
    /// ___________________( English )___________________<br />
    /// Implicit conversion from this 24-bit type to a native 32-bit integer.<br />
    /// </summary>
    /// <param name="pValue">Es: La estructura a convertir. <br />En: The structure object to convert.</param>
    public static implicit operator int(ThreeQuartersInt pValue)
    {
        return pValue._byte1 | (pValue._byte2 << 8) | (pValue._byte3 << 16);
    }
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Conversión implícita de este tipo a un entero sin signo de 32 bits.<br />
    /// ___________________( English )___________________<br />
    /// Implicit conversion from this type to a 32-bit unsigned integer.<br />
    /// </summary>
    /// <param name="pValue">Es: La estructura a convertir. <br />En: The structure object to convert.</param>
    public static implicit operator uint(ThreeQuartersInt pValue)
    {
        return (uint)(pValue._byte1 | (pValue._byte2 << 8) | (pValue._byte3 << 16));
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Conversión implícita de este tipo a la estructura Index.<br />
    /// Notas: Útil para usar en rangos e índices de arrays sin conversión manual previa.<br />
    /// ___________________( English )___________________<br />
    /// Implicit conversion from this type to the Index structure.<br />
    /// Notes: Useful for usage with ranges and array indexing lacking previous manual casting.<br />
    /// </summary>
    /// <param name="pValue">Es: La estructura a ser convertida como índice. <br />En: The structure to convert into an index.</param>
    public static implicit operator Index(ThreeQuartersInt pValue)
    {
        int intValue = pValue._byte1 | (pValue._byte2 << 8) | (pValue._byte3 << 16);
        return new Index(intValue);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Convierte la estructura a un array encapsulando los 3 bytes internos.<br />
    /// ___________________( English )___________________<br />
    /// Converts the structure into an array encapsulating the 3 internal bytes.<br />
    /// </summary>
    /// <returns>Es: Un arreglo de bytes con el contenido. <br />En: A byte array containing its internal values.</returns>
    public readonly byte[] ToArray()
    {
        return new byte[]{ _byte1, _byte2, _byte3 };
    }
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene la representación en formato de texto basada en el valor entero equivalente.<br />
    /// ___________________( English )___________________<br />
    /// Gets the text representation based on its equivalent integer value.<br />
    /// </summary>
    /// <returns>Es: Representación del número en string. <br />En: String representation of the number.</returns>
    public readonly override string ToString()
    {
        return ((int)this).ToString();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Obtiene el código de tipo (Int32 para compatibilidad).<br />
    /// ___________________( English )___________________<br />
    /// Gets the TypeCode (Int32 for compatibility).<br />
    /// </summary>
    public readonly TypeCode GetTypeCode() => TypeCode.Int32;
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Booleano.<br />
    /// ___________________( English )___________________<br />
    /// Converts to Boolean.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly bool ToBoolean(IFormatProvider? pProvider) => Convert.ToBoolean((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Byte.<br />
    /// ___________________( English )___________________<br />
    /// Converts to Byte.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly byte ToByte(IFormatProvider? pProvider) => Convert.ToByte((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Caracter.<br />
    /// ___________________( English )___________________<br />
    /// Converts to Char.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly char ToChar(IFormatProvider? pProvider) => Convert.ToChar((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a DateTime.<br />
    /// ___________________( English )___________________<br />
    /// Converts to DateTime.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly DateTime ToDateTime(IFormatProvider? pProvider) => Convert.ToDateTime((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Decimal.<br />
    /// ___________________( English )___________________<br />
    /// Converts to Decimal.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly decimal ToDecimal(IFormatProvider? pProvider) => Convert.ToDecimal((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Double.<br />
    /// ___________________( English )___________________<br />
    /// Converts to Double.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly double ToDouble(IFormatProvider? pProvider) => Convert.ToDouble((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Entero Corto (16 bits).<br />
    /// ___________________( English )___________________<br />
    /// Converts to Int16.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly short ToInt16(IFormatProvider? pProvider) => Convert.ToInt16((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Entero (32 bits).<br />
    /// ___________________( English )___________________<br />
    /// Converts to Int32.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly int ToInt32(IFormatProvider? pProvider) => (int)this;
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Entero Largo (64 bits).<br />
    /// ___________________( English )___________________<br />
    /// Converts to Int64.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly long ToInt64(IFormatProvider? pProvider) => Convert.ToInt64((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Byte con signo.<br />
    /// ___________________( English )___________________<br />
    /// Converts to SByte.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly sbyte ToSByte(IFormatProvider? pProvider) => Convert.ToSByte((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Single (float).<br />
    /// ___________________( English )___________________<br />
    /// Converts to Single (float).<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly float ToSingle(IFormatProvider? pProvider) => Convert.ToSingle((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Convierte el valor a cadena con formatos específicos.<br />
    /// ___________________( English )___________________<br />
    /// Converts the string to a formatted version.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly string ToString(IFormatProvider? pProvider) => ((int)this).ToString(pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Convierte y devuelve objeto en base a un Type.<br />
    /// ___________________( English )___________________<br />
    /// Converts and returns an object casting towards Type.<br />
    /// </summary>
    /// <param name="pConversionType">Es: El tipo a convertir. <br />En: Cast destination type.</param>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly object ToType(Type pConversionType, IFormatProvider? pProvider) => Convert.ChangeType((int)this, pConversionType, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Entero sin signo Corto (16 bits).<br />
    /// ___________________( English )___________________<br />
    /// Converts to UInt16.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly ushort ToUInt16(IFormatProvider? pProvider) => Convert.ToUInt16((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Entero sin signo (32 bits).<br />
    /// ___________________( English )___________________<br />
    /// Converts to UInt32.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly uint ToUInt32(IFormatProvider? pProvider) => Convert.ToUInt32((int)this, pProvider);
    
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Realiza conversión a Entero sin signo Largo (64 bits).<br />
    /// ___________________( English )___________________<br />
    /// Converts to UInt64.<br />
    /// </summary>
    /// <param name="pProvider">Es: Proveedor de formatos. <br />En: Format provider.</param>
    public readonly ulong ToUInt64(IFormatProvider? pProvider) => Convert.ToUInt64((int)this, pProvider);
}