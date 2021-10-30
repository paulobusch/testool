﻿using TesTool.Core.Models.Metadata;
using TesTool.Core.Models.Templates.Common;

namespace TesTool.Core.Interfaces.Services
{
    public interface IFixtureService
    {
        string GetFixtureFullPath();
        Fixture GetFixtureModel(Class dbContextClass);
    }
}
