using System;
using System.Collections.Generic;
using System.IO.Pipes;
using TerbinLibrary.Executables;
using TerbinLibrary.Communication;

namespace TerbinService.Communication;
// TODO: Instalar BepiEx.
// ├─Crear Tuberia y Encabezado.
// ├─Usar Reflexión para a las clases y metodos sin tener un swich enorme.
// └─Intentar instalar BepiEx.

//public interface ITerbinExecutable
//{
//    static abstract void Init();
//}

// TODO: Hacerlo como libreria.
// TODO: Movelo a TerbinLibrary.
public class CommunicationThreads
{
    public static NamedPipeServerStream NewTerbinPipe
    {
        get => CreateServerPipe();
    }
    public static NamedPipeClientStream NewClientTerbinPipe
    {
        get => CreateClientPipe();
    }

    public static NamedPipeServerStream CreateServerPipe()
    {
        return new NamedPipeServerStream(
                "TerbinPipe",
                PipeDirection.In | PipeDirection.Out,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
    }
    public static NamedPipeClientStream CreateClientPipe()
    {
        return new NamedPipeClientStream(
                ".",
                "TerbinPipe",
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
    }

    //public static async Task CreateServerThread()
    //{
    //}


    [Obsolete]
    public static void Init()
    {
        _ = createPipe();
    }

    [Obsolete]
    private static async Task createPipe()
    {
        //while (!Worker.Cts.Token.IsCancellationRequested)
        //{ }

        var pipe = new NamedPipeServerStream(
                "TerbinPipe",
                PipeDirection.In | PipeDirection.Out,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

        Console.WriteLine("[Worker] Waiting Client...");
        await pipe.WaitForConnectionAsync();

        _ = Task.Run(() => createPipe());
        //_ = Task.Run(() => handleClient(pipe));

        _ = handleClient(pipe);
    }

    [Obsolete]
    private static async Task handleClient(NamedPipeServerStream pPipe)
    {
        //_ = handleRead(pPipe);
        //_ = handleSend(pPipe);

        while (!Worker.Cts.Token.IsCancellationRequested)
        {
            // TODO: Alguna forma esperar a que mande un mensaje.

            StreamReadStruct reader = new(pPipe);
            StreamWritesStruct writer = new(pPipe);

            PacketRequest cap = await reader.ReadAsycn<PacketRequest>();
            Console.WriteLine($"[Worker] Client {cap.Head.OrderRequest}");


            var capR = await ExecutableDispatcher.DispatchAsync(cap);

            _ = writer.WriteAsycn<PacketRequest>(capR);
        }
    }

    private static async Task handleRead(NamedPipeServerStream pServer)
    {
        /*
         using var reader = new StreamReader(pServer);

        while (true)
        {

        }
         */
    }

    private static async Task handleSend(NamedPipeServerStream pServer)
    {
        //using var writer = new StreamWriter(pServer);
        //writer.AutoFlush = true;
        // pWriter.WriteAsync();
    }
}

