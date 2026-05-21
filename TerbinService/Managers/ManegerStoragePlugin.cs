using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinService.Managers;

public static class Maneger
{
    public static class StoragePlugin
    {
        // TerbinConfiguration
        // TerbinServiceConst.MANIFEST_STORAGE

        public static async Task<Guid> Store(string pPathPlugin, string pNameFile)
        {
            // TODO: Renombrar el archivo de pPathPlugin por pNameFile.
            // TODO: Llamar a Store.
            throw new NotImplementedException("Ñe");
        }
        public static async Task<Guid> Store(string pPathPlugin)
        {
            throw new NotImplementedException("Ñe");
        }


        public static async Task<bool> Exist(string pFile)
        {


            return false;
        }

        public static async Task<bool> Exist(Guid pG)
        {


            return false;
        }
    }
}
