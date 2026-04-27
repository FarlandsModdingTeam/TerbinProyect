using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Serialize;

namespace TerbinService.Useful;

public struct TerbinInfoProgrss
{
    public byte[] Content;
    public bool Last;

    public static TerbinInfoProgrss CreateLast(byte[] pContent)
    {
        return new TerbinInfoProgrss
        {
            Last = true,
            Content = pContent,
        };
    }

    public static TerbinInfoProgrss Create(byte[] pContent, bool Last = false)
    {
        return new TerbinInfoProgrss
        {
            Last = Last,
            Content = pContent,
        };
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
    public static void ReportProgress(long pCurrentRead, double? pTotalInverse, IProgress<TerbinInfoProgrss>? pProgress, bool last, ref int pPrevouslyReported)
    {
        if (!pTotalInverse.HasValue || pProgress == null)
            return;

        int percent = (int)(pCurrentRead * pTotalInverse.Value);

        if (percent > pPrevouslyReported)
        {
            pPrevouslyReported = percent;
            byte[] bytesTotal = Serialineitor.Serialize<long>(pCurrentRead);
            byte[] m = [(byte)percent, .. bytesTotal];
            pProgress.Report(TerbinInfoProgrss.Create(m, last));
        }
    }


    public static double? GetInverse(long? pTotal)
    {
       return (pTotal.HasValue) ? (100.0d / pTotal.Value) : null;
    }
}
