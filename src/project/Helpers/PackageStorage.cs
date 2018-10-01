using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SharpStrap.Modules;

namespace SharpStrap.Helpers
{
    public enum PackageEvaluationStates
    {
        /// <summary>
        /// Requirements have not been checked.
        /// </summary>
        NotEvaluated,
        /// <summary>
        /// Package can be run.
        /// </summary>
        Ready,
        /// <summary>
        /// Requirements have not been met but 
        /// </summary>
        UnmetDependency,
        /// <summary>
        /// There are unsatisfiable requirements.
        /// </summary>
        Unresolvable,
        /// <summary>
        /// The package has been run successfully.
        /// </summary>
        Solved,
        /// <summary>
        /// The package has been run and failed.
        /// </summary>
        Failed
    }

    public class PackageStorage
    {
        protected readonly Dictionary<PackageEvaluationStates, IList<Package>> packagePool;
        protected readonly ITextFileOutput textOutput;

        public PackageStorage(ITextFileOutput textOutput, string[] successfulPackageNames, IEnumerable<Package> packages)
        {
            // set private variables
            this.textOutput = textOutput;
            this.packagePool = new Dictionary<PackageEvaluationStates, IList<Package>>();
            
            // populate the dictionary with all values of the PackageEvaluationStates enum
            foreach (var value in Enum.GetValues(typeof(PackageEvaluationStates)).Cast<PackageEvaluationStates>())
                packagePool.Add(value, new List<Package>());
            
            // add the packages to the package pool
            if (successfulPackageNames == null)
                successfulPackageNames = new string[0];
            foreach(var package in packages)
                if(successfulPackageNames.Contains(package.Name))
                    packagePool[PackageEvaluationStates.Solved].Add(package);
                else
                    packagePool[PackageEvaluationStates.NotEvaluated].Add(package);
        }
        
        public Package GetNextPackage()
        {
            EvaluatePackages();
            return this.packagePool[PackageEvaluationStates.Ready].FirstOrDefault();
        }

        public void MarkPackageSolved(Package p)
        {
            MarkPackageAs(p, PackageEvaluationStates.Solved);
        }
        
        public void MarkPackageFailed(Package p)
        {
            MarkPackageAs(p, PackageEvaluationStates.Failed);
        }

        private void MarkPackageAs(Package p, PackageEvaluationStates newState)
        {
            if(p == null)
                throw new ArgumentException($"Cannot mark 'null' as solved.");
            
            if (this.packagePool[PackageEvaluationStates.Ready].Contains(p))
            {
                this.packagePool[PackageEvaluationStates.Ready].Remove(p);
                this.packagePool[newState].Add(p);
            }
            else
            {
                var (found, state) = TryGetEvaluationStateForPackage(p, PackageEvaluationStates.NotEvaluated);
                if(found)
                    throw new ArgumentException($"The package '{p.Name}' is not in the 'Ready'-state and thus cannot be marked solved. It was found in the '{state}'-state.");
                else
                    throw new ArgumentException($"The package '{p.Name}' does not exist in the storage.");
            }
        }

        private (bool, PackageEvaluationStates) TryGetEvaluationStateForPackage(Package p, PackageEvaluationStates fallback)
        {
            foreach(var state in Enum.GetValues(typeof(PackageEvaluationStates)).Cast<PackageEvaluationStates>())
                if (this.packagePool[state].Contains(p))
                    return (true, state);

            return (false, fallback);
        }

        /// <summary>
        /// Iterates through all remaining packages and checks if their requirements have been met. 
        /// </summary>
        private void EvaluatePackages()
        {
            var packagesToCheck = GetAllPackagesToCheck().ToArray();

            foreach (var package in packagesToCheck)
            {
                var newState = CheckPackageState(package);
                var (found, currentState) = TryGetEvaluationStateForPackage(package, PackageEvaluationStates.NotEvaluated);

                if (found)
                {
                    this.packagePool[currentState].Remove(package);                   
                    this.packagePool[newState].Add(package);
                }
                else
                {
                    throw new InvalidOperationException($"Encountered an error while trying to evaluate all package states: Could not find the current state for a package.");
                }
            }
        }

        /// <summary>
        /// Returns all packages that need to be run (Ready, NotEvaluated, UnmetDepedency).
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Package> GetAllPackagesToCheck()
        {
            return this.packagePool[PackageEvaluationStates.Ready]
                       .Union(packagePool[PackageEvaluationStates.NotEvaluated])
                       .Union(packagePool[PackageEvaluationStates.UnmetDependency]);
        }

        private PackageEvaluationStates CheckPackageState(Package package)
        {
            if(package.Requires.Any() == false)
                return PackageEvaluationStates.Ready;

            if(package.Requires
                      .Any( r => this.packagePool[PackageEvaluationStates.Unresolvable].Any(p => p.Name == r) || this.packagePool[PackageEvaluationStates.Failed].Any(p => p.Name == r))) 
            {
                // At least one requirement for this package is unsatisfiable.
                return PackageEvaluationStates.Unresolvable;
            }

            var solvedPackageNames = this.packagePool[PackageEvaluationStates.Solved].Select(p => p.Name);
            if(package.Requires.Except(solvedPackageNames).Any() == false)
                return PackageEvaluationStates.Ready;
            else
                return PackageEvaluationStates.UnmetDependency;
        }

        /// <summary>
        /// Writes the current state of the packages to the <see cref="ITextFileOutput"/>.
        /// </summary>
        public void LogResult(string filename)
        {
            // TODO: rethink this as it destroys the original stack traces!
            
            var errorLogs = new List<string>();
            foreach (var state in Enum.GetValues(typeof(PackageEvaluationStates)).Cast<PackageEvaluationStates>())
            {
                try
                {
                    var logName = filename + state.ToString();
                    this.textOutput.WriteAllLines(logName, this.packagePool[state].Select(p => p.Name));
                }
                catch
                {
                    errorLogs.Add(state.ToString());
                }
            }

            if (errorLogs.Any())
            {
                var joinedNames = string.Join(", ", errorLogs);
                throw new IOException($"The following logs could not be written: {joinedNames}.");
            }
        }
    }
}