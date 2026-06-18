using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Instance;
using TerbinLibrary.Data.Plugin;
using TerbinLibrary.Data.Store;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.Useful;
using TerbinService.Managers;

namespace TerbinService.Services;

internal class ServicePluginStorage
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio encargado de leer y devolver la lista completa de todos los plugins guardados en el almacenamiento del sistema.<br />
    /// Construye una respuesta empaquetando un conteo inicial seguido de la estructura serializada de cada plugin.<br />
    /// Notas: A pesar de llamarse "GetOne", la lógica interna y su código de servicio corresponden a "ReadAll". Retorna un byte [0] si el almacén está vacío.<br />
    /// Tips: Ideal para actualizar el inventario o la interfaz gráfica del cliente con los plugins disponibles localmente.<br />
    /// ___________________( English )___________________<br />
    /// Service in charge of reading and returning the complete list of all plugins saved in the system's storage.<br />
    /// Builds a response packing an initial count followed by the serialized structure of each plugin.<br />
    /// Notes: Despite being named "GetOne", the internal logic and its service code correspond to "ReadAll". Returns a [0] byte if the storage is empty.<br />
    /// Tips: Ideal for updating the inventory or the client's graphical interface with locally available plugins.<br />
    /// </summary>
    /// <returns>Es: Un InfoResponse con la cantidad de plugins y la lista de ReferencePluginStoreDTO serializados.<br />En: An InfoResponse with the amount of plugins and the serialized list of ReferencePluginStoreDTO.</returns>
    // ReferencePluginStoreDTO
    [TerbinExecutable((byte)CodeServices.ReadAll, (byte)CodeServicesSection.PluginStorage)]
    public static async Task<InfoResponse?> GetOne(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        List<ReferencePluginStore> plugin;
        Serialineitor s = new();

        plugin = await Manager.StoragePlugin.GetAll();

        if (plugin.Count <= 0)
            return InfoResponse.CreateSucces(pHead.IdRequest, [0]);

        s.Add<ThreeQuartersInt>(plugin.Count);
        for (int i = 0; i < plugin.Count; i++)
            s.AddStruct<ReferencePluginStoreDTO>(plugin[i].ToDTO());

        return InfoResponse.CreateSucces(pHead.IdRequest, s.Serialize());
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio destinado a recuperar la información individual de un único plugin almacenado.<br />
    /// Extrae el identificador del payload y busca la referencia exacta en los registros de almacenamiento de plugins.<br />
    /// Notas: Aunque el método se llame "GetAll", actúa como un lector específico (Read) evaluando un único ID.<br />
    /// Tips: Controla internamente si el plugin no existe devolviendo un error de tipo PluginNotExist.<br />
    /// ___________________( English )___________________<br />
    /// Service intended to retrieve the individual information of a single stored plugin.<br />
    /// Extracts the identifier from the payload and searches for the exact reference in the plugin storage records.<br />
    /// Notes: Even though the method is named "GetAll", it acts as a specific reader (Read) evaluating a single ID.<br />
    /// Tips: Internally handles if the plugin does not exist by returning a PluginNotExist error type.<br />
    /// </summary>
    /// <param name="id">Es: (Obligatorio) Cadena de texto (String) con el Guid identificador del plugin.<br />En: (Mandatory) Text string containing the plugin's identifying Guid.</param>
    /// <returns>Es: Un InfoResponse con los datos del ReferencePluginStoreDTO solicitados o un error interno.<br />En: An InfoResponse with the requested ReferencePluginStoreDTO data or an internal error.</returns>
    // ReferencePluginStoreDTO
    [TerbinExecutable((byte)CodeServices.Read, (byte)CodeServicesSection.PluginStorage)]
    public static async Task<InfoResponse?> GetAll(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string id = reader.ReadArray<char>().CrString();

        ReferencePluginStore? plugin;

        plugin = await Manager.StoragePlugin.Get(id);
        if (plugin is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.PluginNotExist));

        return InfoResponse.CreateSucces(pHead.IdRequest, plugin.ToSerilizeDTO());
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio utilizado para eliminar física y lógicamente un plugin del sistema base.<br />
    /// Parsea el identificador del payload y procede a solicitar la eliminación irreversible del registro en el manifiesto y su respectivo archivo en disco.<br />
    /// Notas: Considera la cancelación asíncrona mediante Token antes de llamar al Manager, interrumpiendo el flujo si se pide cancelar.<br />
    /// Tips: Maneja las respuestas con códigos de error en caso de que la eliminación falle (ej. si está en uso o no se encuentra).<br />
    /// ___________________( English )___________________<br />
    /// Service used to physically and logically delete a plugin from the base system.<br />
    /// Parses the identifier from the payload and proceeds to request the irreversible deletion of the registry in the manifest and its respective file on disk.<br />
    /// Notes: Considers asynchronous cancellation via Token before calling the Manager, interrupting the flow if cancellation is requested.<br />
    /// Tips: Handles responses with error codes in case the deletion fails (e.g., if it is in use or not found).<br />
    /// </summary>
    /// <param name="id">Es: (Obligatorio) Cadena de texto (String) con el Guid identificador del plugin a borrar.<br />En: (Mandatory) Text string containing the Guid identifier of the plugin to delete.</param>
    /// <returns>Es: Un InfoResponse de éxito si el archivo se purga correctamente, o un error interno de fallo.<br />En: A success InfoResponse if the file is correctly purged, or an internal error upon failure.</returns>[TerbinExecutable((byte)CodeServices.Deleted, (byte)CodeServicesSection.PluginStorage)]
    public static async Task<InfoResponse?> Delete(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        string id = reader.ReadArray<char>().CrString();

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        var result = await Manager.Plugin.DeletedOne(id, pToken);
        if (result != InternalErrors.IsSucces)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize<ushort>((ushort)result));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }
}