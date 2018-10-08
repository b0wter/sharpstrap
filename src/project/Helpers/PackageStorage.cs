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
        Failed,
        /// <summary>
        /// The package has been solved during a previous run.
        /// </summary>
        PreviouslyRun
    }

    public class PackageStorage
    {
        private const PackageEvaluationStates DefaultPackageState = PackageEvaluationStates.NotEvaluated;
        public const PackageEvaluationStates DefaultSuccessSate = PackageEvaluationStates.Solved;
        public const PackageEvaluationStates DefaultFailedState = PackageEvaluationStates.Failed;
        
        /// <summary>
        /// Returns all packages.
        /// </summary>
        public IEnumerable<Package> All
        {
            get
            {
                var values = Enum.GetValues(typeof(PackageEvaluationStates)).Cast<PackageEvaluationStates>();
                return values.SelectMany(value => this.packagePool[value]);
            }
        }

        /// <summary>
        /// Returns all packages that have been run and succeeded.
        /// </summary>
        public IEnumerable<Package> Solved => packagePool[DefaultSuccessSate];

        /// <summary>
        /// Returns all packages that have not been solved yet (all except solved and previously run).
        /// </summary>
        public IEnumerable<Package> Unsolved => All.Except(Solved).Except(PreviouslyRun);

        /// <summary>
        /// Returns all packages that have been run previously.
        /// </summary>
        public IEnumerable<Package> PreviouslyRun => packagePool[PackageEvaluationStates.PreviouslyRun];

        protected readonly Dictionary<PackageEvaluationStates, IList<Package>> packagePool;

        public PackageStorage(IEnumerable<LogEntry> logEntries, IEnumerable<Package> packages)
        {
            // set private variables
            this.packagePool = new Dictionary<PackageEvaluationStates, IList<Package>>();
            
            // populate the dictionary with all values of the PackageEvaluationStates enum
            foreach (var value in Enum.GetValues(typeof(PackageEvaluationStates)).Cast<PackageEvaluationStates>())
                packagePool.Add(value, new List<Package>());
            
            // add the packages to the package pool
            if (logEntries == null)
                logEntries = new LogEntry[0];
            logEntries = logEntries.ToList();
            
            // assign a matching evaluation state to the packages
            foreach(var package in packages)
                if(logEntries.Any(entry => 
                        entry.Name == package.Name && 
                        string.Equals(entry.Status, DefaultSuccessSate.ToString(), StringComparison.InvariantCultureIgnoreCase)))
                    packagePool[PackageEvaluationStates.PreviouslyRun].Add(package);
                else if(logEntries.Any(entry => 
                        entry.Name == package.Name && 
                        string.Equals(entry.Status, PackageEvaluationStates.PreviouslyRun.ToString(), StringComparison.InvariantCultureIgnoreCase)))
                    packagePool[PackageEvaluationStates.PreviouslyRun].Add(package);
                else
                    packagePool[DefaultPackageState].Add(package);
        }
        
        /// <summary>
        /// Evaluates all packages in the pool and returns a package that is ready to run.
        /// Returns null if there is no package available.
        /// </summary>
        /// <returns></returns>
        public Package GetNextPackage()
        {
            EvaluatePackages();
            return this.packagePool[PackageEvaluationStates.Ready].FirstOrDefault();
        }
        
        /// <summary>
        /// Runs basic validation on the packages (dependency checking, setting fallback names).
        /// </summary>
        /// <returns></returns>
        public (bool, string) ValidatePackages()
        {
            AddMissingNamesForAllPackages();
            return CheckForNonExistingRequirements();
        }
        
        /// <summary>
        /// Adds placeholder names for packages which don't specify a name.
        /// </summary>
        private void AddMissingNamesForAllPackages()
        {
            var packagesWithoutName = this.All.Where(p => string.IsNullOrWhiteSpace(p.Name)).ToList();
            for(var i = 0; i < packagesWithoutName.Count; ++i)
                packagesWithoutName[i].Name = $"<Unnamed Package #{i}>";
        }
        
        /// <summary>
        /// Returns whether all requirements exist as packages in the current pool.
        /// </summary>
        /// <returns></returns>
        private (bool state, string reason) CheckForNonExistingRequirements()
        {
            var requirements = this.All.SelectMany(p => p.Requires).Distinct();
            var names = this.All.Select(p => p.Name);

            var nonExistingNames = requirements.Where(r => names.Contains(r) == false).ToList();

            if (nonExistingNames.Any())
                return (false,
                    $"The following requirements are listed but do not exist: {string.Join(", ", nonExistingNames)}.");
            else
                return (true, string.Empty);
        }

        /// <summary>
        /// Marks a package as solved in the internal package pool.
        /// </summary>
        /// <param name="p"></param>
        public void MarkPackageSolved(Package p)
        {
            MarkPackageAs(p, PackageEvaluationStates.Solved);
        }
        
        /// <summary>
        /// Marks a package as failed in the internal package pool.
        /// </summary>
        /// <param name="p"></param>
        public void MarkPackageFailed(Package p)
        {
            MarkPackageAs(p, PackageEvaluationStates.Failed);
        }
        
        /// <summary>
        /// Moves a package from one state to another.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="newState"></param>
        /// <exception cref="ArgumentException"></exception>
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
                var (found, state) = TryGetEvaluationStateForPackage(p, DefaultPackageState);
                if(found)
                    throw new ArgumentException($"The package '{p.Name}' is not in the 'Ready'-state and thus cannot be marked solved. It was found in the '{state}'-state.");
                else
                    throw new ArgumentException($"The package '{p.Name}' does not exist in the storage.");
            }
        }

        /// <summary>
        /// Check whether the dependencies for all packages can be met.
        /// </summary>
        /// <returns></returns>
        public (bool, string) DryRunDependencies()
        {
            var requirements = this.packagePool[PackageEvaluationStates.NotEvaluated].Select(p => (p.Name, p.Requires)).ToList();
            var solved = requirements.Where(r => r.Requires.Any() == false)
                                     .Union(this.packagePool[PackageEvaluationStates.PreviouslyRun]
                                     .Select(p => (p.Name, (IEnumerable<string>) (new string[0]))))
                                     .ToList();

            while(requirements.Count() != 0)
            {
                var solvable = requirements.Where(p => !p.Requires.Except(solved.Where(d => d.Name != null).Select(d => d.Name)).Any());
                if (!solvable.Any())
                    return (false, $"Requirements not met: {string.Join(", ", requirements.Select(r => r.Name))}");

                solved.AddRange(solvable);

                requirements.RemoveAll(r => solvable.Contains(r));
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Searches the complete packagePool dictionary for the state of a package. Returns false if the package is not found.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
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
                var (found, currentState) = TryGetEvaluationStateForPackage(package, DefaultPackageState);

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

        /// <summary>
        /// Refreshes the <see cref="DefaultPackageState"/> for the given package.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
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
        /// Gets a list of log entries based on whether they ran successfully or not.
        /// </summary>
        public IEnumerable<LogEntry> GetLogResult()
        {
            var success = this.packagePool[PackageEvaluationStates.Solved].Select(x => new LogEntry
                {Name = x.Name, Status = DefaultSuccessSate.ToString()});

            var nonFinishedStates = Enum.GetValues(typeof(PackageEvaluationStates))
                .Cast<PackageEvaluationStates>()
                .SelectMany(x => this.packagePool[x])
                .Select(x => new LogEntry { Name = x.Name, Status = DefaultFailedState.ToString()});

            return success.Union(nonFinishedStates);
        }
    }
}