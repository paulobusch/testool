﻿using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using TesTool.Core.Attributes;
using TesTool.Core.Interfaces;
using TesTool.Core.Interfaces.Services;

namespace TesTool.Core.Commands.Options
{
    [Command("--version", "-v", IsOption = true, HelpText = "Exiba a versão do TesTool em uso.")]
    public class VersionCommand : ICommand
    {
        private readonly ILoggerInfraService _loggerService;

        public VersionCommand(ILoggerInfraService loggerService)
        {
            _loggerService = loggerService;
        }

        public Task ExecuteAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            _loggerService.LogInformation(version);
            return Task.CompletedTask;
        }
    }
}
