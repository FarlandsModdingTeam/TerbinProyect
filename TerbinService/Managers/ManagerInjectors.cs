using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.IO;
using TerbinLibrary;
using TerbinLibrary.Communication;
using TerbinLibrary.Execution;
using TerbinLibrary.Extension;
using TerbinLibrary.Serialize;
using TerbinLibrary.Useful;
using TerbinLibrary.TerbinServiceHelper;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.TerbinServiceHelper.Consoles;
using TerbinLibrary.Protocol;
using TerbinLibrary.Useful.NetWork;

namespace TerbinService.Managers;

public partial class Manager
{
#if false
    [TerbinExecutable((byte)CodeServices.InstallBepInEx)]
    public static async Task<InfoResponse?> IntallBepInEx(Header pHead, byte[] pParameters)
    {
        if (pParameters.Length <= 0)
            return InfoResponse.Create(pHead.IdRequest, CodeStatus.ErrorNotPayload);

        long? sizeBepInEx = await NetUtil.GetContentLength(TerbinURLs.BepInEx);
        if (sizeBepInEx is null)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize((ushort)CodeInternalErrors.BepInExNotConect));

        AmongInfoThreads info = Worker.CurrentConst.Value;

        string rute = Serialineitor.DeserializeArray<char>(pParameters).CrString();
        // Habra alguna forma de saber si es un direccion valida?

        if (!Directory.Exists(rute))
            Directory.CreateDirectory(rute);

        byte idMemory = 0;
        var rId = await info.Communicator.SoliciteRequestMemory();
        if (rId.Head.Status != CodeStatus.Succes)
            return InfoResponse.CreateInteralError(pHead.IdRequest, Serialineitor.Serialize((ushort)CodeInternalErrors.IdSoliciteError));

        idMemory = rId.Payload[0];
        _ = HandleInstallBepInExWithProgress(idMemory, rute);

        return new InfoResponse
        {
            IdRequest = pHead.IdRequest,
            Status = CodeStatus.Succes,
            Payload = [idMemory, .. Serialineitor.Serialize<long>(sizeBepInEx.Value)],
        };
    }
#endif
    [Obsolete("Use: Injectors")]
    public static class BepInEx
    {

        [Obsolete]
        public static async Task HandleInstallBepInExWithProgress(byte pIdMemory, string pDir)
        {
            IProgress<TerbinInfoProgrss> progressBarr = new Progress<TerbinInfoProgrss>(p =>
            {
                var Content = p.ToArray();
                _ = Worker.CurrentContext.Value.Communicator.Load(TerbinProtocol.ORDER_SINGLE, pIdMemory, Content);
                Console.Write($"\rDescargando... {Math.Round((float)p.Percentage, 2)}% completado | Total:X/{p.Current}:Actual ");
            });
            try
            {
                StatusNetUtil? r = await HandleInstallBepInEx(TerbinURLs.BepInEx, progressBarr);
                if (r is null) throw new Exception("TODO: informar de que BepInEx ya esta instalado");
                if (r != StatusNetUtil.Succes)
                {
                    InternalErrors error = r switch
                    {
                        StatusNetUtil.ExceptionOnExtractZip => InternalErrors.ZipExtractException,
                        StatusNetUtil.ExceptionDeleteTemporalFile => InternalErrors.ZipDeletedTempException,
                        _ => InternalErrors.ZipExtractError
                    };
                    throw new Exception($"TODO: informar de {error}");

                    // Prototipo del funcionamiento de Info
                    InfoLocalThreads info = Worker.CurrentContext.Value;
                    byte[] pld = new Serialineitor()
                        .Add(TypeService.Service)
                        .Add(CodeServices.Dowload)
                        .Add(error)
                        .Serialize();
                    _ = info.Communicator.Send(new((byte)CodeTerbinProtocol.ExceptionAlert), pld);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.CrString("HandleInstallBepInExWithProgress"));
            }
        }

        [Obsolete("Use: normal nstall")]
        public static async Task<StatusNetUtil?> HandleInstallBepInEx(string pDir, IProgress<TerbinInfoProgrss>? pProgress = default)
        {
            StatusNetUtil r = StatusNetUtil.Succes;
            if (CheckInstallBepInEx(pDir)) return null;
            r = await NetUtil.InstallZip(TerbinURLs.BepInEx, pDir, pProgress);
            return r;
        }

        [Obsolete("Use: Injectors")]
        public static bool CheckInstallBepInEx(string pDir)
        {
            string bep = Path.Combine(pDir, "BepInEx");
            return Directory.Exists(bep);
        }


        [Obsolete("Use: Injectors")]
        public static string GetBepInExFolderPlugin(string pPathInstance) // BepInEx/plugins/
        {
            string pathBepInExFolder;
            string pathPlugins;

            pathBepInExFolder = Path.Combine(pPathInstance, "BepInEx");
            if (!Directory.Exists(pathBepInExFolder))
                Directory.CreateDirectory(pathBepInExFolder);

            pathPlugins = Path.Combine(pathBepInExFolder, "plugins");
            if (!Directory.Exists(pathPlugins))
                Directory.CreateDirectory(pathPlugins);

            return pathPlugins;
        }
    }

    /// <summary>
    /// ___________________( Español )___________________<br />
    /// Clase encargada de proveer utilidades útiles e inyectar complementos y configuraciones.<br />
    /// Gestor para facilitar la instalación y manejo de rutas para herramientas como BepInEx.<br />
    /// ___________________( English )___________________<br />
    /// Class responsible for providing useful utilities and injecting plugins and configurations.<br />
    /// Manager to facilitate the installation and route management for tools such as BepInEx.<br />
    /// </summary>
    public static class Injector
    {
        private const string _FOLDER_BEPINEX = "BepInEx";
        private const string _FOLDER_PLUGIN = "plugins";

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Comprueba si BepInEx se encuentra instalado en la ruta proporcionada.<br />
        /// Notas: Únicamente verifica que el directorio base de BepInEx exista dentro del directorio principal.<br />
        /// ___________________( English )___________________<br />
        /// Checks if BepInEx is installed in the provided path.<br />
        /// Notes: It only verifies that the BepInEx base directory exists within the main directory.<br />
        /// </summary>
        /// <param name="pDir">Es: Ruta principal del directorio a comprobar. <br />En: Main path of the directory to check.</param>
        /// <returns>Es: Verdadero en caso de estar instalado, de lo contrario falso. <br />En: True if it is installed, false otherwise.</returns>
        public static bool CheckInstallBepInEx(string pDir)
        {
            string bep = Path.Combine(pDir, _FOLDER_BEPINEX);
            return Directory.Exists(bep);
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Construye y obtiene la ruta de la carpeta de complementos (plugins) de BepInEx.<br />
        /// Si las carpetas requeridas no existen dentro de la instancia, serán creadas de forma automática.<br />
        /// ___________________( English )___________________<br />
        /// Builds and gets the path of the BepInEx plugins folder.<br />
        /// If the required folders do not exist within the instance, they will be created automatically.<br />
        /// </summary>
        /// <param name="pPathInstance">Es: La ruta base de la instancia destino. <br />En: The base path of the target instance.</param>
        /// <returns>Es: Ruta absoluta de la carpeta de plugins. <br />En: Absolute path of the plugins folder.</returns>
        public static string GetBepInExFolderPlugin(string pPathInstance) // BepInEx/plugins/
        {
            string pathBepInExFolder;
            string pathPlugins;

            pathBepInExFolder = Path.Combine(pPathInstance, _FOLDER_BEPINEX);
            if (!Directory.Exists(pathBepInExFolder))
                Directory.CreateDirectory(pathBepInExFolder);

            pathPlugins = Path.Combine(pathBepInExFolder, _FOLDER_PLUGIN);
            if (!Directory.Exists(pathPlugins))
                Directory.CreateDirectory(pathPlugins);

            return pathPlugins;
        }

        /// <summary>
        /// ___________________( Español )___________________<br />
        /// Concatena la ruta de la instancia base con las de la carpeta de complementos de BepInEx.<br />
        /// Notas: A diferencia de GetBepInExFolderPlugin, este método no crea ni altera los directorios, sólo devuelve la ruta.<br />
        /// ___________________( English )___________________<br />
        /// Concatenates the base instance path with the BepInEx plugins folder paths.<br />
        /// Notes: Unlike GetBepInExFolderPlugin, this method does not create or alter directories, it only returns the path.<br />
        /// </summary>
        /// <param name="pPathInstance">Es: Ruta principal de la instancia del proyecto a buscar. <br />En: Main path of the instance project to look for.</param>
        /// <returns>Es: Cadena de texto con la ruta combinada. <br />En: String with the combined path.</returns>
        public static string MakeBepInExFolderPlugin(string pPathInstance) =>
            Path.Combine(pPathInstance, _FOLDER_BEPINEX, _FOLDER_PLUGIN);
    }
}