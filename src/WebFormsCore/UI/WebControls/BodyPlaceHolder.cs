using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebFormsCore.UI.WebControls;

public class BodyPlaceHolder : PlaceHolder, IBodyControl
{
    protected override void OnInit(EventArgs args)
    {
        base.OnInit(args);

        Page.BodyControls.Add(this);
    }

    public override ValueTask RenderAsync(HtmlTextWriter writer, CancellationToken token)
    {
        return default;
    }

    public virtual async Task RenderInBodyAsync(HtmlTextWriter writer, CancellationToken token)
    {
        if (!Visible)
        {
            return;
        }

        writer.AddAttribute("data-wfc-owner", Form?.ClientID ?? "");
        await writer.RenderBeginTagAsync("div");

        await base.RenderAsync(writer, token);

        await writer.RenderEndTagAsync();
    }
}
