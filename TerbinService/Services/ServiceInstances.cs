using System;
using System.Collections.Generic;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Communication.Packets;
using TerbinLibrary.Data.Instance;
using TerbinLibrary.Data.Transport;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.HelperData;
using TerbinLibrary.Protocol;
using TerbinLibrary.Serialize;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinService.Managers;

namespace TerbinService.Services;
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

[TODO("Update Instance")]
internal static class ServiceInstances
{
    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio encargado de crear una nueva instancia en el sistema.<br />
    /// Lee el nombre de la instancia desde el payload y, opcionalmente, la ruta de instalación.<br />
    /// Notas: Cancela la operación si el token lo solicita antes de invocar al gestor de almacenamiento.<br />
    /// Tips: Envía la ruta si deseas ubicar la instancia de forma absoluta fuera del directorio por defecto.<br />
    /// ___________________( English )___________________<br />
    /// Service responsible for creating a new instance in the system.<br />
    /// Reads the instance name from the payload and, optionally, the installation path.<br />
    /// Notes: Cancels the operation if the token requests it before invoking the storage manager.<br />
    /// Tips: Send the path if you wish to locate the instance absolutely outside the default directory.<br />
    /// </summary>
    /// <param name="name">Es: Nombre de la nueva instancia (Obligatorio). <br />En: Name of the new instance (Mandatory).</param>
    /// <param name="path">Es: Ruta física específica para crear la instancia (Opcional). <br />En: Specific physical path to create the instance (Optional).</param>
    /// <returns>Es: Un InfoResponse indicando éxito o error interno. <br />En: An InfoResponse indicating success or internal error.</returns>
    [TerbinExecutable((byte)CodeServices.Create, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> CreateInstance(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        string name;
        string path;

        ReadOnlySpan<byte> reader = pParameters;
        name = reader.ReadArray<char>().CrString();
        if (reader.Length > ThreeQuartersInt.Size)
            path = reader.ReadArray<char>().CrString();

        if (pToken.IsCancellationRequested)
            return InfoResponse.CreateCancelled(pHead.IdRequest);

        // TODO: Si hay path crearlo ahí.
        bool succes = Manager.Instances.NewInstance(name, false);
        if (!succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.InstanceCreate));

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio destinado a eliminar completamente una instancia existente y sus archivos.<br />
    /// Lee el nombre de la instancia a borrar directamente del payload recibido.<br />
    /// Notas: Verifica errores detallados devueltos por el gestor para identificar si la ruta no existe, no era una instancia válida o si el borrado falló.<br />
    /// Tips: Esta acción es destructiva; asegúrate de pedir confirmación del usuario antes de enviar este comando.<br />
    /// ___________________( English )___________________<br />
    /// Service designed to completely delete an existing instance and its files.<br />
    /// Reads the name of the instance to be deleted directly from the received payload.<br />
    /// Notes: Checks detailed errors returned by the manager to identify if the path does not exist, was not a valid instance, or if deletion failed.<br />
    /// Tips: This action is destructive; make sure to ask for user confirmation before sending this command.<br />
    /// </summary>
    /// <param name="name">Es: Nombre de la instancia a eliminar (Obligatorio). <br />En: Name of the instance to delete (Mandatory).</param>
    /// <returns>Es: Un InfoResponse de éxito, cancelación o error interno. <br />En: An InfoResponse of success, cancellation, or internal error.</returns>
    [TerbinExecutable((byte)CodeServices.Deleted, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> DeleteInstances(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        string name;
        ReadOnlySpan<byte> reader = pParameters;
        name = reader.ReadArray<char>().CrString();

        var r = await Manager.Instances.Delete(name, pToken);

        if (r == Manager.Instances.Status.IsCancelled)
            return InfoResponse.CreateCancelled(pHead.IdRequest);
        if (r != Manager.Instances.Status.Succes)
        {
            var error = TSHelper.GetError(r switch
            {
                Manager.Instances.Status.ErrorNotExist => InternalErrors.InstanceNotExist,
                Manager.Instances.Status.ErrorIsNotInstance => InternalErrors.InstanceIsNotInstance,
                Manager.Instances.Status.ErrorUnregistInstance => InternalErrors.InstanceUnregister,
                _ => InternalErrors.NodeDinamite,
            });
            return InfoResponse.CreateInteralError(pHead.IdRequest, error);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest);
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio que recupera la lista completa de instancias registradas en el índice del sistema.<br />
    /// Serializa y devuelve un array compuesto por el conteo total seguido de objetos ReferenceInstanceDTO.<br />
    /// Notas: Si no hay instancias registradas en el índice, devuelve un payload con un único byte nulo o de valor 0.<br />
    /// Tips: Ideal para inicializar la vista principal del cliente o refrescar el catálogo de juegos.<br />
    /// ___________________( English )___________________<br />
    /// Service that retrieves the complete list of instances registered in the system's index.<br />
    /// Serializes and returns an array composed of the total count followed by ReferenceInstanceDTO objects.<br />
    /// Notes: If there are no instances registered in the index, it returns a payload with a single null byte or 0 value.<br />
    /// Tips: Ideal for initializing the client's main view or refreshing the game catalog.<br />
    /// </summary>
    /// <returns>Es: Un InfoResponse conteniendo la cantidad de instancias y sus estructuras serializadas. <br />En: An InfoResponse containing the instance count and their serialized structures.</returns>
    [TerbinExecutable((byte)CodeServices.ReadAll, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> GetAllInstances(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        List<ReferenceInstance> instances = Manager.Index.GetAllInstances();
        Serialineitor s = new();

        if (instances.Count <= 0)
            return InfoResponse.CreateSucces(pHead.IdRequest, [0]);

        s.Add<ThreeQuartersInt>(instances.Count);
        for (int i = 0; i < instances.Count; i++)
        {
            ReferenceInstanceDTO tmp = (ReferenceInstanceDTO)instances[i];
            s.AddStruct<ReferenceInstanceDTO>(tmp);
        }

        return InfoResponse.CreateSucces(pHead.IdRequest, s.Serialize());
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Servicio utilizado para leer la información detallada de una única instancia.<br />
    /// Lee el nombre de la instancia solicitada desde el payload y recupera su manifiesto (ManifestInstance).<br />
    /// Notas: Retorna un error interno tipificado si la instancia buscada no existe en disco o falla su lectura.<br />
    /// Tips: Utilízalo para cargar propiedades detalladas (como la cantidad de plugins) antes de interactuar con la instancia.<br />
    /// ___________________( English )___________________<br />
    /// Service used to read detailed information of a single instance.<br />
    /// Reads the requested instance name from the payload and retrieves its manifest (ManifestInstance).<br />
    /// Notes: Returns a typed internal error if the targeted instance does not exist on disk or fails to be read.<br />
    /// Tips: Use it to load detailed properties (like plugin count) before interacting with the instance.<br />
    /// </summary>
    /// <param name="name">Es: Nombre de la instancia a consultar (Obligatorio). <br />En: Name of the instance to query (Mandatory).</param>
    /// <returns>Es: Un InfoResponse de éxito con el ManifestInstanceDTO serializado o un error interno. <br />En: A success InfoResponse with the serialized ManifestInstanceDTO or an internal error.</returns>
    [TerbinExecutable((byte)CodeServices.Read, (byte)CodeServicesSection.Instances)]
    public static async Task<InfoResponse?> GetOne(Header pHead, byte[] pParameters, CancellationToken pToken)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        ReadOnlySpan<byte> reader = pParameters;
        var name = reader.ReadArray<char>().CrString();

        ManifestInstance? manifest;

        manifest = await Manager.Instances.GetManifestByName(name);
        if (manifest is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, TSHelper.GetError(InternalErrors.InstanceNotExist));

        byte[] dto = ((ManifestInstanceDTO)manifest).Serialize();

        return InfoResponse.CreateSucces(pHead.IdRequest, dto);
    }
}
