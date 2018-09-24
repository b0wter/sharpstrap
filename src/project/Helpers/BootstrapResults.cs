using System;
using System.Collections.Generic;
using System.Linq;
using SharpStrap.Modules;

namespace SharpStrap.Helpers
{
    public class BootstrapResults
    {
        /// <summary>
        /// Returns true if all packages have been run successfully.
        /// </summary>
        /// <param name="r.IsSuccess"></param>
        /// <returns></returns>
        public bool IsSuccess => results.All(r => r.IsSuccess == true);
        /// <summary>
        /// Number of packages that have been run successfully.
        /// </summary>
        /// <param name="r.IsSuccess"></param>
        /// <returns></returns>
        public int Successes => results.Count(r => r.IsSuccess);
        /// <summary>
        /// Nmber of packages that ran and failed.
        /// </summary>
        /// <param name="r.IsSuccess"></param>
        /// <returns></returns>
        public int Errors => results.Count(r => r.IsSuccess == false);
        /// <summary>
        /// Returns true if this instance contains at least one result.
        /// </summary>
        /// <returns></returns>
        public bool DidWork => results.Any();

        private List<BootstrapResult> results = new List<BootstrapResult>();

        public BootstrapResults()
        {

        }

        public void AddResult(BootstrapResult result)
        {
            results.Add(result);
        }

        public static BootstrapResults CreateEmpty()
        {
            return new BootstrapResults();
        }
    }

    public class BootstrapResult
    {
        /// <summary>
        /// Package whose result is stored.
        /// </summary>
        /// <value></value>
        public Package Package { get; }
        /// <summary>
        /// Returns wether the package run was a success.
        /// </summary>
        /// <value></value>
        public bool IsSuccess { get; }

        public BootstrapResult(Package package, bool isSuccess)
        {
            this.Package = package;
            this.IsSuccess = isSuccess;
        }
    }
}