using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpStrap.Modules;
using Tests.Modules;
using Xunit;

namespace Tests
{
    public class BootstrapTests
    {
        private Bootstrap CreateSuccessModuleBootstrap(int packageCount = 2, int modulePerPackageCount = 2)
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

        private Bootstrap CreateErrorModuleBootstrap(int packageCount = 2, int modulePerPackageCount = 2)
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
                ErrorLogFilename = "error.log",
                SuccessLogFilename = "success.log"
            };

            return bootstrap;
        }

        [Fact]
        public async Task Run_WithSuccessConfiguration_ReturnsSuccess()
        {
            var bootstrap = CreateSuccessModuleBootstrap();

            var result = await bootstrap.Run(null, null, null, null, false);

            Assert.True(result);
        }

        [Fact]
        public async Task Run_WithErrorConfiguration_ReturnsError()
        {
            var bootstrap = CreateErrorModuleBootstrap();

            var result = await bootstrap.Run(null, null, null, null, false);

            Assert.False(result);
        }

        [Fact]
        public async Task Run_WithSucessConfiguration_WritesSuccessLog()
        {
            var bootstrap = CreateSuccessModuleBootstrap();

            var result = await bootstrap.Run(null, null, null, false);

            
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
    }
}
