// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Base;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Planning.Repositories
{
    public interface ISampleDegreePlanRepository
    {
        Task<IEnumerable<CurriculumTrack>> GetProgramCatalogCurriculumTracksAsync(string programCode, string catalogCode);
        Task<SampleDegreePlan> GetAsync(string curriculumTrackCode);
    }
}
