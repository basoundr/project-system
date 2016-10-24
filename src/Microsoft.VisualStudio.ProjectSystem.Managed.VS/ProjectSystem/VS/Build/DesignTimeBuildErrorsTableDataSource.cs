using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Build
{
    internal class DesignTimeBuildErrorsTableDataSource : ITableDataSource
    {
        private ImmutableList<Subscription> subscriptions = ImmutableList.Create<Subscription>();

        public DesignTimeBuildErrorsTableDataSource(IVsHierarchy hierarchy, Guid projectGuid, string projectPath)
        {
            this.Hierarchy = hierarchy;
            this.ProjectGuid = projectGuid;
            this.ProjectPath = projectPath;
        }

        public String DisplayName
        {
            get
            {
                return VSResources.DesignTimeBuildErrors;
            }
        }

        public IVsHierarchy Hierarchy { get; }

        public String Identifier
        {
            get
            {
                return VSResources.DesignTimeBuildErrors;
            }
        }

        public Guid ProjectGuid { get; }
        public string ProjectPath { get; }

        public String SourceTypeIdentifier
        {
            get
            {
                return StandardTableDataSources.ErrorTableDataSource;
            }
        }

        protected IEnumerable<ITableDataSink> CurrentSubscribers
        {
            get { return this.subscriptions.Select(s => s.Receiver); }
        }

        IDisposable ITableDataSource.Subscribe(ITableDataSink sink)
        {
            Requires.NotNull(sink, nameof(sink));

            var subscription = new Subscription(this, sink);
            ThreadingTools.ApplyChangeOptimistically(
                ref this.subscriptions,
                s => s.Add(subscription));

            return subscription;
        }

        public void AddEntry(ITableEntry tableEntry)
        {
            Requires.NotNull(tableEntry, nameof(tableEntry));

            var tableEntries = ImmutableList.Create(tableEntry);
            foreach (var sink in this.CurrentSubscribers)
            {
                sink.AddEntries(tableEntries);
            }
        }

        public void RemoveAllEntries()
        {
            foreach (var sink in this.CurrentSubscribers)
            {
                sink.RemoveAllEntries();
            }
        }

        private void Unsubscribe(Subscription subscription)
        {
            Requires.NotNull(subscription, nameof(subscription));

            bool found = ThreadingTools.ApplyChangeOptimistically(
                ref this.subscriptions,
                s => s.Remove(subscription));
            Report.IfNot(found, "Subscription not found.");
        }

        private class Subscription : IDisposable
        {
            private readonly DesignTimeBuildErrorsTableDataSource source;

            internal Subscription(
                DesignTimeBuildErrorsTableDataSource source,
                ITableDataSink receiver)
            {
                Requires.NotNull(source, nameof(source));
                Requires.NotNull(receiver, nameof(receiver));

                this.source = source;
                this.Receiver = receiver;
            }

            internal ITableDataSink Receiver { get; private set; }

            public void Dispose()
            {
                this.source.Unsubscribe(this);
            }
        }
    }
}