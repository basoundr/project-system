// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Build
{
    [AppliesTo(ProjectCapability.CSharpOrVisualBasic)]
    [Export(typeof(IBuildLoggerProviderAsync))]
    internal class DesignTimeBuildLoggerProvider : IBuildLoggerProviderAsync
    {

        [ImportingConstructor]
        public DesignTimeBuildLoggerProvider(
            UnconfiguredProject unconfiguredProject,
            ITableManagerProvider tableManagerProvider)
        {
            this.UnconfiguredProject = unconfiguredProject;
            this.VsHierarchies = new OrderPrecedenceImportCollection<IVsHierarchy>(
                projectCapabilityCheckProvider: unconfiguredProject);
            this.ProjectGuidServices = new OrderPrecedenceImportCollection<IProjectGuidService>(
                projectCapabilityCheckProvider: unconfiguredProject);

            var tableManager = tableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            var tableSource = new DesignTimeBuildErrorsTableDataSource(
                null,
                Guid.Empty,
                this.UnconfiguredProject.FullPath);
            tableManager.AddSource(tableSource, DesignTimeBuildErrorTableEntry.SupportedColumnNames);
            this.DesignTimeBuildErrorsTableDataSource = tableSource;
        }
        
        public DesignTimeBuildErrorsTableDataSource DesignTimeBuildErrorsTableDataSource
        {
            get;
        }

        [ImportMany]
        public OrderPrecedenceImportCollection<IProjectGuidService> ProjectGuidServices
        {
            get;
        }

        [ImportMany]
        public OrderPrecedenceImportCollection<IVsHierarchy> VsHierarchies
        {
            get;
        }
        public UnconfiguredProject UnconfiguredProject
        {
            get;
        }

        public Task<IImmutableSet<ILogger>> GetLoggersAsync(IReadOnlyList<String> targets, IImmutableDictionary<String, String> properties, CancellationToken cancellationToken)
        {
            var loggers = ImmutableHashSet<ILogger>.Empty;

            // Connect an MSBuild logger to the output window pane.
            var logger = new DesignTimeBuildLogger(this);
            loggers = loggers.Add(logger);

            return Task.FromResult((IImmutableSet<ILogger>)loggers);
        }
    }
}
