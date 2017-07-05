// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.IntegrationTest.Utilities;
using Microsoft.VisualStudio.ProjectSystem.IntegrationTests;
using Xunit;

namespace Microsoft.VisualStudio.CSharp
{
    [Collection(nameof(SharedIntegrationHostFixture))]
    public class CSharpDebugProfileTests : AbstractIntegrationTest
    {
        protected override string DefaultLanuageName => LanguageNames.CSharp;

        public CSharpDebugProfileTests(VisualStudioInstanceFactory instanceFactory)
            : base(nameof(CSharpDebugProfileTests), WellKnownProjectTemplates.CSharpNetCoreConsoleApplication, instanceFactory)
        {
        }

        [Fact, Trait("Integration", "DebugProfiles")]
        public void Summa()
        {
            VisualStudio.SolutionExplorer.EditProjectFile(Project);

            // Set the project to be Multi TFM
            VisualStudio.Editor.SetText(@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>netcoreapp1.1;net461</TargetFrameworks>
  </PropertyGroup>

</Project>");

            VisualStudio.SolutionExplorer.SaveAll();
            //VisualStudio.Dialog.VerifyOpen("Project Modification Detected");
            //VisualStudio.SendKeys.Send(VirtualKey.R);

            VisualStudio.SolutionExplorer.RestoreNuGetPackages();
            VisualStudio.Workspace.WaitForAsyncOperations(FeatureAttribute.SolutionCrawler);
            VisualStudio.WaitForApplicationIdle();

            var fileName = "Program.cs";
            VisualStudio.SolutionExplorer.OpenFile(Project, fileName);
            VisualStudio.Editor.SetText(@"using System;
using System.IO;

namespace TestProj
{
    class Program
    {
        static void Main(string[] args)
        {
            var value = args[0];
            var path = System.Reflection.Assembly.GetEntryAssembly().Location;
            var tfm = Path.GetFileName(Path.GetDirectoryName(path));
            Console.WriteLine(path);
        }
    }
}");
            VisualStudio.SolutionExplorer.SaveFile(Project, fileName);

            var launchSettingsContent = @"{
  ""profiles"": {
    ""Profile1"": {
      ""commandName"": ""Project"",
      ""commandLineArgs"": ""ProfileOne""
    },
    ""Profile2"": {
      ""commandName"": ""Project"",
      ""commandLineArgs"": ""ProfileTwo""
    }
  }
}";
            VisualStudio.SolutionExplorer.AddFile(Project, "Properties\\launchSettings.json", launchSettingsContent);
            VisualStudio.Workspace.WaitForAsyncOperations(FeatureAttribute.Workspace);

            VisualStudio.Debugger.SetBreakPoint(fileName, "Console.WriteLine(path);");
            VisualStudio.Debugger.Go(waitForBreakMode: true);

            VisualStudio.LocalsWindow.Verify.CheckEntry("value", "string", "\"ProfileOne\"");
            VisualStudio.LocalsWindow.Verify.CheckEntry("tfm", "string", "\"netcoreapp1.1\"");
            VisualStudio.Debugger.Go(waitForBreakMode: true);

            // Change the profile and the target framework
            var csprojectUserContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <ActiveDebugFramework>net461</ActiveDebugFramework>
    <ActiveDebugProfile>Profile2</ActiveDebugProfile>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Configuration)|$(TargetFramework)|$(Platform)'=='Debug|netcoreapp1.1|AnyCPU'"">
    <DebuggerFlavor>ProjectDebugger</DebuggerFlavor>
  </PropertyGroup>
</Project>";
            VisualStudio.SolutionExplorer.AddFile(Project, "TestProj.csproj.user", csprojectUserContent);
            VisualStudio.Workspace.WaitForAsyncOperations(FeatureAttribute.Workspace);

            // Unload and Reload the project to detect the .user file
            VisualStudio.SolutionExplorer.UnloadProject(Project);
            VisualStudio.SolutionExplorer.ReloadProject(Project);
            VisualStudio.Workspace.WaitForAsyncOperations(FeatureAttribute.SolutionCrawler);

            // Start
            VisualStudio.Debugger.Go(waitForBreakMode: true);

            VisualStudio.LocalsWindow.Verify.CheckEntry("value", "string", "\"ProfileTwo\"");
            VisualStudio.LocalsWindow.Verify.CheckEntry("tfm", "string", "\"net461\"");
            VisualStudio.Debugger.Go(waitForBreakMode: true);

            //var availableCommands = VisualStudio.GetAvailableCommands();
        }
    }
}
