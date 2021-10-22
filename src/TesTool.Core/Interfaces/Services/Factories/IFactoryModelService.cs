﻿using System.Threading.Tasks;
using TesTool.Core.Models.Templates.Factories;

namespace TesTool.Core.Interfaces.Services.Factories
{
    public interface IFactoryModelService
    {
        string GetNamespace();
        Task<string> GetFactoryNameAsync();
        ModelFakerFactory GetModelFactory(string name);
    }
}