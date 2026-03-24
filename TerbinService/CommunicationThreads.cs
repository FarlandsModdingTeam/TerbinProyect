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
        server.WaitForConnection();

        using var reader = new StreamReader(server);
        using var writer = new StreamWriter(server);
        writer.AutoFlush = true;

        while (true)
        {
            //var msg = reader.ReadLine();
        }
    }

    internal async void send(StreamWriter pWriter)
    {
        // pWriter.WriteAsync();
    }
}

