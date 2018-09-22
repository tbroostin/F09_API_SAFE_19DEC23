/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IFacultyContractRepository
    {
        /// <summary>
        /// Gets a list of Faculty Contracts for a given collection of employee IDs    
        /// </summary>
        /// <param name="facultyIds">Faculty Ids</param>
        /// <returns>A collection of FacultyContracts</returns>
        Task<IEnumerable<FacultyContract>> GetFacultyContractsByFacultyIdAsync(string facultyId);
    }
}
