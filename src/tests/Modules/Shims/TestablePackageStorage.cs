using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SharpStrap.Helpers;
using SharpStrap.Modules;

namespace Tests.Modules.Shims
{
    public class TestablePackageStorage : PackageStorage
    {
        public IDictionary<PackageEvaluationStates, IList<Package>> OrderedPackages => this.packagePool;

        public TestablePackageStorage(ITextFileOutput textOutput, string[] successfulPackageNames, IEnumerable<Package> packages) 
            : base("default.log.", textOutput, successfulPackageNames, packages)
        {
            //
        }
    }
}