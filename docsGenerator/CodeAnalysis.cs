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
        private IEnumerable<ClassDeclarationSyntax> classSyntaxes;

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
            return code;
        }

        internal IEnumerable<ClassPropertyComment> GetPropertiesWithComments()
        {
            throw new NotImplementedException();
        }
    }
}