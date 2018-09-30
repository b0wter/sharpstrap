using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FakeItEasy;
using FakeItEasy.Sdk;
using FluentAssertions;
using SharpStrap.Helpers;
using SharpStrap.Modules;
using Tests.Modules.Shims;
using Xunit;

namespace Tests.Helpers
{
    public class PackageStorageTests
    {
        private const string DummyPackageNameTemplate = "Package #{0}";
        private const int DummyPackageCount = 5;
        
        private IEnumerable<Package> CreateDefaultPackages()
        {
            var packages = new List<Package>(DummyPackageCount);
            
            for(int i = 0; i < DummyPackageCount; ++i)
                packages.Add(new Package()
                {
                    Name = string.Format(DummyPackageNameTemplate, i),
                    Description = "Dummy package for unit tests."
                });

            return packages;
        }
        
        [Fact]
        public void InitFromLogFileAndPackages_WithNullPackageNames_AddsAllPackagesNotEvaluated()
        {
            var packages = CreateDefaultPackages();
            var textOutput = A.Fake<ITextFileOutput>();
            var storage = new TestablePackageStorage(textOutput, null, packages);

            storage.OrderedPackages[PackageEvaluationStates.NotEvaluated].Count.Should().Be(packages.Count());
        }
        
        [Fact]
        public void InitFromLogFileAndPackages_WithEmptyPackageNames_AddsAllPackagesAsNotEvaluated()
        {
            var packages = CreateDefaultPackages();
            var textOutput = A.Fake<ITextFileOutput>();
            var storage = new TestablePackageStorage(textOutput, new string[0], packages);

            storage.OrderedPackages[PackageEvaluationStates.NotEvaluated].Count.Should().Be(packages.Count());
        }

        [Fact]
        public void InitFromLogFileAndPackages_WithPackageNamesAndContents_AddsPackagesAsSolved()
        {
            var packages = CreateDefaultPackages();
            var solvedPackages = packages.Count() / 2;
            var unsolvedPackages = packages.Count() - solvedPackages;
            var textOutput = A.Fake<ITextFileOutput>();
            var successfulPackageNames = packages.Take(solvedPackages).Select(x => x.Name).ToArray();
            var storage = new TestablePackageStorage(textOutput, successfulPackageNames, packages);

            storage.OrderedPackages[PackageEvaluationStates.Solved].Count.Should().Be(solvedPackages);
            storage.OrderedPackages[PackageEvaluationStates.NotEvaluated].Count.Should().Be(unsolvedPackages);
        }
        
        [Fact]
        public void InitFromLogFileAndPackages_WithDifferentPackageNames_AddsAllPackagesAsNotEvaluated()
        {
            var packages = CreateDefaultPackages();
            var textOutput = A.Fake<ITextFileOutput>();
            var successfulPackageNames = packages.Select(p => p.Name + "ABC").ToArray();
            var storage = new TestablePackageStorage(textOutput, successfulPackageNames, packages);

            storage.OrderedPackages[PackageEvaluationStates.Solved].Count.Should().Be(0);
            storage.OrderedPackages[PackageEvaluationStates.NotEvaluated].Count.Should().Be(packages.Count());
        }

        /*
            In its current state this test is broken as it requires the MarkPackageSolved-method.
         
        [Fact]
        public void GetNextPackage_WithCorrectSetup_CanBeCalledNumberOfPackagesTimes()
        {
            var packages = CreateDefaultPackages();
            var textOutput = A.Fake<ITextFileOutput>();
            var storage = new TestablePackageStorage(textOutput, new string[0], packages);

            int counter = 0;
            Package p;
            while ((p = storage.GetNextPackage()) != null &&
                   counter < packages.Count() + 2) // prevent infinite loops
            {
                storage.MarkPackageSolved(p);
                counter++;
            }

            counter.Should().Be(packages.Count());
        }
        */

        [Fact]
        public void GetNextPackage_WithNoPackages_ReturnsNull()
        {
            var packages = new List<Package>(0);
            var textOutput = A.Fake<ITextFileOutput>();
            var storage = new TestablePackageStorage(textOutput, new string[0], new Package[0]);

            var result = storage.GetNextPackage();

            result.Should().BeNull();
        }

        [Fact]
        public void GetNextPackage_WithUnmetRequirements_ThrowsArgumentException()
        {
            var packages = new List<Package>
            {
                new Package
                {
                    Name = "DummyPackage #1",
                    Requires = new string[] { "non-existing-package" },
                }
            };
            var textOutput = A.Fake<ITextFileOutput>();
            var storage = new TestablePackageStorage(textOutput, new string[0], packages);
            
            var result = storage.GetNextPackage();

            result.Should().BeNull();
            storage.OrderedPackages[PackageEvaluationStates.UnmetDependency].Should().Contain(packages.First());
            storage.OrderedPackages[PackageEvaluationStates.NotEvaluated].Should().NotContain(packages.First());
        }

        [Fact]
        public void MarkPackageSolved_WithNullPackage_ThrowsArgumentException()
        {
            var packages = CreateDefaultPackages();
            var textOutput = A.Fake<ITextFileOutput>();
            var storage = new TestablePackageStorage(textOutput, new string[0], packages);
            
            Assert.Throws<ArgumentException>(() => storage.MarkPackageSolved(null)).Message.Should().Contain("Cannot mark 'null' as solved.");
        }

        [Fact]
        public void MarkPackageSolved_WithNonExistingPackage_ThrowsArgumentException()
        {
            var packages = CreateDefaultPackages();
            var textOutput = A.Fake<ITextFileOutput>();
            var storage = new TestablePackageStorage(textOutput, new string[0], packages);

            Assert.Throws<ArgumentException>(() => storage.MarkPackageSolved(new Package())).Message.Should().Contain("does not exist in the storage.");
        }

        [Fact]
        public void MarkPackageSolved_WithPackageInCorrectState_Runs()
        {
            var packages = CreateDefaultPackages().ToArray();
            var textOutput = A.Fake<ITextFileOutput>();
            var storage = new TestablePackageStorage(textOutput, new string[0], packages);

            var p = storage.GetNextPackage();
            storage.MarkPackageSolved(packages.First());

            storage.OrderedPackages[PackageEvaluationStates.Solved].Should().Contain(packages.First());
        }
        
        // TODO: Add test for LogResult.
    }
}