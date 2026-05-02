using System;
using System.Collections.Generic;
using TerbinLibrary.Extension;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Serialize;

namespace TerbinService;

internal partial class Template
{
    //[TerbinExecutableCompound((byte)CodeTerbinProtocol.Create, (byte)CodeSubServices.Game)]
    //[TerbinExecutable((byte)CodeServices.WIP_NewService)]
    public static async Task<InfoResponse?> TemplateMethod(Header pHead, byte[] pParameters)
    {
        // Comprobaciones.
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        // Leer.
        ReadOnlySpan<byte> buffer = pParameters;
        string name = buffer.ReadArray<char>().CrString();
        string dir = buffer.ReadArray<char>().CrString();

        // AmongInfoThreads.
        AmongInfoThreads info = Worker.CurrentConst.Value;

        // Solicitar id memoria.
        var rIdB = await info.Communicator.SoliciteRequestMemory();
        if (rIdB.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        byte id = rIdB.Payload[0];

        // Responder.
        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = [],
        };
    }
}
