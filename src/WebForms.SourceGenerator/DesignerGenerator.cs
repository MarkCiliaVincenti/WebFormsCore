﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Scriban;
using WebForms.Designer;

namespace WebForms.SourceGenerator;

[Generator]
public class DesignerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var files = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase) ||
                        a.Path.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
            .Select((a, c) => (a.Path, a.GetText(c)!.ToString()));

        var compilationAndFiles = context.CompilationProvider.Combine(files.Collect());

        context.RegisterSourceOutput(compilationAndFiles, Generate);
    }
    
    public void Generate(SourceProductionContext context, (Compilation Left, ImmutableArray<(string, string)> Right) sourceContext)
    {
        var (compilation, files) = sourceContext;
        var types = new List<DesignerType>();

        foreach (var (path, text) in files)
        {
            if (DesignerType.Parse(compilation, path, text) is { Fields.Count: > 0 } type)
            {
                types.Add(type);
            }
        }

        const string templateFile = "Templates/designer.scriban";
        var model = new DesignerModel(types);
        var template = Template.Parse(EmbeddedResource.GetContent(templateFile), templateFile);
        var output = template.Render(model, member => member.Name);

        context.AddSource("WebForms.Designer.cs", output);
    }
}