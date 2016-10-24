using System;
using Microsoft.Build.Framework;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Build
{
    internal class DesignTimeBuildLogger : ILogger
    {
        private DesignTimeBuildLoggerProvider _loggerProvider;
        private IEventSource _eventSource;
        
        public DesignTimeBuildLogger(DesignTimeBuildLoggerProvider designTimeBuildLoggerProvider)
        {
            this._loggerProvider = designTimeBuildLoggerProvider;
        }

        public String Parameters
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public LoggerVerbosity Verbosity
        {
            get
            {
                return LoggerVerbosity.Normal;
            }
            set
            {
            }
        }

        public void Initialize(IEventSource eventSource)
        {
            this._eventSource = eventSource;
            eventSource.AnyEventRaised += this.AnyEventRaisedHandler;
        }

        private void AnyEventRaisedHandler(Object sender, BuildEventArgs args)
        {
            BuildErrorEventArgs buildError;
            BuildWarningEventArgs buildWarning;
            BuildStartedEventArgs buildStarted;

            if ((buildStarted = args as BuildStartedEventArgs) != null)
            {
                _loggerProvider.DesignTimeBuildErrorsTableDataSource.RemoveAllEntries();
            }
            if ((buildError = args as BuildErrorEventArgs) != null)
            {
                var tableEntry = DesignTimeBuildErrorTableEntry.CreateEntry(
                    _loggerProvider.DesignTimeBuildErrorsTableDataSource,
                    buildError);
                _loggerProvider.DesignTimeBuildErrorsTableDataSource.AddEntry(tableEntry);
            }
            else if ((buildWarning = args as BuildWarningEventArgs) != null)
            {
                var tableEntry = DesignTimeBuildErrorTableEntry.CreateEntry(
                    _loggerProvider.DesignTimeBuildErrorsTableDataSource,
                    buildWarning);
                _loggerProvider.DesignTimeBuildErrorsTableDataSource.AddEntry(tableEntry);
            }

        }

        public void Shutdown()
        {
            _eventSource.AnyEventRaised -= this.AnyEventRaisedHandler;
        }
    }
}