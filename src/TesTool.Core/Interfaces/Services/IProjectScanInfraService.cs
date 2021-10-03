﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TesTool.Core.Models.Metadata;

namespace TesTool.Core.Interfaces.Services
{
    public interface IProjectScanInfraService
    {
        Task<IEnumerable<Controller>> GetControllersAsync();
    }
}
