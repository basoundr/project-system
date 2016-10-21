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

namespace Microsoft.VisualStudio.ProjectSystem.VS.Build
{
    [AppliesTo(ProjectCapability.CSharpOrVisualBasic)]
    [Export(typeof(IBuildLoggerProviderAsync))]
    internal class DesignTimeBuildLoggerProvider : IBuildLoggerProviderAsync
    {
        public async Task<IImmutableSet<ILogger>> GetLoggersAsync(IReadOnlyList<String> targets, IImmutableDictionary<String, String> properties, CancellationToken cancellationToken)
        {
            var loggers = ImmutableHashSet<ILogger>.Empty;

            // Connect an MSBuild logger to the output window pane.
            var logger = new DesignTimeBuildLogger();
            loggers = loggers.Add(logger);

            return loggers;
        }
    }
}
