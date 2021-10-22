﻿using System.Threading.Tasks;
using TesTool.Core.Models.Metadata;
using TesTool.Core.Models.Templates.Faker;

namespace TesTool.Core.Interfaces.Services.Fakers
{
    public interface IFakeEntityService
    {
        string GetNamespace();
        Task<EntityFaker> GetFakerEntityAsync(Class model, bool @static);
    }
}
