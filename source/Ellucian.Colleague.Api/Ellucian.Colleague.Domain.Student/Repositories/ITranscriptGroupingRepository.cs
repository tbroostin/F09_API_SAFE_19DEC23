// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface ITranscriptGroupingRepository
    {
        /// <summary>
        /// Return the set of transcript groupings that are valid for users to select
        /// </summary>
        /// <returns>A set of transcript groupings</returns>
        Task<IEnumerable<TranscriptGrouping>> GetAsync();
    }
}
