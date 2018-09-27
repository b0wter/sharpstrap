using System;
using Ninject.Modules;

namespace SharpStrap.Helpers
{
    public class SharpStrapModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IIODefinition>().To<ConsoleIODefinition>().InSingletonScope();
            Bind<ITextFileInput>().To<FrameworkTextFileInput>().InSingletonScope();
            Bind<ITextFileOutput>().To<FrameworkTextFileOutput>().InSingletonScope();
            Bind<IModuleFinder>().ToMethod(x => ModuleFinder.CreateDefault()).InSingletonScope();
        }
    }
}