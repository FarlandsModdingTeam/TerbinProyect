using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Useful;

public struct TerbinInfoProgrss
{
    public long Current;
    public byte Percentage; // 0 => 100
    public bool Finish; // alert to release

    public readonly byte[] ToArray()
    {
        return Serialize();
    }

    public readonly byte[] Serialize()
    {
        byte[] array = new Serialineitor()
            .Add(Percentage)
            .Add(Current)
            .Add(Finish)
            .Serialize();
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

    public static double? GetInverse(long? pTotal)
    {
       return (pTotal.HasValue) ? (100.0d / pTotal.Value) : null;
    }
    public static double GetInverse(long pTotal)
    {
        return (100.0d / pTotal);
    }
}
