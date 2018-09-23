using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpStrap.Helpers;
using SharpStrap.Modules;

namespace Tests.Modules
{
    public class ErrorModule : SharpStrap.Modules.BaseModule
    {
        public override async Task<ModuleResult> Run(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            await Task.Delay(1); // add something awaitable to make this an async task like all other tasks
            var result = new ModuleResult(ModuleResultStates.Error, new List<string>(), "no command run");
            return result;
        }
    }
}