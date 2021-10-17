﻿using System.IO;
using System.Threading.Tasks;
using TesTool.Core.Attributes;
using TesTool.Core.Enumerations;
using TesTool.Core.Exceptions;
using TesTool.Core.Interfaces.Services;
using TesTool.Core.Interfaces.Services.Factories;
using TesTool.Core.Models.Metadata;
using TesTool.Core.Models.Templates.Comparator;
using TesTool.Core.Models.Templates.Factory;

namespace TesTool.Core.Commands.Generate
{
    [Command("compare", Order = 4, HelpText = "Gerar código de comparação entre objetos.")]
    public class GenerateCompareCommand : GenerateCommandBase
    {
        [Parameter(HelpText = "Nome da classe de origem.")]
        public string SourceClassName { get; set; }

        [Parameter(HelpText = "Nome da classe de destino.")]
        public string TargetClassName { get; set; }

        [Flag(HelpText = "Habilita modo estático de geração de código.")]
        public bool Static { get; set; }

        private readonly ICompareService _compareService;
        private readonly ISolutionService _solutionService;
        private readonly IFactoryCompareService _factoryCompareService;
        private readonly ITestScanInfraService _testScanInfraService;
        private readonly ITestCodeInfraService _testCodeInfraService;
        private readonly ITemplateCodeInfraService _templateCodeInfraService;
        private readonly IWebApiScanInfraService _webApiScanInfraService;

        public GenerateCompareCommand(
            ICompareService compareService,
            ISolutionService solutionService,
            IFactoryCompareService factoryCompareService,
            ITestScanInfraService testScanInfraService,
            ITestCodeInfraService testCodeInfraService,
            ITemplateCodeInfraService templateCodeInfraService,
            IWebApiScanInfraService webApiScanInfraService,
            IEnvironmentInfraService environmentInfraService,
            IFileSystemInfraService fileSystemInfraService
        ) : base(environmentInfraService, fileSystemInfraService)
        {
            _compareService = compareService;
            _solutionService = solutionService;
            _factoryCompareService = factoryCompareService;
            _testScanInfraService = testScanInfraService;
            _testCodeInfraService = testCodeInfraService;
            _templateCodeInfraService = templateCodeInfraService;
            _webApiScanInfraService = webApiScanInfraService;
        }

        protected override async Task GenerateAsync()
        {
            if (!await _webApiScanInfraService.ProjectExistAsync())
                throw new ProjectNotFoundException(ProjectTypeEnumerator.WEB_API);
            if (!await _testScanInfraService.ProjectExistAsync())
                throw new ProjectNotFoundException(ProjectTypeEnumerator.INTEGRATION_TESTS);

            var factoryName = await _factoryCompareService.GetFactoryNameAsync();
            if (!await _testScanInfraService.ClassExistAsync(factoryName))
                throw new ClassNotFoundException(factoryName);

            var comparatorClass = await _compareService.GetComparatorClassAsync(SourceClassName, TargetClassName);
            if (comparatorClass is not null) throw new DuplicatedClassException(comparatorClass.Name);

            Class sourceModel = await _webApiScanInfraService.GetModelAsync(SourceClassName) as Class;
            Class targetModel = await _webApiScanInfraService.GetModelAsync(TargetClassName) as Class;
            if (targetModel is null) throw new ClassNotFoundException(SourceClassName);
            if (sourceModel is null) throw new ClassNotFoundException(TargetClassName);

            var comparatorName = GetComparatorName();
            var filePath = GetComparatorFilePath();
            if (await _fileSystemInfraService.FileExistAsync(filePath))
                throw new DuplicatedSourceFileException(comparatorName);

            string sourceCode;
            if (Static)
            {
                var templateModel = await _compareService.GetCompareStaticAsync(sourceModel, targetModel);
                sourceCode = _templateCodeInfraService.BuildCompareStatic(templateModel);
            } else
            {
                var templateModel = await _compareService.GetCompareDynamicAsync(sourceModel, targetModel);
                await CreateAssertExtensionAsync(templateModel);
                sourceCode = _templateCodeInfraService.BuildCompareDynamic(templateModel);
            }
            
            await _fileSystemInfraService.SaveFileAsync(filePath, sourceCode);
            await AppendComparatorFactoryMethodAsync();
        }

        public async Task AppendComparatorFactoryMethodAsync() {
            var templateModel = _factoryCompareService.GetModelFactory(await _factoryCompareService.GetFactoryNameAsync());
            templateModel.AddNamespace(_compareService.GetNamespace());
            templateModel.AddMethod(new ComparatorFactoryMethod(GetComparatorName()));

            var factoryPathFile = await _testScanInfraService.GetPathClassAsync(templateModel.Name);
            var factorySourceCode = _templateCodeInfraService.BuildComparatorFactory(templateModel);
            var mergedSourceCode = await _testCodeInfraService.MergeClassCodeAsync(templateModel.Name, factorySourceCode);
            await _fileSystemInfraService.SaveFileAsync(factoryPathFile, mergedSourceCode);
        }

        private async Task CreateAssertExtensionAsync(CompareDynamic templateModel)
        {
            var extensionClassName = "AssertExtensions";
            var extensionClass = await _testScanInfraService.GetClassAsync(extensionClassName);
            if (extensionClass is null)
            {
                var extensionNamespace = _solutionService.GetTestNamespace("Extensions");
                var extensionFilePath = Path.Combine(GetOutputDirectory(), $"{extensionClassName}.cs");
                var extensionSourceCode = _templateCodeInfraService.BuildAssertExtensions(extensionNamespace);
                if (await _fileSystemInfraService.FileExistAsync(extensionFilePath))
                    throw new DuplicatedSourceFileException(extensionClassName);

                await _fileSystemInfraService.SaveFileAsync(extensionFilePath, extensionSourceCode);
                templateModel.AddNamespace(extensionNamespace);
                return;
            }

            templateModel.AddNamespace(extensionClass.Namespace);
        }

        private string GetComparatorName()
        {
            return _compareService.GetComparatorName(SourceClassName, TargetClassName);
        }

        protected string GetComparatorFilePath()
        {
            var fileName = $"{GetComparatorName()}.cs";
            return Path.Combine(GetOutputDirectory(), fileName);
        }
    }
}
