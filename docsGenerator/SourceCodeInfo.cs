using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocsGenerator
{
    /// <summary>
    /// Uses Roslyn to create in-memory compilations of code files.
    /// </summary>
    internal class SourceCodeInfo
    {
        // A tutorial for the code analysis toolkit can be found here:
        // https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/get-started/semantic-analysis

        private IEnumerable<ClassDeclarationSyntax> classSyntaxes;
        private Compilation compilation;
        private SyntaxTree tree;
        private CompilationUnitSyntax root;

        /// <summary>
        /// List of error messages that are a result of the compilation process.
        /// </summary>
        public IReadOnlyCollection<string> ErrorMessages { private set; get; }
        /// <summary>
        /// Returns wether there are compilation errors.
        /// </summary>
        public bool HasErrors => ErrorMessages == null ? false : ErrorMessages.Count > 0;
        public IEnumerable<ClassComment> ClassComments { private set; get; }
        public IEnumerable<ClassPropertyComment> ClassPropertyComments { private set; get; }

        private SourceCodeInfo() { }

        /// <summary>
        /// Creates a class instance from a source code file.
        /// Will build a <see cref="Compilation"/>, a <see cref="SyntaxTree"> and a <see cref="CompilationUnitSyntax"/>.
        /// </summary>
        /// <param name="filename">Source code file.</param>
        /// <returns>Instance of ClassCodeInfo.</returns>
        internal static SourceCodeInfo FromSourceCodeFile(string filename)
        {
            if(System.IO.File.Exists(filename) == false)
                throw new System.IO.FileNotFoundException($"The file '{filename}' could not be found.");

            //var fileContent = AppendDummyMainToCodeFromFile(filename);
            var fileContent = System.IO.File.ReadAllText(filename);
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
            var code = new SourceCodeInfo();
            code.classSyntaxes = classes;

            code.compilation = CSharpCompilation
                                .Create("DocsGeneratorDynamic")
                                .WithOptions(CreateDefaultCompilerOptions())
                                .AddSyntaxTrees(tree)
                                .AddReferences(
                                    MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                                    MetadataReference.CreateFromFile("../src/bin/Debug/netcoreapp2.0/sharpstrap.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "mscorlib.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Core.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Linq.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Diagnostics.Process.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.IO.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.ComponentModel.Primitives.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.ComponentModel.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Collections.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Console.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Runtime.Extensions.dll"),
                                    MetadataReference.CreateFromFile(coreDir.FullName + System.IO.Path.DirectorySeparatorChar + "System.Runtime.dll")
                                );

            var compilationResult = code.compilation.GetDiagnostics();
            var compilationErrors = compilationResult.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage());
            
            code.ErrorMessages = new ReadOnlyCollection<string>(compilationErrors.ToList());
            code.root = compilationRoot;
            code.tree = tree;
            code.LoadClassAndPropertyComments();

            return code;
        }

        private static CSharpCompilationOptions CreateDefaultCompilerOptions()
        {
            var options = (new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
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

        private void LoadClassAndPropertyComments()
        {
            var model = this.compilation.GetSemanticModel(this.tree);
            var propertyComments = new List<ClassPropertyComment>();
            var classComments = new List<ClassComment>();
            
            var classes = this.root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach(var c in classes)
            {
                var symbol = model.GetDeclaredSymbol(c);
                Console.WriteLine(symbol.Name);
                classComments.Add(new ClassComment()
                {
                    ClassName = symbol.Name,
                    RawComment = symbol.GetDocumentationCommentXml(),
                    IsAbstract = symbol.IsAbstract,
                    Namespace = symbol.ContainingNamespace.Name
                });

                var properties = c.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                foreach(var p in properties)
                {
                    var propertySymbol = model.GetDeclaredSymbol(p);
                    var comment = propertySymbol.GetDocumentationCommentXml();
                    propertyComments.Add(new ClassPropertyComment{
                        ClassName = symbol.Name,
                        PropertyName = propertySymbol.Name,
                        RawComment = comment
                    });
                }
            }

            this.ClassComments = classComments;
            this.ClassPropertyComments = propertyComments;
        }
    }
}
