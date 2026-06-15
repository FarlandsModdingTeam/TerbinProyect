using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Protocol;

namespace SimulateClient;

static class Helper
{
    public static async Task<bool> IsError(CodeStatus pStatus)
    {
        if (pStatus != CodeStatus.Succes)
        {
            //Console.WriteLine($"Error: {pStatus}");
            await PressAnyKeyToContinue();
            return true;
        }
        return false;
    }
    public static async Task<bool> IsError(PacketRequest pCapsule)
    {
        CodeStatus st;

        if (!pCapsule.IsSucces)
        {
            st = pCapsule.Head.Status;
            if (st == CodeStatus.InternalWorkerError)
            {
                ushort value = BitConverter.ToUInt16(pCapsule.Payload[0..2]);
                Console.WriteLine($"ErrorInternal: {(CodeInternalErrors)value}");
            }
            else
                Console.WriteLine($"Error: {st}");

            await PressAnyKeyToContinue();
        }

        return pCapsule.IsSucces;
    }

    public static async Task Fin()
    {
        Console.WriteLine($"[Client] ==> FIN");
        await PressAnyKeyToContinue();
    }
    public static async Task PressAnyKeyToContinue()
    {
        Console.WriteLine($"[Client] Pulse cualquier tecla para continuar ...");
        _ = Console.ReadLine();
        await Task.Delay(500);
    }

    // TODO: Un start y un end.
    public static string Read(string pMSG) // string pStart, string pEnd
    {
        Console.Write($"[Client] {pMSG} -> ( ");

        int startX = Console.CursorLeft;
        int startTop = Console.CursorTop;

        Console.Write(" )");
        Console.SetCursorPosition(startX, startTop);

        StringBuilder txt = new();
        bool flag = true;

        while (flag)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter)
            {
                flag = false;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (txt.Length > 0)
                    txt.Remove(txt.Length - 1, 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                txt.Append(key.KeyChar);
            }

            Console.SetCursorPosition(startX, startTop);
            Console.Write(txt.ToString() + " ) ");
            Console.SetCursorPosition(startX + txt.Length, startTop);
        }

        Console.WriteLine();

        return txt.ToString();
    }
    public static void PrintMethod(params byte[] pData)
    {
        if (pData.Length >= 3)
            Console.WriteLine($"CodeServicesClient:{(CodeServicesClient)pData[2]}");
        if (pData.Length >= 2)
            Console.WriteLine($"CodeServicesSection:{(CodeServicesSection)pData[1]}");
        if (pData.Length >= 1)
        {
            Console.WriteLine($"CodeServices:{(CodeServices)pData[0]}");
            Console.WriteLine($"CodeTerbinProtocol:{(CodeTerbinProtocol)pData[0]}");
        }
    }
}
