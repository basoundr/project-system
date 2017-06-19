﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Build;
using Microsoft.VisualStudio.Telemetry;
using System;
using System.Threading;

namespace Microsoft.VisualStudio.ProjectSystem.Configuration
{
    /// <summary>
    /// Base project configuration dimension provider
    /// </summary>
    internal abstract class BaseProjectConfigurationDimensionProvider : OnceInitializedOnceDisposedAsync, IProjectConfigurationDimensionsProvider2
    {
        protected const string TelemetryEventName = "DimensionChanged";

        private readonly bool _valueContainsPii;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProjectConfigurationDimensionProvider"/> class.
        /// </summary>
        /// <param name="projectXmlAccessor">Lock service for the project file.</param>
        /// <param name="telemetryService">Telemetry service.</param>
        /// <param name="unconfiguredProjectCommonServices">UnconfiguredProject Common Services.</param>
        /// <param name="dimensionName">Name of the dimension.</param>
        /// <param name="propertyName">Name of the project property containing the dimension values.</param>
        /// <param name="valueContainsPii">Says if the dimension values contain PII and needs to hashed before being reported to Telemetry</param>
        public BaseProjectConfigurationDimensionProvider(
            IProjectXmlAccessor projectXmlAccessor,
            ITelemetryService telemetryService,
            IUnconfiguredProjectCommonServices unconfiguredProjectCommonServices,
            string dimensionName,
            string propertyName,
            bool valueContainsPii)
             : base(unconfiguredProjectCommonServices.ThreadingService.JoinableTaskContext)
        {
            Requires.NotNull(projectXmlAccessor, nameof(projectXmlAccessor));

            ProjectXmlAccessor = projectXmlAccessor;
            TelemetryService = telemetryService;
            DimensionName = dimensionName;
            PropertyName = propertyName;
            UnconfiguredProjectCommonServices = unconfiguredProjectCommonServices;

            _valueContainsPii = valueContainsPii;
        }

        public string DimensionName
        {
            get;
        }

        public string PropertyName
        {
            get;
        }

        public IUnconfiguredProjectCommonServices UnconfiguredProjectCommonServices
        {
            get;
        }

        public IProjectXmlAccessor ProjectXmlAccessor
        {
            get;
        }

        public ITelemetryService TelemetryService
        {
            get;
        }
        public Guid Guid
        {
            get;
            set;
        }

        protected override Task DisposeCoreAsync(bool initialized)
        {
            return Task.CompletedTask;
        }

        protected override async Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            var configurationGeneralProperties = await UnconfiguredProjectCommonServices
                                                    .ActiveConfiguredProjectProperties
                                                    .GetConfigurationGeneralPropertiesAsync()
                                                    .ConfigureAwait(false);
            if (Guid.TryParse((string)await configurationGeneralProperties.ProjectGuid.GetValueAsync().ConfigureAwait(false), out Guid guid))
            {
                Guid = guid;
            }
        }

        /// <summary>
        /// Gets the property values for the dimension.
        /// </summary>
        /// <param name="unconfiguredProject">Unconfigured project.</param>
        /// <returns>Collection of values for the dimension.</returns>
        /// <remarks>
        /// From <see cref="IProjectConfigurationDimensionsProvider"/>.
        /// </remarks>
        protected virtual async Task<ImmutableArray<string>> GetOrderedPropertyValuesAsync(UnconfiguredProject unconfiguredProject)
        {
            Requires.NotNull(unconfiguredProject, nameof(unconfiguredProject));

            string propertyValue = await GetPropertyValue(unconfiguredProject).ConfigureAwait(true);
            if (propertyValue == null || string.IsNullOrEmpty(propertyValue))
            {
                return ImmutableArray<string>.Empty;
            }
            else
            {
                return BuildUtilities.GetPropertyValues(propertyValue);
            }
        }

        /// <summary>
        /// Gets the defaults values for project configuration dimensions for the given unconfigured project.
        /// </summary>
        /// <param name="unconfiguredProject">Unconfigured project.</param>
        /// <returns>Collection of key/value pairs for the defaults values for the configuration dimensions of this provider for given project.</returns>
        /// <remarks>
        /// From <see cref="IProjectConfigurationDimensionsProvider"/>.
        /// The interface expectes a collection of key/value pairs containing one or more dimensions along with a single values for each
        /// dimension. In this implementation each provider is representing a single dimension.
        /// </remarks>
        public virtual async Task<IEnumerable<KeyValuePair<string, string>>> GetDefaultValuesForDimensionsAsync(UnconfiguredProject unconfiguredProject)
        {
            Requires.NotNull(unconfiguredProject, nameof(unconfiguredProject));

            var values = await GetOrderedPropertyValuesAsync(unconfiguredProject).ConfigureAwait(false);
            if (values.IsEmpty)
            {
                return ImmutableArray<KeyValuePair<string, string>>.Empty;
            }
            else
            {
                // First value is the default one.
                var defaultValues = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>();
                defaultValues.Add(new KeyValuePair<string, string>(DimensionName, values.First()));
                return defaultValues.ToImmutable();
            }
        }

        /// <summary>
        /// Gets the project configuration dimension and values represented by this provider for the given unconfigured project.
        /// </summary>
        /// <param name="unconfiguredProject">Unconfigured project.</param>
        /// <returns>Collection of key/value pairs for the current values for the configuration dimensions of this provider for given project.</returns>
        /// <remarks>
        /// From <see cref="IProjectConfigurationDimensionsProvider"/>.
        /// The interface expectes a collection of key/value pairs containing one or more dimensions along with the values for each
        /// dimension. In this implementation each provider is representing a single dimension with one or more values.
        /// </remarks>
        public virtual async Task<IEnumerable<KeyValuePair<string, IEnumerable<string>>>> GetProjectConfigurationDimensionsAsync(UnconfiguredProject unconfiguredProject)
        {
            Requires.NotNull(unconfiguredProject, nameof(unconfiguredProject));

            var values = await GetOrderedPropertyValuesAsync(unconfiguredProject).ConfigureAwait(false);
            if (values.IsEmpty)
            {
                return ImmutableArray<KeyValuePair<string, IEnumerable<string>>>.Empty;
            }
            else
            {
                var dimensionValues = ImmutableArray.CreateBuilder<KeyValuePair<string, IEnumerable<string>>>();
                dimensionValues.Add(new KeyValuePair<string, IEnumerable<string>>(DimensionName, values));
                return dimensionValues.ToImmutable();
            }
        }

        /// <summary>
        /// Modifies the project when there's a configuration change.
        /// </summary>
        /// <param name="args">Information about the configuration dimension value change.</param>
        /// <returns>A task for the async operation.</returns>
        /// From <see cref="IProjectConfigurationDimensionsProvider2"/>.
        public abstract Task OnDimensionValueChangedAsync(ProjectConfigurationDimensionValueChangedEventArgs args);

        /// <summary>
        /// Gets the property value for the dimension property of the specified project.
        /// </summary>
        /// <param name="unconfiguredProject">Unconfigured project.</param>
        /// <returns>Value of the dimension property.</returns>
        /// <remarks>
        /// This needs to get the evaluated property in order to get inherited properties defines in props or targets.
        /// </remarks>
        protected async Task<string> GetPropertyValue(UnconfiguredProject unconfiguredProject)
        {
            return await ProjectXmlAccessor.GetEvaluatedPropertyValue(unconfiguredProject, PropertyName).ConfigureAwait(false);
        }

        protected string HashValueIfNeeded(string value)
        {
            return _valueContainsPii ? TelemetryService.HashValue(value) : value;
        }
    }
}
