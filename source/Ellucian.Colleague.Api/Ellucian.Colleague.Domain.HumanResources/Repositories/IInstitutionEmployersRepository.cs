/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IInstitutionEmployersRepository
    {
        Task<InstitutionEmployers> GetInstitutionEmployerByGuidAsync(string guid);
        Task<IEnumerable<InstitutionEmployers>> GetInstitutionEmployersAsync();
    }
}