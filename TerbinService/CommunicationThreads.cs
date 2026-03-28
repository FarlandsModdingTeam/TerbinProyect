using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TerbinLibrary.Communication;

namespace TerbinService.Communication;
// TODO: Instalar BepiEx.
// ├─Crear Tuberia y Encabezado.
// ├─Usar Reflexión para a las clases y metodos sin tener un swich enorme.
// └─Intentar instalar BepiEx.

public interface ITerbinExecutable
{
    static abstract void Init();
}

public class CommunicationThreads : ITerbinExecutable
{
    public static void Init()
    {
        _ = createPipe();
    }

    private static async Task createPipe()
    {
        while (true)
        {
            using var pipe = new NamedPipeServerStream(
                    "TerbinPipe",
                    PipeDirection.In | PipeDirection.Out,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

            await pipe.WaitForConnectionAsync();

            _ = Task.Run(() => createPipe());
            _ = Task.Run(() => handleClient(pipe));
        }
    }

    private static async Task handleClient(NamedPipeServerStream pPipe)
    {
        //_ = read(pPipe);
        //_ = send(pPipe);


    }

    private static async Task read(NamedPipeServerStream pServer)
    {
        /*
         using var reader = new StreamReader(pServer);

        while (true)
        {

        }
         */
    }

    private static async Task send(NamedPipeServerStream pServer)
    {
        //using var writer = new StreamWriter(pServer);
        //writer.AutoFlush = true;
        // pWriter.WriteAsync();
    }
}

