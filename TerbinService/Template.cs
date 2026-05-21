using System;
using System.Collections.Generic;
using TerbinLibrary.Extension;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Execution;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.Protocol;
using System.Collections;

namespace TerbinService;
/*
 -- Variables:
  empieza: _ = es privada NO local.
  empieza: minuscula = es privada local.
  empieza: "p"en minuscula = parametro entrante local.
  empieza: mayuscula = publica.
 -- Funciones:
  empieza: mayusculas = publica.
  empieza: minusculas = privada.
 */

internal partial class Template
{
    // [TerbinExecutable(CodeServices.WIP_NewService)]
    public static async Task<InfoResponse?> TemplateMethod(Header pHead, byte[] pParameters, CancellationToken pToken)
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


#if false
    // Creo que tiene potencial, hay que dejarla madurar, ahora tengo prisa.
    // [TerbinExecutable(CodeServices.WIP_NewService)]
    public static async IAsyncEnumerable<IInfo?> TemplateMethod_new(Header pHead, byte[] pParameters)
    {
        // Comprobaciones.
        if (pParameters.Length <= 0)
        {
            yield return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);
            yield break;
        }

        // Leer.
        ReadOnlySpan<byte> buffer = pParameters;
        string name = buffer.ReadArray<char>().CrString();
        string dir = buffer.ReadArray<char>().CrString();


        // Solicitar id memoria.
        InfoCommunicateResponse r;
        yield return InfoCommunicate.SoliciteRequestMemory(ref r);
        PacketRequest rIdB = await r.GetResult();
        if (rIdB.Head.Status != CodeStatus.Succes)
            yield return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(CodeInternalErrors.IdSoliciteError));
        byte id = rIdB.Payload[0];

        // Responder.
        yield return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = [],
        };
        yield break;
    }
#endif
}
