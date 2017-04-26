// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServices.ProjectSystem;
using Moq;

namespace Microsoft.VisualStudio.ProjectSystem.LanguageServices
{
    internal static class ILanguageServiceHostFactory
    {
        public static ILanguageServiceHost Create()
        {
            return Mock.Of<ILanguageServiceHost>();
        }

        public static ILanguageServiceHost ImplementHostSpecificErrorReporter(Func<object> action)
        {
            var mock = new Mock<ILanguageServiceHost>();
            mock.SetupGet(h => h.HostSpecificErrorReporter)
                .Returns(action);

            return mock.Object;
        }

        public static ILanguageServiceHost Implement(
            IWorkspaceProjectContext projectContext = null,
            Task initializationCompletionTask = null)
        {
            var mock = new Mock<ILanguageServiceHost>();

            if (projectContext != null)
            {
                mock.SetupGet(h => h.ActiveProjectContext)
                    .Returns(projectContext);
            }

            if (initializationCompletionTask != null)
            {
                mock.SetupGet(h => h.InitializationCompletionTask)
                    .Returns(initializationCompletionTask);
            }

            return mock.Object;
        }
    }
}