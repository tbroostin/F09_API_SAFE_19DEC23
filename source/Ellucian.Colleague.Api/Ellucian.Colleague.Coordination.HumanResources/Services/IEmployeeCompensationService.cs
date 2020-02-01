/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IEmployeeCompensationService: IBaseService
    {
         Task<EmployeeCompensation> GetEmployeeCompensationAsync(string effectivePersonId, decimal? salaryAmount);
    }
}
