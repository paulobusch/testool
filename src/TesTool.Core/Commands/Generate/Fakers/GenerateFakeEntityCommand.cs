﻿using System.Threading.Tasks;
using TesTool.Core.Attributes;
using TesTool.Core.Enumerations;
using TesTool.Core.Exceptions;
using TesTool.Core.Interfaces.Services;
using TesTool.Core.Interfaces.Services.Factories;
using TesTool.Core.Interfaces.Services.Fakers;
using TesTool.Core.Models.Metadata;
using TesTool.Core.Models.Templates.Factories;

namespace TesTool.Core.Commands.Generate.Fakers
{
    [Command("entity", HelpText = "Gerar código de fabricação de entidade de banco de dados.")]
    public class GenerateFakeEntityCommand : GenerateFakeBase
    {
        private readonly IFakeEntityService _fakeEntityService;
        private readonly IFactoryEntityService _factoryEntityService;
        private readonly ISettingInfraService _settingInfraService;
        private readonly ITestCodeInfraService _testCodeInfraService;

        public GenerateFakeEntityCommand(
            IFakeEntityService fakeEntityService,
            IFactoryEntityService factoryEntityService,
            ISettingInfraService settingInfraService,
            ITestCodeInfraService testCodeInfraService,
            IFileSystemInfraService fileSystemInfraService, 
            IWebApiScanInfraService webApiScanInfraService, 
            ITestScanInfraService testScanInfraService, 
            ITemplateCodeInfraService templateCodeInfraService
        ) : base(
            fileSystemInfraService, webApiScanInfraService, 
            testScanInfraService, templateCodeInfraService
        )
        { 
            _fakeEntityService = fakeEntityService;
            _factoryEntityService = factoryEntityService;
            _settingInfraService = settingInfraService;
            _testCodeInfraService = testCodeInfraService;
        }

        protected override async Task GenerateAsync()
        {
            if (!await _testScanInfraService.ClassExistAsync(TestClassEnumerator.ENTITY_FAKER_BASE.Name))
                throw new ClassNotFoundException(TestClassEnumerator.ENTITY_FAKER_BASE.Name);

            await base.GenerateAsync();
        }

        protected override string GetFactoryName()
        {
            return _factoryEntityService.GetFactoryName();
        }

        protected override async Task<string> BuildSourceCodeAsync(Class @class, string fakerName)
        {
            var templateModel = await _fakeEntityService.GetFakerEntityAsync(@class, Static);
            return _templateCodeInfraService.BuildEntityFaker(templateModel);
        }

        protected override async Task AppendFactoryMethodAsync(Class @class, string fakerName, string factoryName)
        {
            var dbContext = await _settingInfraService.GetStringAsync(SettingEnumerator.DB_CONTEXT_NAME);
            var factoryTemplateEntity = await _factoryEntityService.GetEntityFactoryAsync(factoryName, dbContext);
            factoryTemplateEntity.AddNamespace(_fakeEntityService.GetNamespace());
            factoryTemplateEntity.AddMethod(new EntityFakerFactoryMethod(ClassName, fakerName));

            var factoryPathFile = await _testScanInfraService.GetPathClassAsync(factoryName);
            var factorySourceCode = _templateCodeInfraService.BuildEntityFakerFactory(factoryTemplateEntity);
            var mergedSourceCode = await _testCodeInfraService.MergeClassCodeAsync(factoryName, factorySourceCode);
            await _fileSystemInfraService.SaveFileAsync(factoryPathFile, mergedSourceCode);
        }

        protected override string GetFakerName(string className)
        {
            return _fakeEntityService.GetFakerName(className);
        }

        protected override string GetOutputDirectory() => string.IsNullOrWhiteSpace(Output)
            ? _fakeEntityService.GetDirectoryBase() : Output;
    }
}
