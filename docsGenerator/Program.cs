using System;

namespace DocsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = CodeFile.FromSourceCodeFile("../src/Modules/DownloadModule.cs");
            code.GetPropertiesWithComments();
        }
    }
}
