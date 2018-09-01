using System;
using System.Collections.Generic;
using SharpStrap.Helpers;

namespace SharpStrap.Modules
{
    public abstract class ServiceModule : ShellModule
    {
        protected const string ServiceCommand = "systemctl";

        protected abstract string ServiceSubCommand { get; }

        /// <summary>
        /// Names of the services to act on.
        /// </summary>
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
            this.SetCommandAndArguments(ServiceCommand, this.ServiceSubCommand + " " + String.Join(" ", this.ServiceNames));
        }
    }

    /// <summary>
    /// Starts a service.
    /// </summary>
    public class ServiceStartModule : ServiceModule
    {
        protected override string ServiceSubCommand => "start";

    }

    /// <summary>
    /// Stops a service.
    /// </summary>
    public class ServiceStopModule : ServiceModule
    {
        protected override string ServiceSubCommand => "stop";

    }
}
