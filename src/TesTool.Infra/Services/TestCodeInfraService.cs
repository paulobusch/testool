﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TesTool.Core.Enumerations;
using TesTool.Core.Exceptions;
using TesTool.Core.Interfaces.Services;

namespace TesTool.Infra.Services
{
    public class TestCodeInfraService : TestScanInfraService, ITestCodeInfraService
    {
        private readonly ICmdInfraService _cmdInfraService;
        private readonly IWebApiScanInfraService _webApiScanInfraService;
        private readonly ISolutionInfraService _solutionInfraService;

        public TestCodeInfraService(
            ICmdInfraService cmdInfraService,
            ILoggerInfraService loggerInfraService,
            ISettingInfraService settingInfraService,
            IWebApiScanInfraService webApiScanInfraService,
            ISolutionInfraService solutionInfraService,
            IEnvironmentInfraService environmentInfraService
        ) : base(loggerInfraService, settingInfraService, environmentInfraService) 
        {
            _cmdInfraService = cmdInfraService;
            _webApiScanInfraService = webApiScanInfraService;
            _solutionInfraService = solutionInfraService;
        }

        public async Task CreateTestProjectAsync(string name, string output)
        {
            var fullOutput = Path.Combine(output, name);
            var testProjectPathFile = @$"{fullOutput}\{name}.csproj";
            if (File.Exists(testProjectPathFile))
                throw new DuplicatedSourceFileException(Path.GetFileName(testProjectPathFile));

            var solutionPathFile = _solutionInfraService.GetSolutionFilePath();
            var webApiProjectPathFile = _webApiScanInfraService.GetProjectPathFile();
            var commands = new List<string> { 
                @$"dotnet new xunit --name ""{name}"" --output ""{fullOutput}"" --no-restore",
                @$"dotnet sln ""{solutionPathFile}"" add ""{fullOutput}""",
                @$"dotnet add ""{name}"" reference ""{webApiProjectPathFile}""",
                @$"dotnet add ""{fullOutput}"" package Microsoft.AspNetCore.TestHost",
                @$"dotnet add ""{fullOutput}"" package bogus",
                @$"dotnet restore ""{fullOutput}""",
                @$"del /f ""{fullOutput}\*.cs"""
            };
            await _cmdInfraService.ExecuteCommandsAsync(commands);
            await _settingInfraService.SetStringAsync(SettingEnumerator.PROJECT_INTEGRATION_TEST_DIRECTORY, testProjectPathFile);
        }

        public async Task<string> MergeClassCodeAsync(string className, string sourceCode)
        {
            var classes = await GetClassesAsync();
            var storedContext = classes.FirstOrDefault(c => c.Declaration.Identifier.Text == className);
            if (storedContext is null) return default;
            var storedCode = await File.ReadAllTextAsync(storedContext.FilePath);
            var compilationUnitStored = SyntaxFactory.ParseCompilationUnit(storedCode);
            var storedClass = compilationUnitStored.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(c => c.Identifier.Text == className);
            var storedUsings = compilationUnitStored.Usings;
            var storedMethods = storedClass.Members.OfType<MethodDeclarationSyntax>().ToList();

            var compilationUnitGenerated = SyntaxFactory.ParseCompilationUnit(sourceCode);
            var sourceClass = compilationUnitGenerated.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(c => c.Identifier.Text == className);
            var sourceUsings = compilationUnitGenerated.Usings;
            var sourceMethods = sourceClass.Members.OfType<MethodDeclarationSyntax>().ToList();

            var usingsToAppend = sourceUsings.Where(u => !storedUsings.Any(s => s.Name.ToString() == u.Name.ToString())).ToList();
            var methodsToAppend = sourceMethods.Where(s => !storedMethods.Any(m => m.Identifier.Text == s.Identifier.Text)).ToList();

            var updatedClass = null as ClassDeclarationSyntax;
            foreach (var method in methodsToAppend) updatedClass = (updatedClass ?? storedClass).AddMembers(method);
            if (updatedClass is not null) compilationUnitStored = compilationUnitStored.ReplaceNode(storedClass, updatedClass);

            foreach (var @using in usingsToAppend) compilationUnitStored = compilationUnitStored.AddUsings(@using);

            return compilationUnitStored.ToFullString();
        }
    }
}
