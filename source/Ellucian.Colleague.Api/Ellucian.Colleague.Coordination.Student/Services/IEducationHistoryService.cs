// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IEducationHistoryService : IBaseService
    {
         Task<IEnumerable<Ellucian.Colleague.Dtos.Student.EducationHistory>> QueryEducationHistoryByIdsAsync(IEnumerable<string> studentIds);
    }
}
