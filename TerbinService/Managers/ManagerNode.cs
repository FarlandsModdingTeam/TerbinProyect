using System;
using System.Collections.Generic;
using System.Text;
using TerbinLibrary;
using TerbinLibrary.Data;
using TerbinLibrary.SteamFarlands;
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
    public static class Node
    {
        public static (long? maxFiles, long? maxDir) GetSizeDir(string pDir)
        {
            long? countFiles = FileUtil.GetCountFiles(pDir);
            long? countDir = FileUtil.GetCountDirectories(pDir);
            return (countFiles, countDir);
        }

        [TODO("Clonar Juego En cualquier lado, Se deberia permitir tener una instancia separada del resto y donde te salga de los cataplines.")]
        public static async Task CloneGame
            (string pPathDir, string pNameInstance, bool pOverwrite, IProgress<TerbinInfoProgrss> pProgrss = default, CancellationToken pCancellationToken = default)
        {
            throw new NotImplementedException();
        }

        [TODO("Dinamitar cualquier directorio.")]
        public static async Task DinamiteDirectory
            (string pPathDir, string pNameInstance, bool pOverwrite, IProgress<TerbinInfoProgrss> pProgrss = default, CancellationToken pCancellationToken = default)
        {

        }

        public static string GetNameByFile(string pFile)
        {
            if (string.IsNullOrWhiteSpace(pFile))
                return string.Empty;

            string fileName = Path.GetFileNameWithoutExtension(pFile);
            return ClearNameByFile(fileName);
        }

        public static string ClearNameByFile(string pFileNameWithoutExtension)
        {
            pFileNameWithoutExtension = pFileNameWithoutExtension.Replace('_', ' ').Replace('-', ' ');
            return string.Join(' ', pFileNameWithoutExtension.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
