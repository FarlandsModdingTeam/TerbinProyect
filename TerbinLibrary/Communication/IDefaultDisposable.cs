using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

namespace TerbinLibrary.Communication
{
    public interface IDefaultDisposable : IDisposable
    {
        bool Disposed { get; set; }
        new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                liberateAdministered();
            }
            liberateNotAdministered();

            Disposed = true;
        }

        // Liberar recursos administrados.
        void liberateAdministered();
        // Liberar recursos NO administrados aquí (si los hubiera).
        void liberateNotAdministered();

        //~IDefaultDisposable()
        //{
        //    Dispose(false);
        //}
    }
}
