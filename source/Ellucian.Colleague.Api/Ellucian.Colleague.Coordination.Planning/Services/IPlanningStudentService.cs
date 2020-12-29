// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Planning.Services
{
    public interface IPlanningStudentService
    {
        Task<PrivacyWrapper<IEnumerable<Dtos.Student.PlanningStudent>>> QueryPlanningStudentsAsync(IEnumerable<string> studentIds);
    }
}
