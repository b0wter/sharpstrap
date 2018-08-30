using System;

namespace DocsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = ClassCodeInfo.FromSourceCodeFile("../src/Modules/DownloadModule.cs");
            code.Foo();
        }
    }
}
