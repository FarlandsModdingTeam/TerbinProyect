using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TerbinLibrary;
using TerbinLibrary.Data;
using TerbinLibrary.SteamFarlands;
using TerbinLibrary.TerbinServiceHelper.Exceptions;
using TerbinLibrary.Useful;
using TerbinLibrary.Useful.Nodes;

namespace TerbinService.Managers;
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


public static partial class Manager
{
    public static class Games
    {
        public static async Task<Status> HandleCloneInInstance
            (string pPathDir, string pNameInstance, ushort pIdRequest, bool pOverwrite, CancellationToken pCancellationToken = default)
        {
            var progress = Util.CreateProgessBarr(Worker.CurrentConst.Value.Communicator, pIdRequest);

            return await CloneInInstance(pPathDir, pNameInstance, pOverwrite, progress, pCancellationToken);
        }


        [TODO("Implementar cancelacion en CloneInInstance")]
        public static async Task<Status> CloneInInstance
            (string pPathDir, string pNameInstance, bool pOverwrite, IProgress<TerbinInfoProgrss> pProgrss = default, CancellationToken pCancellationToken = default)
        {
            string? pathInstace = Instances.MakePathFolder(pNameInstance);
            if (string.IsNullOrEmpty(pathInstace))
                return Status.ErrorGetInstance;

            if (!Manager.Instances.IsInstance(pathInstace))
                return Status.ErrorNotIsInstance;

            var (status, json) = await FileUtil.CloneDirectory(pPathDir, pathInstace, pOverwrite, pProgrss);
            if (status != StatusFileUtil.Succes)
                return Status.GenericError;

            Manager.Manifest.WriteHandwritten(pathInstace, json);

            var exes = FileUtil.GetAllExeFiles(pathInstace);
            if (exes is null)
                return Status.ErrorGameNotExes;

            var update = Manager.Manifest.UpdateInstace(pNameInstance, pathInstace, manifest =>
            {
                manifest.Executable = exes[0];
            });
            if (!update)
                return Status.ErrorUpdateInstace;
            return Status.Succes;
        }

        [TODO("Implementar cancelacion en RemoveInInstance")]
        public static async Task<Status> RemoveInInstance
            (string pNameInstance, IProgress<TerbinInfoProgrss>? pProgress = default, CancellationToken pCancellationToken = default)
        {
            string? pathInstace = Instances.MakePathFolder(pNameInstance);
            if (string.IsNullOrEmpty(pathInstace))
                return Status.ErrorGetInstance;

            var handwritten = Manager.Manifest.GetHandwritten(pathInstace);

            if (handwritten == null)
                return Status.ErrorGetHandwritten;

            if (pCancellationToken.IsCancellationRequested)
                return Status.IsCancelled;

            // Teoricamente DeleteFromHandwritten no puede fallar.
            var r = FileUtil.DeleteFromHandwritten(pathInstace, handwritten, pProgress);

            bool removeHand = Manager.Manifest.RemoveHandwritten(pathInstace);
            if (!removeHand)
                return Status.ErrorRemoveHandwritten;

            return Status.Succes;
        }






        public static string GetVersion()
        {
            return ManagerFarlands.GetVersion();
        }

        public enum Status : sbyte
        {
            GenericException = -1,

            IsCancelled = 0,
            Succes = 1,

            GenericError = 2,
            ErrorHandwritten = 3,
            ErrorGetInstance = 4,
            ErrorNotIsInstance = 5,
            ErrorGameNotExes = 6,
            ErrorUpdateInstace = 7,

            ErrorGetHandwritten = 8,
            ErrorRemoveHandwritten = 9,
        }
    }
}
