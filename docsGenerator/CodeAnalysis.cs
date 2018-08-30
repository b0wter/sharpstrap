using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocsGenerator
{
    public class CodeFile
    {
        private SyntaxNode root;

        private CodeFile() { }

        public static CodeFile FromSourceCodeFile(string filename)
        {
            if(System.IO.File.Exists(filename) == false)
                throw new System.IO.FileNotFoundException($"The file '{filename}' could not be found.");

            var fileContent = System.IO.File.ReadAllText(filename);
            var tree = CSharpSyntaxTree.ParseText(fileContent);
            
            var code = new CodeFile();
            code.root = tree.GetRoot();
            return code;
        }

        public void GetPropertiesWithComments()
        {
            var root = (CompilationUnitSyntax) this.root;

            var classDeclarations = root.Members.Where(x => x.GetType().IsAssignableFrom(typeof(ClassDeclarationSyntax)));

            FindAllClassesInNode(root);
            foreach(var element in classDeclarations)
            {
                Console.WriteLine($"{element.GetText()}");
            }
            //var classNode = (ClassDeclarationSyntax) root.Members.First();

        }

        //private IList<ClassDeclarationSyntax> FindAllClassesInNode(SyntaxNode node)
        private void FindAllClassesInNode(SyntaxNode node)
        {
            foreach(var child in node.ChildNodes())
            {
                Console.WriteLine(child.Kind().ToString());
                FindAllClassesInNode(child);
            }

            /*
            var syntax = (CompilationUnitSyntax) node;

            if(syntax.Members.Count == 0)
            {
                return new List<ClassDeclarationSyntax>(1);
            }
            else
            {
                var temp = new List<ClassDeclarationSyntax>();
                foreach(var element in syntax.Members)
                    temp.AddRange(FindAllClassesInNode(element));
                return temp;
            }
            */
        }
    }
}