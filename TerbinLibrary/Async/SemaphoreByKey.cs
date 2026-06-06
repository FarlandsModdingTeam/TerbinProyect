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
        this._locks = new ConcurrentDictionary<TKey, SemaphoreSlim>(pComparer ?? EqualityComparer<TKey>.Default);
    }

    /// <summary>
    /// Solicita el acceso exclusivo para una clave específica. 
    /// Devuelve un token ejecutable en un bloque 'using' que liberará el semáforo al destruirse.
    /// </summary>
    public async Task<IDisposable> LockAsync(TKey pK, CancellationToken pToken = default)
    {
        var semaphore = _locks.GetOrAdd(pK, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(pToken).ConfigureAwait(false);

        return new Releaser(semaphore);
    }

    public async Task<IDisposable> LockAsync(TKey pK1, TKey pK2, CancellationToken pToken = default)
    {
        var s1 = _locks.GetOrAdd(pK1, _ => new SemaphoreSlim(1, 1));
        var s2 = _locks.GetOrAdd(pK2, _ => new SemaphoreSlim(1, 1));

        await s1.WaitAsync(pToken).ConfigureAwait(false);
        await s2.WaitAsync(pToken).ConfigureAwait(false);

        return new Releaser(s1, s2);
    }


    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim[] _semaphore;
        private int _isDisposed;

        public Releaser(SemaphoreSlim pSemaphore) : this([pSemaphore])
        {
        }
        public Releaser(params SemaphoreSlim[] pSemaphore)
        {
            this._semaphore = pSemaphore;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
                for (int i = 0; i < _semaphore.Length; i++)
                {
                    _semaphore[i].Release();
                }
        }
    }
}