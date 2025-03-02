﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebFormsCore.UI;

namespace WebFormsCore;

public abstract class WebServerContext<T>(IHost host) : IWebServerContext<T>
    where T : Control, new()
{
    private T? _control;
    private string? _url;
    private TaskCompletionSource? _requestLock;

    public T Control => _control ?? throw new InvalidOperationException("Control is not initialized");

    public Task SetControlAsync(T control)
    {
        var requestLock = new TaskCompletionSource();
        _control = control;

        var oldLock = Interlocked.Exchange(ref _requestLock, requestLock);
        oldLock?.SetResult();

        return requestLock.Task;
    }

    public string Url => _url ??= host.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()
        ?.Addresses.FirstOrDefault() ?? throw new InvalidOperationException("Server address is not available");

    protected virtual ValueTask DisposeCoreAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        _requestLock?.SetResult();
        await host.StopAsync();
        await DisposeCoreAsync();

        if (host is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            host.Dispose();
        }
    }

    public abstract Task GoToUrlAsync(string url);

    public abstract ValueTask<string> GetHtmlAsync();

    public abstract ValueTask ReloadAsync();

    public abstract IElement? GetElementById(string id);

    public abstract IElement? QuerySelector(string selector);

    public abstract IAsyncEnumerable<IElement> QuerySelectorAll(string selector);
}
