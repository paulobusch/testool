﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TesTool.Core.Models.Metadata;

namespace TesTool.Core.Interfaces.Services
{
    public interface IWebApiScanInfraService
    {
        string GetNamespace();
        Task<TypeWrapper> GetModelAsync(string name);
        Task<IEnumerable<Controller>> GetControllersAsync();
    }
}
