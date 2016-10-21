using System;
using Microsoft.Build.Framework;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Build
{
    internal class DesignTimeBuildLogger : ILogger
    {
        public DesignTimeBuildLogger()
        {
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
            eventSource.AnyEventRaised += this.AnyEventRaisedHandler;
        }

        private void AnyEventRaisedHandler(Object sender, BuildEventArgs args)
        {
            BuildErrorEventArgs buildError;
            BuildWarningEventArgs buildWarning;

            if ((buildError = args as BuildErrorEventArgs) != null)
            {
                this.consoleLogger.ErrorHandler(null, buildError);
            }
            else if ((buildWarning = args as BuildWarningEventArgs) != null)
            {
                this.consoleLogger.WarningHandler(null, buildWarning);
            }

        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}