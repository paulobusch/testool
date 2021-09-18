﻿using System.Threading.Tasks;
using TesTool.Core.Attributes;

namespace TesTool.Core.Commands.Generate
{
    [Command("project", "p")]
    public class GenerateProjectCommand : GenerateCommandBase
    {
        [Parameter(IsDefault = true)]
        public string Directory { get; set; }

        [Parameter]
        public string Output { get; set; }

        [Flag]
        public string Static { get; set; }

        public override Task ExecuteAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
