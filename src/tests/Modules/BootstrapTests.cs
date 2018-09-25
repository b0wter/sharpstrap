using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using SharpStrap.Helpers;
using SharpStrap.Modules;
using Tests.Helpers;
using Tests.Modules;
using Xunit;

namespace Tests
{
    public class BootstrapTests
    {
        /* 
        private const string ErrorLogName = "error.log";
        private const string SuccessLogName = "succes.log";

        private Bootstrap CreateSuccessModuleBootstrap(int packageCount, int modulePerPackageCount)
        {
            var cascadedModules = new List<List<BaseModule>>(packageCount);
            for(int j = 0; j < packageCount; ++j)
            {
                var successModules = new List<BaseModule>(modulePerPackageCount);
                for(int i = 0; i < modulePerPackageCount; ++i)
                    successModules.Add(new SuccessModule());
                cascadedModules.Add(successModules);
            }
            return CreateBootstrapWithCascadedList(cascadedModules);
        }

        private Bootstrap CreateErrorModuleBootstrap(int packageCount, int modulePerPackageCount)
        {
            var cascadedModules = new List<List<BaseModule>>(packageCount);
            for(int j = 0; j < packageCount; ++j)
            {
                var errorModules = new List<BaseModule>(modulePerPackageCount);
                for(int i = 0; i < modulePerPackageCount; ++i)
                    errorModules.Add(new ErrorModule());
                cascadedModules.Add(errorModules);
            }
            return CreateBootstrapWithCascadedList(cascadedModules);
        }

        private Bootstrap CreateBootstrapWithCascadedList(IEnumerable<IEnumerable<BaseModule>> cascadedModules)
        {
            int packageCounter = 0;
            var packages = new List<Package>();
            foreach(var list in cascadedModules)
            {
                var package = new Package
                {
                    Name = $"Package #{packageCounter.ToString().PadLeft(3)}",
                    Modules = list
                };
                packageCounter++;
            }

            var bootstrap = new Bootstrap
            {
                Packages = packages,
                ErrorLogFilename = ErrorLogName,
                SuccessLogFilename = SuccessLogName
            };

            return bootstrap;
        }

        [Fact]
        public async Task Run_WithSuccessConfiguration_ReturnsSuccess()
        {
            int packageCount = 2; int modulePerPackageCount = 2;
            var bootstrap = CreateSuccessModuleBootstrap(packageCount, modulePerPackageCount);
            var input = A.Fake<IIODefinition>();
            A.CallTo(() => input.TextReader.Read()).Returns('y');
            var textInput = A.Fake<ITextFileInput>();
            var textOutput = A.Fake<ITextFileOutput>();

            var result = await bootstrap.Run(input, textInput, textOutput, false);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Run_WithErrorConfiguration_ReturnsError()
        {
            int packageCount = 2; int modulePerPackageCount = 2;
            var bootstrap = CreateErrorModuleBootstrap(packageCount, modulePerPackageCount);
            var input = A.Fake<IIODefinition>();
            A.CallTo(() => input.TextReader.Read()).Returns('y');
            var textInput = A.Fake<ITextFileInput>();
            var textOutput = A.Fake<ITextFileOutput>();

            var result = await bootstrap.Run(input, textInput, textOutput, false);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Run_WithSucessConfiguration_WritesSuccessLog()
        {
            int packageCount = 2;
            int moduleCount = 3;
            var bootstrap = CreateSuccessModuleBootstrap(packageCount, moduleCount);
            bootstrap.ErrorLogFilename = "error.log";
            var input = A.Fake<IIODefinition>();
            A.CallTo(() => input.TextReader.Read()).Returns('y');
            var textInput = new DummyTextFileInput(new string[] {});
            var textOutput = new DummyTextFileOutput();

            var result = await bootstrap.Run(input, textInput, textOutput, false);

            result.Should().Be(true);
            textOutput.Contents[ErrorLogName].Should().HaveLength(0);
            textOutput.Contents[SuccessLogName].Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Run_WithErrorConfiguration_WritesErrorLog()
        {

        }

        [Fact]
        public async Task Run_WithMixedConfiguration_WritesSuccessAndErrorLog()
        {

        }

        [Fact]
        public async Task Run_WithSuccessConfigurationTwice_WritesSuccessLog()
        {
            // IMPORTANT: this needs to run a mixture of failing and succeeding packages on the first run and only successfull packages on the second
        }
        */
    }
}
