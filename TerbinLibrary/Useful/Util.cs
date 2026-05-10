using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Communication;
using TerbinLibrary.Serialize;

namespace TerbinLibrary.Useful;

public struct TerbinInfoProgrss
{
    public byte Percentage; // 0 => 100
    public long Current;
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


    public static IProgress<TerbinInfoProgrss> CreateProgessBarr(
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
    public static IProgress<TerbinInfoProgrss> CreateProgessBarr(
        TerbinCommunicator pCommunicator, ushort pIdRequest, Action<TerbinInfoProgrss>? pAction = default, params byte[] pMethod)
    {
        if (pMethod.Length <= 0)
            throw new OverflowException($"¡No Action send!");

        byte method = pMethod[0];
        byte[] restMethod = pMethod[1..];
        byte[] id = Serialineitor.Serialize(pIdRequest);
        return new Progress<TerbinInfoProgrss>(p =>
        {
            pAction?.Invoke(p);
            byte[] pld = Serialineitor.Splice(restMethod, p.Serialize());
            _ = pCommunicator.Send(method, pld);
            _ = pCommunicator.Send((byte)CodeTerbinProtocol.Prolong, id);
        });
    }
}
