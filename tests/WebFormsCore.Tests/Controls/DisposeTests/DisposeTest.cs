﻿using WebFormsCore.TestFramework;
using WebFormsCore.UI;

namespace WebFormsCore.Tests.Controls.DisposeTests.Pages;

public class DisposeTest
{
    [Theory, ClassData(typeof(TestTypeData))]
    public async Task PageWithControl(TestType type)
    {
        DisposableControl staticControl;
        DisposableControl dynamicControl;

        await using (var result = await StartAsync<DisposePage>(type))
        {
            staticControl = result.Control.FindRequiredControl<DisposableControl>("staticControl");
            dynamicControl = result.Control.FindRequiredControl<DisposableControl>("dynamicControl");

            Assert.False(staticControl.IsDisposed, "Control in Page should not be disposed");
            Assert.False(dynamicControl.IsDisposed, "Dynamic control should not be disposed");
        }

        Assert.True(staticControl.IsDisposed, "Control in Page should be disposed");
        Assert.True(dynamicControl.IsDisposed, "Dynamic control should be disposed");
    }

}
