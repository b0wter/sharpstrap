using System;
using System.Collections.Generic;
using Cootstrap.Helpers;

namespace Cootstrap.Modules
{
    public abstract class ServiceModule : ShellModule
    {
        protected const string ServiceCommand = "systemctl";

        protected abstract string ServiceSubCommand { get; }

        public IEnumerable<string> ServiceNames { get; set; } = new List<string>();

        public ServiceModule()
            : base()
        {
            //
        }

        public ServiceModule(params string[] serviceNames)
            : this()
        {
            this.ServiceNames = serviceNames;
        }

        protected override void PreExecution(IDictionary<string, string> variables, ColoredTextWriter output)
        {
            this.SetCommandAndArguments(ServiceCommand, this.ServiceSubCommand + String.Join(" ", this.ServiceNames));
        }
    }

    public class StartServiceModule : ServiceModule
    {
        protected override string ServiceSubCommand => "start";

    }

    public class StopServiceModule : ServiceModule
    {
        protected override string ServiceSubCommand => "stop";

    }
}
