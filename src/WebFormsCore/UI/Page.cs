﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HttpStack;
using Microsoft.Extensions.DependencyInjection;
using WebFormsCore.Events;
using WebFormsCore.Security;
using WebFormsCore.UI.HtmlControls;

namespace WebFormsCore.UI;

public class Page : Control, INamingContainer, IStateContainer, System.Web.UI.Page, IInternalPage
{
    private IHttpContext? _context;
    private ScopedControlContainer? _scopedContainer;
    public List<object>? _changedPostDataConsumers;

    public Page()
    {
        ClientScript = new ClientScriptManager(this);
    }

    public bool EnablePageViewState { get; set; } = true;

    public Csp Csp { get; set; } = new();

    public ClientScriptManager ClientScript { get; }

    protected override IHttpContext Context => _context ?? throw new InvalidOperationException("No HttpContext available.");

    public bool IsPostBack { get; set; }

    protected override IServiceProvider ServiceProvider => Context.RequestServices;

    private ScopedControlContainer ScopedContainer => _scopedContainer ??= ServiceProvider.GetRequiredService<ScopedControlContainer>();

    public List<HtmlForm> Forms { get; set; } = new();

    public override HtmlForm? Form => null;

    internal HtmlForm? ActiveForm { get; set; }

    internal List<IBodyControl> BodyControls { get; set; } = new();

    internal async Task<HtmlForm?> ProcessRequestAsync(CancellationToken token)
    {
        IsPostBack = Context.Request.Method == "POST";

        var viewStateManager = ServiceProvider.GetRequiredService<IViewStateManager>();

        InvokeFrameworkInit(token);

        foreach (var pageService in ServiceProvider.GetServices<IPageService>())
        {
            await pageService.BeforeInitializeAsync(this);
        }

        await InvokeInitAsync(token);

        foreach (var pageService in ServiceProvider.GetServices<IPageService>())
        {
            await pageService.AfterInitializeAsync(this);
        }

        var form = await viewStateManager.LoadFromRequestAsync(Context, this);

        ActiveForm = form;

        if (form != null)
        {
            Forms.RemoveAll(i => i != form && i.Parent.Controls.Remove(i));
        }
        else if (IsPostBack)
        {
            Forms.RemoveAll(i => i.Parent.Controls.Remove(i));
        }

        await InvokeLoadAsync(token, form);

        if (IsPostBack)
        {
            if (Context.Request.Form.TryGetValue("wfcTarget", out var eventTarget))
            {
                var eventArgument = Context.Request.Form.TryGetValue("wfcArgument", out var eventArgumentValue)
                    ? eventArgumentValue.ToString()
                    : string.Empty;

                await InvokePostbackAsync(token, form, eventTarget, eventArgument);
            }

            await RaiseChangedEventsAsync(token);

            if (form != null)
            {
                await form.OnSubmitAsync(token);
            }
        }

        await InvokePreRenderAsync(token, form);

        return form;
    }

    public async ValueTask RaiseChangedEventsAsync(CancellationToken cancellationToken)
    {
        if (_changedPostDataConsumers is not {} consumers) return;

        foreach (var consumer in consumers)
        {
            if (consumer is IPostBackDataHandler handler)
            {
                handler.RaisePostDataChangedEvent();
            }

            if (consumer is IPostBackAsyncDataHandler eventHandler)
            {
                await eventHandler.RaisePostDataChangedEventAsync(cancellationToken);
            }
        }
    }

    protected internal virtual void Initialize(IHttpContext context)
    {
        _context = context;
    }

    void IInternalPage.Initialize(IHttpContext context) => Initialize(context);

    protected internal virtual void RegisterDisposable(Control control)
    {
        ScopedContainer.Register(control);
    }

    protected override string GetUniqueIDPrefix() => "p$";
}
