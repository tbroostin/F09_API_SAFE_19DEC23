// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for a TranscriptGroupingService
    /// </summary>
    public interface ITranscriptGroupingService
    {
        /// <summary>
        /// Return the set of transcript groupings that are user-selectable
        /// </summary>
        /// <returns>A set of transcript grouping DTOs</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TranscriptGrouping>> GetSelectableTranscriptGroupingsAsync();
    }
}
