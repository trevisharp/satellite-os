using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SatelliteOS;

internal class Compiler
{
    protected IEnumerable<MetadataReference> GetReferences(
        IEnumerable<Assembly> extraRefs)
    {
            var defaultRefs = new[]
            {
                "System.Private.CoreLib",
                "System.Console",
                "System.Runtime",
                "System.Linq",
                "System.Linq.Expressions",
                "System.Collections",
                "netstandard"
            };
        
        var assembly = Assembly.GetEntryAssembly();
        var assemblies = assembly
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Append(assembly)
            .Concat(defaultRefs.Select(Assembly.Load))
            .Concat(extraRefs);
        
        return 
            from a in assemblies
            select a.Location into loc
            select MetadataReference.CreateFromFile(loc);
    }

    public (Assembly, string[]) GetNewAssembly(
        string[] files,
        IEnumerable<Assembly> extraRefs)
    {
        var syntaxTrees = files
            .Select(text => CSharpSyntaxTree.ParseText(text));

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.ConsoleApplication
        );
        
        var compilation = CSharpCompilation.Create(
            "CodeCompilation",
            syntaxTrees: syntaxTrees,
            references: GetReferences(extraRefs),
            options: compilationOptions
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (result.Success)
        {
            ms.Seek(0, SeekOrigin.Begin);
            return (Assembly.Load(ms.ToArray()), []);
        }
        
        return (null, [.. result.Diagnostics.Select(d => d.GetMessage())]);
    }
}