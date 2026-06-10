using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary.Protocol;

namespace SimulateClient;

static class Helper
{
    public static async Task<bool> IsError(CodeStatus pStatus)
    {
        if (pStatus != CodeStatus.Succes)
        {
            Console.WriteLine($"Error: {pStatus}");
            await PressAnyKeyToContinue();
            return true;
        }
        return false;
    }
    public static async Task PressAnyKeyToContinue()
    {
        Console.WriteLine($"[Client] Pulse cualquier tecla para continuar ...");
        _ = Console.ReadLine();
        await Task.Delay(500);
    }
}
