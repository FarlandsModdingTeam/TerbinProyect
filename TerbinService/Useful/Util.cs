using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Serialize;

namespace TerbinService.Useful;

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
    public static void ReportProgress(long pCurrentRead, long? pTotal, double? pTotalInverse, IProgress<byte[]>? pProgress, ref int pPrevouslyReported)
    {
        if (!pTotalInverse.HasValue || !pTotal.HasValue || pProgress == null)
            return;

        int percent = (int)(pCurrentRead * pTotalInverse.Value);

        if (percent > pPrevouslyReported)
        {
            pPrevouslyReported = percent;
            byte[] m = [(byte)percent, .. Serialineitor.Serialize(pTotal.Value)];
            pProgress.Report(m);
        }
    }
}
