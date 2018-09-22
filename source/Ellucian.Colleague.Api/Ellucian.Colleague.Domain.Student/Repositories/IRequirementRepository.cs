// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Base;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IRequirementRepository
    {
        Task<Requirement> GetAsync(string requirementCode);
        Task<IEnumerable<Requirement>> GetAsync(IEnumerable<string> requirementCodes, ProgramRequirements pr);
        Task<IEnumerable<Requirement>> GetAsync(IEnumerable<string> requirementCodes);
        Task<DegreeAuditParameters> GetDegreeAuditParametersAsync();
    }
}
