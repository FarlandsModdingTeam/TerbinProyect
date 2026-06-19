using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;
using TerbinService.Managers;

namespace TerbinService.Services;

internal static class ServiceGames
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio encargado de clonar o copiar un juego dentro de una instancia especificada.<br />
    /// Deserializa secuencialmente de los parámetros: el nombre de la instancia, el directorio del juego y, opcionalmente, una bandera para usar reporte de progreso.<br />
    /// Notas: Si se activa el progreso, se calculará el tamaño total del contenido del directorio antes de comenzar.<br />
    /// Tips: Es recomendable usar el progreso para juegos o archivos grandes y así evitar timeouts innecesarios en el cliente.<br />
    /// ___________________( English )___________________<br />
    /// Service in charge of cloning or copying a game into a specified instance.<br />
    /// Sequentially deserializes from the parameters: the instance name, the game directory, and optionally, a flag to use progress reporting.<br />
    /// Notes: If progress is enabled, the total size of the directory's content will be calculated before starting.<br />
    /// Tips: It is highly recommended to use the progress flag for large games or files to avoid unnecessary client timeouts.<br />
    /// </summary>
    /// <param name="nameInstance">Es: Nombre de la instancia destino (Obligatorio). <br />En: Name of the destination instance (Mandatory).</param>
    /// <param name="dirGame">Es: Ruta o directorio de origen del juego a clonar (Obligatorio). <br />En: Source path or directory of the game to be cloned (Mandatory).</param>
    /// <param name="useProgress">Es: Bandera booleana para habilitar el progreso por paquetes (Opcional, si hay datos restantes). <br />En: Boolean flag to enable packet-based progress (Optional, if remaining data exists).</param>
    /// <returns>
    /// Es: Un InfoResponse indicando el éxito, o un error si falta el payload o hay fallos internos. <br />
    /// En: An InfoResponse indicating success, or an error if payload is missing or internal failures occur.
    /// </returns>
    [TerbinExecutable((byte)CodeServices.Duplicate, (byte)CodeServicesSection.Game)]
    public static async Task<InfoResponse?> CloneGame(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string nameInstance = reader.ReadArray<char>().CrString();
        string dirGame = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        IProgress<TerbinInfoProgrss>? progress = null;
        if (useProgress)
        {
            long maxSize = (long)NodeUtil.CountContent(dirGame);
            MaxProgressDTO max = new(maxSize);
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin);
        }

        var result = await Manager.Games.CloneInInstance(dirGame, nameInstance, true, progress, pToken);
        if (result != InternalErrors.IsSucces)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize<ushort>((ushort)result));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio que elimina los archivos de un juego vinculado a una instancia existente.<br />
    /// Deserializa secuencialmente de los parámetros: el nombre de la instancia y, opcionalmente, si se requiere llevar un registro del progreso del borrado.<br />
    /// Notas: Validará primeramente si la carpeta de la instancia realmente existe en el sistema antes de iniciar el borrado.<br />
    /// Tips: Asegúrate de detener la instancia antes de invocar este servicio para evitar conflictos de archivos bloqueados.<br />
    /// ___________________( English )___________________<br />
    /// Service that deletes the game files linked to an existing instance.<br />
    /// Sequentially deserializes from the parameters: the instance name, and optionally, whether to keep a record of the deletion progress.<br />
    /// Notes: It will first validate if the instance folder actually exists on the system before attempting deletion.<br />
    /// Tips: Make sure to stop the instance before invoking this service to avoid locked file conflicts.<br />
    /// </summary>
    /// <param name="nameInstance">Es: Nombre de la instancia de la cual se eliminará el juego (Obligatorio). <br />En: Name of the instance from which the game will be deleted (Mandatory).</param>
    /// <param name="useProgress">Es: Bandera booleana para notificar el progreso de eliminación de archivos (Opcional, si hay datos restantes). <br />En: Boolean flag to notify file deletion progress (Optional, if remaining data exists).</param>
    /// <returns>
    /// Es: Un InfoResponse exitoso, o código de error interno si la instancia no existe o falla la eliminación. <br />
    /// En: A successful InfoResponse, or an internal error code if the instance does not exist or deletion fails.
    /// </returns>
    [TerbinExecutable((byte)CodeServices.Deleted, (byte)CodeServicesSection.Game)]
    public static async Task<InfoResponse?> DeletedGame(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string nameInstance = reader.ReadArray<char>().CrString();
        bool useProgress = (reader.Length >= 1) && reader.Read<bool>();

        string? dirGame = Manager.Instances.GetPathFolder(nameInstance);
        if (string.IsNullOrEmpty(dirGame))
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.InstanceNotExist));

        IProgress<TerbinInfoProgrss>? progress = null;
        if (useProgress)
        {
            long maxSize = (long)NodeUtil.CountContent(dirGame);
            MaxProgressDTO max = new(maxSize);
            progress = ProgressUtil.CreateProgressAndSetMax
                (Worker.CurrentContext.Value.Communicator, max, pHead.IdRequest, (byte)CodeServices.Dowload, (byte)CodeServicesSection.Plugin);
        }

        var result = await Manager.Games.RemoveInInstance(nameInstance, progress, pToken);
        if (result != InternalErrors.IsSucces)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize<ushort>((ushort)result));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio destinado a la ejecución o arranque de un juego configurado en una instancia.<br />
    /// Únicamente requiere y deserializa el nombre de la instancia desde los parámetros enviados.<br />
    /// Notas: Depende de que el mánager de juegos valide internamente que los binarios estén presentes.<br />
    /// Tips: Controla los tiempos de respuesta del cliente en caso de que el juego tarde en reportar su inicio al sistema operativo.<br />
    /// ___________________( English )___________________<br />
    /// Service intended for the execution or startup of a game configured in an instance.<br />
    /// It only requires and deserializes the instance name from the sent parameters.<br />
    /// Notes: It relies on the game manager internally validating that the binaries are present.<br />
    /// Tips: Manage client response times in case the game takes a while to report its startup to the OS.<br />
    /// </summary>
    /// <param name="nameInstance">Es: Nombre de la instancia que contiene el juego a arrancar (Obligatorio). <br />En: Name of the instance containing the game to start (Mandatory).</param>
    /// <returns>
    /// Es: Retorna éxito si el proceso arranca, o un código de error si el manejador falla al inicializarlo. <br />
    /// En: Returns success if the process starts, or an error code if the manager fails to initialize it.
    /// </returns>
    [TerbinExecutable((byte)CodeServices.Execute, (byte)CodeServicesSection.Game)]
    public static async Task<InfoResponse?> ExecuteGame(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> buffer = pParameters;
        string nameInstance = buffer.ReadArray<char>().CrString();

        var result = await Manager.Games.RunInInstance(nameInstance);
        if (result != InternalErrors.IsSucces)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize<ushort>((ushort)result));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }
}
