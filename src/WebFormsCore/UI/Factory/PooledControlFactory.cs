using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.ObjectPool;

namespace WebFormsCore.UI;

internal sealed class PooledControlFactory<T> : IControlFactory<T>, IDisposable
    where T : Control
{
    private readonly IControlInterceptor[] _interceptors;
    private readonly ObjectPool<T> _pool;
    private readonly ConcurrentStack<T> _controls = new();

    public PooledControlFactory(ObjectPool<T> pool, IEnumerable<IControlInterceptor> interceptors)
    {
        _pool = pool;
        _interceptors = interceptors.ToArray();
    }

    public T CreateControl(IServiceProvider provider)
    {
        var control = _pool.Get();
        _controls.Push(control);

        foreach (var interceptor in _interceptors)
        {
            control = interceptor.OnControlCreated(control);
        }

        return control;
    }

    public void Dispose()
    {
        foreach (var control in _controls)
        {
            _pool.Return(control);
        }
    }

    object IControlFactory.CreateControl(IServiceProvider provider)
    {
        return CreateControl(provider);
    }
}
