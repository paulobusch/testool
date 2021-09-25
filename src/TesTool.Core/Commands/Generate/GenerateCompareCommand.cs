﻿using System.Threading.Tasks;
using TesTool.Core.Attributes;

namespace TesTool.Core.Commands.Generate
{
    [Command("--compare", "-c", HelpText = "Gera código de comparação entre objetos.")]
    public class GenerateCompareCommand : GenerateCommandBase
    {
        [Parameter(IsDefault = true, HelpText = "Nome da classe de origem.")]
        public string SourceClassName { get; set; }

        [Parameter(IsDefault = true, HelpText = "Nome da classe de destino.")]
        public string TargetClassName { get; set; }

        [Parameter(IsDefault = true, HelpText = "Nome da classe que terá o método de comparação.")]
        public string ComparatorName { get; set; }
        
        public override Task ExecuteAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
