/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPersonEmploymentStatusRepository
    {
        Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesAsync(IEnumerable<string> personIds);
    }
}
