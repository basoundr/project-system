// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.IO;
using Microsoft.VisualStudio.ProjectSystem.LanguageServices;
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.VS.LanguageServices
{
    [ProjectSystemTrait]
    public class VisualBasicRuntimeReferencesProviderTests
    {
        [Fact]
        public void Constructor_NullAsActiveConfiguredProjectProperties_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("activeConfiguredProjectProperties", () =>
            {
                CreateInstance(null, IUnconfiguredProjectVsServicesFactory.Implement());
            });
        }

        [Fact]
        public void Constructor_NullAsLanguageServiceHost_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("languageServiceHost", () =>
            {
                var activeConfiguredProjectProperties = ActiveConfiguredProjectFactory.ImplementValue(() => (ProjectProperties)null);
                var projectServices = IUnconfiguredProjectVsServicesFactory.Implement();
                CreateInstance(activeConfiguredProjectProperties, projectServices);
            });
        }

        [Fact]
        public void Constructor_NullAsFileSystem_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("fileSystem", () =>
            {
                var activeConfiguredProjectProperties = ActiveConfiguredProjectFactory.ImplementValue(() => (ProjectProperties)null);
                var projectServices = IUnconfiguredProjectVsServicesFactory.Implement();
                var languageServiceHost = ILanguageServiceHostFactory.Create();
                CreateInstance(activeConfiguredProjectProperties, projectServices, languageServiceHost);
            });
        }

        [Fact]
        public void ConstructorNotNull()
        {
            var activeConfiguredProjectProperties = ActiveConfiguredProjectFactory.ImplementValue(() => (ProjectProperties)null);
            var projectServices = IUnconfiguredProjectVsServicesFactory.Implement();
            var languageServiceHost = ILanguageServiceHostFactory.Create();
            var fileSystem = IFileSystemFactory.Create();
            CreateInstance(activeConfiguredProjectProperties, projectServices, languageServiceHost, fileSystem);
        }

        [InlineData(@"", new string[] { }, false)]
        [InlineData(@"C:\temp\sdk\path", new string[] { @"C:\temp\sdk\path\mscorlib.dll", @"C:\temp\sdk\path\Microsoft.VisualBasic.dll"}, true)]
        [InlineData(@"C:\temp\sdk\wrong\path", new string[] { @"C:\temp\sdk\path\mscorlib.dll", @"C:\temp\sdk\path\Microsoft.VisualBasic.dll"}, false)]
        [Theory]
        public async Task TestTryAddingRuntimeReferences(
            string sdkPath,
            string[] referencesFilesPresent,
            bool found
            )
        {
            var project = UnconfiguredProjectFactory.Create(filePath: @"C:\Myproject.vbproj");
            var data = new PropertyPageData()
            {
                Category = ConfigurationGeneral.SchemaName,
                PropertyName = ConfigurationGeneral.FrameworkPathOverrideProperty,
                Value = sdkPath,
            };

            var projectProperties = ProjectPropertiesFactory.Create(project, data);
            var activeConfiguredProject = ActiveConfiguredProjectFactory.ImplementValue(() => projectProperties);
            var projectServices = IUnconfiguredProjectVsServicesFactory.Implement();

            // Setup Context
            var referencesPushedToWorkspace = new List<string>();
            Action<string> onReferenceAdded = s => referencesPushedToWorkspace.Add(s);
            Action<string> onReferenceRemoved = s => referencesPushedToWorkspace.Remove(s);
            var context = IWorkspaceProjectContextFactory.CreateForMetadataReferences(project, onReferenceAdded, onReferenceRemoved);
            
            // Setup ILanguageServiceHost
            var languageServiceHost = ILanguageServiceHostFactory.Implement(context, Task.CompletedTask);

            // Setup file system
            var fileSystem = new IFileSystemMock();
            foreach (var file in referencesFilesPresent)
            {
                fileSystem.Create(file);
            }

            var runtimeProvider = CreateInstance(activeConfiguredProject, projectServices, languageServiceHost, fileSystem);
            await runtimeProvider.Load().ConfigureAwait(false);

            if (found)
            {
                Assert.True(referencesPushedToWorkspace.Count == referencesFilesPresent.Count());
                for (int i = 0; i < referencesPushedToWorkspace.Count; i++)
                {
                    Assert.True(string.Compare(referencesPushedToWorkspace[i], referencesFilesPresent[i], StringComparison.OrdinalIgnoreCase) == 0);
                }
            }
            else
            {
                Assert.True(referencesPushedToWorkspace.Count == 0);
            }
        }

        private VisualBasicRuntimeReferencesProvider CreateInstance(
            ActiveConfiguredProject<ProjectProperties> activeConfiguredProjectProperties = null,
            IUnconfiguredProjectVsServices projectServices = null,
            ILanguageServiceHost languageServiceHost = null,
            IFileSystem fileSystem = null)
        {
            return new VisualBasicRuntimeReferencesProvider(activeConfiguredProjectProperties, projectServices, languageServiceHost, fileSystem);
        }
    }
}
