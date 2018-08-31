using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocsGenerator
{
    internal class ClassCodeInfo
    {
        // A tutorial for the code analysis toolkit can be found here:
        // https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/get-started/semantic-analysis

        private IEnumerable<ClassDeclarationSyntax> classSyntaxes;
        private Compilation compilation;
        private SyntaxTree tree;
        private CompilationUnitSyntax root;

        private ClassCodeInfo() { }

        internal static ClassCodeInfo FromSourceCodeFile(string filename)
        {
            if(System.IO.File.Exists(filename) == false)
                throw new System.IO.FileNotFoundException($"The file '{filename}' could not be found.");

            var fileContent = AppendDummyMainToCodeFromFile(filename);
            var tree = CSharpSyntaxTree.ParseText(fileContent, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_1));
            var compilationRoot = tree.GetCompilationUnitRoot();

            var classes = compilationRoot
                            .Members
                            .OfType<NamespaceDeclarationSyntax>()
                            .SelectMany(
                                x => x.Members.OfType<ClassDeclarationSyntax>()
                            );

            var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = System.IO.Directory.GetParent(dd);
            var code = new ClassCodeInfo();
            code.classSyntaxes = classes;

            // TODO: check if any dll is necessary since the usings are now set in the compilation options
            code.compilation = CSharpCompilation
                                .Create("DocsGeneratorDynamic")
                                .AddSyntaxTrees(tree)
                                .AddReferences(
                                    MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                                    MetadataReference.CreateFromFile("../src/bin/Debug/netcoreapp2.0/sharpstrap.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "mscorlib.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Core.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Runtime.dll")
                                );

            var compilationResult = code.compilation.GetDiagnostics();

            code.root = compilationRoot;
            code.tree = tree;
            return code;
        }

        private static CSharpCompilationOptions CreateDefaultCompilerOptions()
        {
            string[] defaultNamespaces = new [] 
            {
                "System",
                "System.Linq",
                "System.IO",
                "System.Collection.Generic"
            };

            var options = (new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                            .WithUsings(defaultNamespaces);

            return options;
        }

        private static string AppendDummyMainToCodeFromFile(string filename)
        {
            // TODO: check if necessary as compilation is set to produce a dll
            var dummyMain = @"class DummyClass1234567890 { public static int Main() { return 0; } }";

            var content = System.IO.File.ReadAllLines(filename);

            int currentLine = content.Length - 1;
            
            while(currentLine >= 0 && content[currentLine].EndsWith('}') == false)
                currentLine--;

            content[currentLine] = content[currentLine].TrimEnd('}');

            var newContent = content.Take(currentLine).ToList();
            newContent.Add(dummyMain);
            newContent.Add("}");

            return string.Join(Environment.NewLine, newContent);
        }

        internal IEnumerable<ClassPropertyComment> GetPropertiesWithComments()
        {
            var model = this.compilation.GetSemanticModel(this.tree);
            var comments = new List<ClassPropertyComment>();
            
            var classes = this.root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach(var c in classes)
            {
                var symbol = model.GetDeclaredSymbol(c);
                Console.WriteLine(symbol.Name);

                var properties = c.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                foreach(var p in properties)
                {
                    var propertySymbol = model.GetDeclaredSymbol(p);
                    var comment = propertySymbol.GetDocumentationCommentXml();
                    comments.Add(new ClassPropertyComment{
                        ClassName = symbol.Name,
                        PropertyName = propertySymbol.Name,
                        Comment = comment
                    });
                }
            }

            return comments;
        }
    }
}