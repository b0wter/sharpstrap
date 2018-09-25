using System;
using System.Collections.Generic;
using FluentAssertions;
using SharpStrap.Modules;
using Xunit;

namespace Tests.Modules
{
    public class PackageTests
    {
        #region CanRun Tests
        [Fact]
        public void CanRun_WithValidParameters_ReturnsTrue()
        {
            const string p1Name = "A1", p2Name = "B2", p3Name = "C3";
            var package = new Package
            {
                Name = "Dummy Package",
                Requires = new List<string> { p1Name, p2Name, p3Name }
            };
            var toTest = new List<string> { p1Name, p2Name, p3Name };

            var result = package.CanRun(toTest);

            result.Should().BeTrue();
        }

        [Fact]
        public void CanRun_WithTooManyParameters_ReturnsTrue()
        {
            const string p1Name = "A1", p2Name = "B2", p3Name = "C3";
            var package = new Package
            {
                Name = "Dummy Package",
                Requires = new List<string> { p1Name, p2Name, p3Name }
            };
            var toTest = new List<string> { p1Name, p2Name, p3Name, "extra 1", "extra 2" };

            var result = package.CanRun(toTest);

            result.Should().BeTrue();
        }

        [Fact]
        public void CanRun_WithInvalidParameters_ReturnsFalse()
        {
            const string p1Name = "A1", p2Name = "B2", p3Name = "C3";
            var package = new Package
            {
                Name = "Dummy Package",
                Requires = new List<string> { p1Name, p2Name, p3Name }
            };
            var toTest = new List<string> { p1Name, "wrong", p3Name };

            var result = package.CanRun(toTest);

            result.Should().BeFalse();
        }

        [Fact]
        public void CanRun_WithTooFewParameters_ReturnsFalse()
        {
            const string p1Name = "A1", p2Name = "B2", p3Name = "C3";
            var package = new Package
            {
                Name = "Dummy Package",
                Requires = new List<string> { p1Name, p2Name, p3Name }
            };
            var toTest = new List<string> { p1Name, p3Name };

            var result = package.CanRun(toTest);

            result.Should().BeFalse();
        }

        [Fact]
        public void CanRun_WithEmptyParameters_ReturnsTrue()
        {
            const string p1Name = "A1", p2Name = "B2", p3Name = "C3";
            var package = new Package
            {
                Name = "Dummy Package",
                Requires = new List<string>()
            };

            var result = package.CanRun(new List<string> { p1Name, p2Name, p3Name });

            result.Should().BeTrue();
        }
        #endregion
    }
}