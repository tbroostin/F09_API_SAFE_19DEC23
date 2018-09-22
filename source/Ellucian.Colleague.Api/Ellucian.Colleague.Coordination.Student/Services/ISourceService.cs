// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Source services
    /// </summary>
    public interface ISourceService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.Source>> GetSourcesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.Source> GetSourceByIdAsync(string id);
    }
}
