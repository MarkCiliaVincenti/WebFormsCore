﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyTests;
using VerifyXunit;

namespace WebForms.SourceGenerator.Tests;

[UsesVerify]
public class GenerateTest
{
    [Fact]
    public Task GenerateDesigner()
    {
        VerifySourceGenerators.Enable();

        var generator = new DesignerGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(Control).Assembly.Location)
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using System.Web.UI.WebControls;

            namespace Tests
            {
                public partial class Example
                {
                    public HtmlForm form1;
                }
            }
            """
        );

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] {syntaxTree},
            references: references);

        driver = driver.AddAdditionalTexts(
            ImmutableArray.Create<AdditionalText>(
                new MemoryAdditionalText(
                    "Example.aspx",
                    """
                    <%@ Page language="C#" Inherits="Tests.Example" %>
                    <%@ Register TagPrefix="asp" Namespace="System.Web.UI.WebControls" %>
                    <!DOCTYPE htm>
                    <html>
                    <head runat="server">
                        <title></title>
                    </head>
                    <body>
                        <form id="form1" runat="server">
                            <div>
                                <asp:TextBox id="tbUsername" runat="server" /><br />
                                <asp:textbox id="tbPassword" runat="server" /><br />
                                <asp:button id="btnLogin" runat="server" click="btnLogin_Click" text="Login" />
                            </div>
                        </form>
                    </body>
                    </html>
                    """
                )
            )
        );

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver);
    }


    [Fact]
    public Task GenerateViewState()
    {
        VerifySourceGenerators.Enable();

        var generator = new ViewStateGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(Control).Assembly.Location)
        };

        var syntaxTree = CSharpSyntaxTree.ParseText(
            """
            using System.Web;

            namespace Tests
            {
                public partial class Example
                {
                    [ViewState] private string test;
                }
            }
            """
        );

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references);

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver);
    }
}
