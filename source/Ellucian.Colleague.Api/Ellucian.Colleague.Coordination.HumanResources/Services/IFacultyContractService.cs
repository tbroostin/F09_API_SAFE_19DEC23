/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IFacultyContractService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.FacultyContract>> GetFacultyContractsByFacultyIdAsync(string facultyId);
    }
}
