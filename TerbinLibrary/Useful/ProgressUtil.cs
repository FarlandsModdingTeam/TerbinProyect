using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Useful;
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
/// Estructura que almacena la información de progreso.<br />
/// ___________________( English )___________________<br />
/// Structure that stores progress information.<br />
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TerbinInfoProgrss : IStructSerializable
{
    public byte Percentage; // 0 => 100
    public long Current;
    public bool Finish; // alert to release

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Convierte la estructura a un arreglo de bytes.<br />
    /// ___________________( English )___________________<br />
    /// Converts the structure to a byte array.<br />
    /// </summary>
    /// <returns>Es: Un arreglo de bytes. <br />En: A byte array.</returns>
    public readonly byte[] ToArray()
    {
        return Serialize();
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Serializa los datos de la estructura empleando Serialineitor.<br />
    /// ___________________( English )___________________<br />
    /// Serializes the data of the structure using Serialineitor.<br />
    /// </summary>
    /// <returns>Es: Arreglo de bytes resultante de serializar los campos. <br />En: Byte array resulting from serializing the fields.</returns>
    public readonly byte[] Serialize()
    {
        byte[] array = new Serialineitor()
            .Add(Percentage)
            .Add(Current)
            .Add(Finish)
            .Serialize();
        return array;
    }

    public readonly int GetSize() => 10;

    public void WriteTo(Span<byte> pBuffer)
    {
        // A dios Rezo.
        pBuffer = Serialize();
    }

    public void ReadFrom(ReadOnlySpan<byte> pBuffer)
    {
        int offset = 0;
        Percentage = pBuffer.Read<byte>(ref offset);
        Current = pBuffer.Read<long>(ref offset);
        Finish = pBuffer.Read<bool>(ref offset);
    }
}

/// <summary>
/// ___________________( Español )___________________<br />
/// Clase estática que provee diversas utilidades de soporte al sistema.<br />
/// ___________________( English )___________________<br />
/// Static class that provides various system support utilities.<br />
/// </summary>
public static class ProgressUtil
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Intenta reportar y calcular el porcentaje de progreso en una operación asíncrona.<br />
    /// Notas: Si el parámetro es nulo, no se realiza acción alguna.<br />
    /// ___________________( English )___________________<br />
    /// Attempts to calculate and report the progress percentage of an asynchronous operation.<br />
    /// Notes: If the parameter is null, no action is taken.<br />
    /// </summary>
    /// <param name="pCurrentRead">Es: Estado actualmente procesado (leído). <br />En: Currently processed state (read).</param>
    /// <param name="pTotalInverse">Es: Total esperado expresado de forma inversa porcentualmente. <br />En: Given total expected in an inverse percentage ratio.</param>
    /// <param name="pProgress">Es: Objeto para reportar progresos. <br />En: Progress reporter object.</param>
    /// <param name="pFinish">Es: Refleja si el rastreo ha finalizado o debe concluirse. <br />En: Reflects if tracing has finished or ends.</param>
    /// <param name="pPrevouslyReported">Es: Porcentaje reportado anteriormente. <br />En: Previously reported percentage.</param>
    /// <returns>Es: Verdadero en caso exitoso. <br />En: True if successful.</returns>
    public static bool TryReportProgressPercent(long pCurrentRead, double? pTotalInverse, IProgress<TerbinInfoProgrss>? pProgress, bool pFinish, ref int pPrevouslyReported)
    {
        if (!pTotalInverse.HasValue || pProgress == null)
            return false;

        int percent = (int)(pCurrentRead * pTotalInverse.Value);

        if (percent > pPrevouslyReported)
        {
            pPrevouslyReported = percent;
            ReportProgressPercent(percent, pCurrentRead, pFinish, pProgress);
            return true;
        }
        return false;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Reporta un porcentaje concreto de progreso a un objeto monitor asíncrono.<br />
    /// ___________________( English )___________________<br />
    /// Reports a specific progress percentage to an asynchronous monitor object.<br />
    /// </summary>
    /// <param name="pPercent">Es: Avance reportado. <br />En: Advanced reported.</param>
    /// <param name="pCurrentRead">Es: Capacidad actual. <br />En: Current capacity.</param>
    /// <param name="pFinish">Es: Indicador limitante del fin. <br />En: Bounding end indicator.</param>
    /// <param name="pProgress">Es: Objeto emisor. <br />En: Emitting object.</param>
    public static void ReportProgressPercent(int pPercent, long pCurrentRead, bool pFinish, IProgress<TerbinInfoProgrss> pProgress)
    {
        var info = new TerbinInfoProgrss
        {
            Percentage = (byte)pPercent,
            Current = pCurrentRead,
            Finish = pFinish,
        };
        pProgress.Report(info);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Calcula el inverso escalar basándose en una cantidad total opcional.<br />
    /// ___________________( English )___________________<br />
    /// Calculates the scalar inverse based on an optional total amount.<br />
    /// </summary>
    /// <param name="pTotal">Es: Total esperado (escalar base). <br />En: Expected total (base scalar).</param>
    /// <returns>Es: Resultado inverso porcentual o nulo si no hay valor total. <br />En: Percentage inverse result or null if total has no value.</returns>
    public static double? GetInverse(long? pTotal)
    {
       return (pTotal.HasValue) ? (100.0d / pTotal.Value) : null;
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Calcula el inverso escalar basándose en una cantidad total asegurada.<br />
    /// ___________________( English )___________________<br />
    /// Calculates the scalar inverse based on an assured total amount.<br />
    /// </summary>
    /// <param name="pTotal">Es: Total esperado asegurado. <br />En: Assured expected total.</param>
    /// <returns>Es: Multiplicador inverso de progreso porcentual. <br />En: Progress inverse percentage multiplier.</returns>
    public static double GetInverse(long pTotal)
    {
        return (100.0d / pTotal);
    }



    public static IProgress<TerbinInfoProgrss> CreateProgressAndSetMax
        (TerbinCommunicator pCommunicator, IStructSerializable pMax, ushort pIdRequest, params byte[] pMethod)
    {
        IdArray idMax = new IdArray(pMethod, (byte)CodeServicesClient.SetMaxProgress);
        IdArray idSet = new IdArray(pMethod, (byte)CodeServicesClient.SetBarProgress);

        ProgressUtil.SendAndProlong(pCommunicator, pMax, pIdRequest, idMax);
        return ProgressUtil.CreateProgessBarr(pCommunicator, pIdRequest, pMethod: idSet);
    }



    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea un objeto de seguimiento de progreso dirigido directo a memoria, para enviar usando el protocolo respectivo.<br />
    /// ___________________( English )___________________<br />
    /// Creates a progress tracking object aimed straight at memory, to be sent using proper protocol routines.<br />
    /// </summary>
    /// <param name="pCommunicator">Es: Clase delegada para el tráfico web. <br />En: Delegate class for network traffic.</param>
    /// <param name="pIdMemory">Es: Identificador de posición en memoria. <br />En: Storage array memory id pointer.</param>
    /// <param name="pAction">Es: Tarea auxiliar o delegada de registro. <br />En: Registry side utility action delegate.</param>
    /// <returns>Es: Objeto emisor provisto con esta operación. <br />En: Broadcaster object bundled into this function.</returns>
    public static IProgress<TerbinInfoProgrss> CreateProgessBarrForMemory(
        TerbinCommunicator pCommunicator, byte pIdMemory, Action<TerbinInfoProgrss>? pAction = default)
    {
        if (pIdMemory <= TerbinProtocol.RESERVE_MEMORY)
            throw new OverflowException($"Id memory is reserved! {pIdMemory}");
        return new Progress<TerbinInfoProgrss>(p =>
        {
            pAction?.Invoke(p);
            _ = pCommunicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemory, p.Serialize());
        });
    }

    // Prolongar cuando actulizamos el progreso no es la mejor solucion pero si para salir del paso (TODO:).
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Crea un rastreador de progreso enviando eventos generalizados al comunicador en curso prolongado.<br />
    /// ___________________( English )___________________<br />
    /// Creates a progress tracker projecting widespread events to the prolongable current communications line.<br />
    /// </summary>
    /// <param name="pCommunicator">Es: El despachador general. <br />En: General dispatcher endpoint relay.</param>
    /// <param name="pIdRequest">Es: Marcador y etiqueta de solicitud entrante. <br />En: Inbound trace and token label.</param>
    /// <param name="pAction">Es: Ejecutador opcional sobre-asignado. <br />En: Optional overloader trigger event.</param>
    /// <param name="pMethod">Es: Especificaciones serializadas del identificador de matriz. <br />En: Identifier sequence serialized details.</param>
    /// <returns>Es: Objeto genérico Progress preparado. <br />En: Progress generic initialized interface object.</returns>
    public static IProgress<TerbinInfoProgrss> CreateProgessBarr(
        TerbinCommunicator pCommunicator, ushort pIdRequest, Action<TerbinInfoProgrss>? pAction = default, params byte[] pMethod)
    {
        if (pMethod.Length <= 0)
            throw new OverflowException($"¡No Action send!");

        return CreateProgessBarr(pCommunicator, pIdRequest, new IdArray(pMethod), pAction);
    }

    public static IProgress<TerbinInfoProgrss> CreateProgessBarr(
        TerbinCommunicator pCommunicator, ushort pIdRequest, IdArray pMethod, Action<TerbinInfoProgrss>? pAction = default)
    {
        byte[] id = Serialineitor.Serialize(pIdRequest);
        return new Progress<TerbinInfoProgrss>(p =>
        {
            pAction?.Invoke(p);
            _ = pCommunicator.Send(pMethod, p.Serialize());
            _ = pCommunicator.Send(new IdArray((byte)CodeTerbinProtocol.Prolong), id);
        });
    }


    public static void SendAndProlong(TerbinCommunicator pCommunicator, IStructSerializable pData, ushort pIdRequest, IdArray pMethod)
    {
        byte[] id = Serialineitor.Serialize(pIdRequest);
        _ = pCommunicator.Send(pMethod, pData.Serialize());
        _ = pCommunicator.Send(new IdArray((byte)CodeTerbinProtocol.Prolong), id);
    }




    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Utilidad interna para agilizar funciones que posibilitan reportes de errores más descriptivos a las bitácoras.<br />
    /// ___________________( English )___________________<br />
    /// Internal utility module easing tracing capabilities towards an advanced descriptive reporting logic.<br />
    /// </summary>
    public class DebugTerbinLibrary
    {
        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Imprime los valores guardados en una lista matriz serializada dentro de un texto, separados por coma.<br />
        /// ___________________( English )___________________<br />
        /// Overlooks elements bundled on arrays putting them inline delimited by commas string form.<br />
        /// </summary>
        /// <typeparam name="T">Es: Tipado dinámico matriz original. <br />En: Typing bounds base sequence schema.</typeparam>
        /// <param name="pArray">Es: Parámetros iterados extraídos de dicha iteración. <br />En: Extracted nested param iterating object arrays data.</param>
        /// <returns>Es: Visualizador crudo del dato. <br />En: Raw string debugger format output.</returns>
        public static string ArrayToString<T>(params T[] pArray)
        {
            string data = "";
            for (int i = 0; i < pArray.Length; i++)
            {
                data += pArray[i];
                if ((i + 1) < pArray.Length)
                    data += ",";
            }
            return data;
        }
    }
}
