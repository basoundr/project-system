// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.IO;
using Microsoft.VisualStudio.ProjectSystem.LanguageServices;

namespace Microsoft.VisualStudio.ProjectSystem.VS.LanguageServices
{
    /// <summary>
    /// This provider adds references to the Runtime Assmemblies, mscorlib.dll and Microsoft.VisualBasic.dll,
    /// to the Framework VB projects
    /// </summary>
    internal class VisualBasicRuntimeReferencesProvider : OnceInitializedOnceDisposedAsync
    {
        private readonly ActiveConfiguredProject<ProjectProperties> _activeConfiguredProjectProperties;
        private readonly ILanguageServiceHost _languageServiceHost;
        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public VisualBasicRuntimeReferencesProvider(
            ActiveConfiguredProject<ProjectProperties> activeConfiguredProjectProperties,
            IUnconfiguredProjectVsServices projectServices,
            ILanguageServiceHost languageServiceHost,
            IFileSystem fileSystem)
            : base(projectServices.ThreadingService.JoinableTaskContext)
        {
            Requires.NotNull(activeConfiguredProjectProperties, nameof(activeConfiguredProjectProperties));
            Requires.NotNull(languageServiceHost, nameof(languageServiceHost));
            Requires.NotNull(fileSystem, nameof(fileSystem));

            _activeConfiguredProjectProperties = activeConfiguredProjectProperties;
            _languageServiceHost = languageServiceHost;
            _fileSystem = fileSystem;
        }

        [ProjectAutoLoad(startAfter: ProjectLoadCheckpoint.ProjectFactoryCompleted)]
        [AppliesTo(ProjectCapability.VisualBasic)]
        public Task Load()
        {
            return InitializeAsync();
        }

        protected override Task DisposeCoreAsync(bool initialized)
        {
            return Task.CompletedTask;
        }

        protected async override Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            var configurationGeneral = await _activeConfiguredProjectProperties.Value.GetConfigurationGeneralPropertiesAsync().ConfigureAwait(false);
            var sdkPath = await configurationGeneral.FrameworkPathOverride.GetEvaluatedValueAtEndAsync().ConfigureAwait(false);

            // Net Core Projects set the property FrameworkPathOverride to nothing. This property is available only for Framework projects
            if (string.IsNullOrEmpty(sdkPath))
            {
                return;
            }

            await AddMetadataReferenceAsync(sdkPath, "mscorlib.dll").ConfigureAwait(false);
            await AddMetadataReferenceAsync(sdkPath, "Microsoft.VisualBasic.dll").ConfigureAwait(false);
        }

        private async Task AddMetadataReferenceAsync(string sdkPath, string assemblyName)
        {
            var assemblyPath = PathHelper.Combine(sdkPath, assemblyName);
            if (_fileSystem.FileExists(assemblyPath))
            {
                await _languageServiceHost.InitializationCompletionTask.ContinueWith(
                    t => _languageServiceHost.ActiveProjectContext.AddMetadataReference(assemblyPath, MetadataReferenceProperties.Assembly), TaskScheduler.Default);
            }
        }
    }
}
