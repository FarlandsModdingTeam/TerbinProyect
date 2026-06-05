using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TerbinLibrary.Async;
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


// StringComparer.OrdinalIgnoreCase

public class SemaphoreByKey<TKey> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks;

    public SemaphoreByKey(IEqualityComparer<TKey>? pComparer = null)
    {
        _locks = new ConcurrentDictionary<TKey, SemaphoreSlim>(pComparer ?? EqualityComparer<TKey>.Default);
    }

    /// <summary>
    /// Solicita el acceso exclusivo para una clave específica. 
    /// Devuelve un token ejecutable en un bloque 'using' que liberará el semáforo al destruirse.
    /// </summary>
    public async Task<IDisposable> LockAsync(TKey pKey, CancellationToken pToken = default)
    {
        var semaphore = _locks.GetOrAdd(pKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(pToken).ConfigureAwait(false);

        return new Releaser(semaphore);
    }




    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _isDisposed;

        public Releaser(SemaphoreSlim pSemaphore)
        {
            _semaphore = pSemaphore;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                _semaphore.Release();
            }
        }
    }
}