using System;
using System.Collections.Generic;
using System.Linq;
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

            var fileContent = System.IO.File.ReadAllText(filename);
            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var compilationRoot = tree.GetCompilationUnitRoot();

            var classes = compilationRoot
                            .Members
                            .OfType<NamespaceDeclarationSyntax>()
                            .SelectMany(
                                x => x.Members.OfType<ClassDeclarationSyntax>()
                            );

            var code = new ClassCodeInfo();
            code.classSyntaxes = classes;
            code.compilation = CSharpCompilation
                                .Create("DocsGeneratorDynamic")
                                .AddSyntaxTrees(tree)
                                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location));
            code.root = compilationRoot;
            code.tree = tree;
            return code;
        }

        internal IEnumerable<ClassPropertyComment> GetPropertiesWithComments()
        {
            return GetPropertiesWithComments(this.classSyntaxes);
        }

        internal void Foo()
        {
            var model = this.compilation.GetSemanticModel(this.tree);
            
            UsingDirectiveSyntax usingSystem = root.Usings[0];
            NameSyntax name = usingSystem.Name;
            SymbolInfo nameInfo = model.GetSymbolInfo(name);

            var systemSymbol = (INamespaceSymbol)nameInfo.Symbol;
            foreach(INamespaceSymbol ns in systemSymbol.GetNamespaceMembers())
                Console.WriteLine(ns);
        }

        private IEnumerable<ClassPropertyComment> GetPropertiesWithComments(IEnumerable<ClassDeclarationSyntax> classes)
        {
            var classPropertyComments = new List<ClassPropertyComment>();

            foreach(var item in classes)
            {
                var properties = item.Members.OfType<PropertyDeclarationSyntax>();

                foreach(var m in item.Members)
                    Console.WriteLine(m.GetType().Name);

                var cpc = new ClassPropertyComment
                {
                };
                classPropertyComments.Add(cpc);
            }

            return null;
        }
    }
}