// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service interface for all things related to sectioncrosslist
    /// </summary>
    public interface ISectionCrosslistService : IBaseService
    {
        /// <summary>
        /// Get the page of sectioncrosslist's that can be fitlered by section if desired 
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="section">The section GUID to filter SectionCrosslist list on</param>
        /// <returns>A Tuple containining a DataModel format List of SectionCrosslist DTO and the total count of records</returns>
        Task<Tuple<IEnumerable<SectionCrosslist>, int>> GetDataModelSectionCrosslistsPageAsync(int offset, int limit, string section = "");

        /// <summary>
        /// Get a sectioncrosslist using its GUID
        /// </summary>
        /// <param name="guid">The sectioncrosslist's GUID</param>
        /// <returns>A DataModel format SectionCrosslist DTO</returns>
        Task<SectionCrosslist> GetDataModelSectionCrosslistsByGuidAsync(string guid);

        /// <summary>
        /// Create a sectioncrosslist
        /// </summary>
        /// <param name="sectionCrosslist">The sectioncrosslist to create</param>
        /// <returns>A DataModel-format SectionCrosslist DTO</returns>
        Task<SectionCrosslist> CreateDataModelSectionCrosslistsAsync(SectionCrosslist sectionCrosslist);

        /// <summary>
        /// Update a sectioncrosslist 
        /// </summary>
        /// <param name="sectionCrosslist">The sectioncrosslist to update</param>
        /// <returns>A DataModel-format SectionCrosslist DTO</returns>
        Task<SectionCrosslist> UpdateDataModelSectionCrosslistsAsync(SectionCrosslist sectionCrosslist);

        /// <summary>
        /// Deletes a sectioncrosslist using its GUID
        /// </summary>
        /// <param name="guid">The sectioncrosslist's GUID</param>
        Task DeleteDataModelSectionCrosslistsByGuidAsync(string guid);
    }
}
