using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using TerbinLibrary.Communication;

namespace TerbinService.Communication;
// TODO: Instalar BepiEx.
// ├─Crear Tuberia y Encabezado.
// ├─Usar Reflexión para a las clases y metodos sin tener un swich enorme.
// └─Intentar instalar BepiEx.

public interface IServer
{
    void Init();
}

public class CommunicationThreads
{
    public static void Init()
    {
        using var server = new NamedPipeServerStream("TerbinPipe");
        while (true)
        {
            server.WaitForConnection();

            _ = read(server);
            _ = send(server);
        }
    }

    private static async Task read(NamedPipeServerStream pServer)
    {
        using var reader = new StreamReader(pServer);

        char[] a = [];
        var r = await reader.ReadBlockAsync(a, 1, 1);


    }

    private static async Task send(NamedPipeServerStream pServer)
    {
        //using var writer = new StreamWriter(pServer);
        //writer.AutoFlush = true;
        // pWriter.WriteAsync();
    }
}

