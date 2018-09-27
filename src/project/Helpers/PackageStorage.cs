using System;
using System.Collections.Generic;
using System.Linq;
using SharpStrap.Modules;

namespace SharpStrap.Helpers
{
    public enum PackageEvaluationStates
    {
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
        public Dictionary<PackageEvaluationStates, IList<Package>> PackagePool { get; private set; }

        /// <summary>
        /// Reads the given log file and moves set packages to successful.
        /// </summary>
        /// <param name="successLogFilename"></param>
        public void InitFromLogFile(string successLogFilename)
        {
            
        }

        /* 
        /// <summary>
        /// Returns a package whose requirements have been met. If there are no packages left null will be returned.
        /// </summary>
        /// <exception cref="ArgumentException">If there is no package left whose requirements have been met.</exception>
        /// <returns>Package whose requirements have been met.</returns>
        public Package GetNextPackage()
        {
            if(PackagePool.Count == 0)
                return null;

            int counter = this.PackagePool.Count;;
            while(counter >= 0)
            {
                var state = CheckPackageState(this.PackagePool[counter]);
                switch(state)
                {
                    case PackageEvaluationStates.Unresolvable:
                        this.UnresolvablePackages.Add(this.PackagePool[counter]);
                        this.PackagePool.RemoveAt(counter);
                        counter--;
                        break;
                    case PackageEvaluationStates.UnmetDependency:
                        counter--;
                        break;
                    case PackageEvaluationStates.Ready:
                        return this.PackagePool[counter];
                }
            }

            // this point should only be reached if there are unresolvable packages
            MoveRemainingPoolPackagesToUnsolvable();
            var aggregatedUnresolvableNames = string.Join(", ", this.UnresolvablePackages.Select(p => p.Name));
            throw new ArgumentException($"There are packages whose requirements cannot be met: {aggregatedUnresolvableNames}.");
        }

        private void FailWithUnresovablePackages()
        {
            MoveRemainingPoolPackagesToUnsolvable();
            var aggregatedUnresolvableNames = string.Join(", ", UnresolvablePackages.Select(p => p.Name));
            throw new ArgumentException($"There are packages whose requirements cannot be met: {aggregatedUnresolvableNames}.");
        }

        private void MoveRemainingPoolPackagesToUnsolvable()
        {
            for(int i = PackagePool.Count - 1; i >= 0; --i)
            {
                UnresolvablePackages.Add(PackagePool[i]);
                PackagePool.RemoveAt(i);
            }
        }

        private PackageEvaluationStates CheckPackageState(Package package)
        {
            if(package.Requires.Count() == 0)
                return PackageEvaluationStates.Ready;

            if(package.Requires
                      .Any( r => this.UnresolvablePackages.Any(p => p.Name == r) || this.FailedPackages.Any(p => p.Name == r))) 
            {
                // At least one requirement for this package is unsatisfiable.
                return PackageEvaluationStates.Unresolvable;
            }

            var solvedPackageNames = this.SolvedPackages.Select(p => p.Name);
            if(package.Requires.Except(solvedPackageNames).Count() == 0)
                return PackageEvaluationStates.Ready;
            else
                return PackageEvaluationStates.UnmetDependency;
        }

        public static PackageStorage FromFiles(IEnumerable<Package> packages, string successLog, string errorLog)
        {
            var successPackages = new List<Package>();
            foreach(var )
        }

        private static string[] GetPackageNamesFromFile(string filename)
        {

        }*/
    }
}