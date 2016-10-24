using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Build
{
    internal class DesignTimeBuildErrorTableEntry : ITableEntry
    {
        internal static readonly ImmutableArray<string> SupportedColumnNames =
            ImmutableArray.Create(
                StandardTableColumnDefinitions.ErrorSource,
                StandardTableColumnDefinitions.ProjectName,
                StandardTableColumnDefinitions.Text,
                StandardTableColumnDefinitions.ErrorSeverity);

        private LazyFormattedBuildEventArgs _eventArgs;
        private ImmutableDictionary<String, Object> _properties;

        public DesignTimeBuildErrorTableEntry(ImmutableDictionary<String, Object> properties, LazyFormattedBuildEventArgs eventArgs)
        {
            this._properties = properties;
            this._eventArgs = eventArgs;
        }

        public Object Identity
        {
            get
            {
                return null;
            }
        }

        public Boolean CanSetValue(String keyName)
        {
            return false;
        }

        public Boolean TryGetValue(String keyName, out Object content)
        {
            if (_properties.TryGetValue(keyName, out content))
            {
                return true;
            }

            return false;
        }

        public Boolean TrySetValue(String keyName, Object content)
        {
            return false;
        }

        internal static ITableEntry CreateEntry(DesignTimeBuildErrorsTableDataSource designTimeBuildErrorsTableDataSource, BuildErrorEventArgs buildErrorArgs)
        {
            var properties = new Dictionary<string, object>();
            properties.Add(StandardTableColumnDefinitions.ErrorSource, ErrorSource.Other);
            properties.Add(StandardTableColumnDefinitions.ProjectName, designTimeBuildErrorsTableDataSource.ProjectPath);
            properties.Add(StandardTableColumnDefinitions.Text, buildErrorArgs.Message);
            properties.Add(StandardTableColumnDefinitions.ErrorSeverity, __VSERRORCATEGORY.EC_ERROR);
            properties.Add("project", designTimeBuildErrorsTableDataSource.Hierarchy);
            properties.Add("projectguid", designTimeBuildErrorsTableDataSource.ProjectGuid);

            return new DesignTimeBuildErrorTableEntry(properties.ToImmutableDictionary(), buildErrorArgs);
        }

        internal static ITableEntry CreateEntry(DesignTimeBuildErrorsTableDataSource designTimeBuildErrorsTableDataSource, BuildWarningEventArgs buildWarningArgs)
        {
            var properties = new Dictionary<string, object>();
            properties.Add(StandardTableColumnDefinitions.ErrorSource, ErrorSource.Other);
            properties.Add(StandardTableColumnDefinitions.ProjectName, designTimeBuildErrorsTableDataSource.ProjectPath);
            properties.Add(StandardTableColumnDefinitions.Text, buildWarningArgs.Message);
            properties.Add(StandardTableColumnDefinitions.ErrorSeverity, __VSERRORCATEGORY.EC_WARNING);
            properties.Add("project", designTimeBuildErrorsTableDataSource.Hierarchy);
            properties.Add("projectguid", designTimeBuildErrorsTableDataSource.ProjectGuid);

            return new DesignTimeBuildErrorTableEntry(properties.ToImmutableDictionary(), buildWarningArgs);
        }
    }
}
