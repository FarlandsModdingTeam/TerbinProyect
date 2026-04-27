using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Serialize;

namespace TerbinService.Useful;

public struct TerbinInfoProgrss
{
    [Obsolete]
    public byte[] Content;

    public long Current;
    public byte Percentage; // 0 => 100
    public bool Last; // alert to release

    [Obsolete]
    public static TerbinInfoProgrss CreateLast(byte[] pContent)
    {
        return new TerbinInfoProgrss
        {
            Last = true,
            Content = pContent,
        };
    }

    [Obsolete]
    public static TerbinInfoProgrss Create(byte[] pContent, bool Last = false)
    {
        return new TerbinInfoProgrss
        {
            Last = Last,
            Content = pContent,
        };
    }

    public readonly byte[] ToArray()
    {
        byte[] array = new Serialineitor()
            .Add(Percentage)
            .Add(Current)
            .Add(Last)
            .ToArray();
        return array;
    }
}

public static class Util
{
    /// <summary>
    /// Calcula y reporta el porcentaje de progreso de la operación.
    /// </summary>
    /// <param name="pCurrentRead">Cantidad total actual de bytes leídos.</param>
    /// <param name="pTotalInverse">Cantidad total esperada de bytes de multiplicacion inversa.</param>
    /// <param name="pProgress">
    /// Objeto opcional para reportar el progreso.
    /// </param>
    /// <remarks>
    /// Si el tamaño total es desconocido o no se proporcionó un
    /// objeto de progreso, no se reporta nada.
    /// </remarks>
    public static void ReportProgressPercent(long pCurrentRead, double? pTotalInverse, IProgress<TerbinInfoProgrss>? pProgress, bool pLast, ref int pPrevouslyReported)
    {
        if (!pTotalInverse.HasValue || pProgress == null)
            return;

        int percent = (int)(pCurrentRead * pTotalInverse.Value);

        if (percent > pPrevouslyReported)
        {
            pPrevouslyReported = percent;
            var info = new TerbinInfoProgrss
            {
                Percentage = (byte)percent,
                Current = pCurrentRead,
                Last = pLast,
            };
            pProgress.Report(info);
        }
    }


    public static double? GetInverse(long? pTotal)
    {
       return (pTotal.HasValue) ? (100.0d / pTotal.Value) : null;
    }
}
