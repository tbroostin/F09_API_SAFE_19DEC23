  
  
 
 
 
 
 
/* ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the CreateIServiceTemplate T4 Generator - Version 1.0
//     Last generated on  03/26/2019 09:15:46
//
//      Schema Version: ema
//      Schema Name:  prospect-opportunities
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//    
// </auto-generated>     
//----------------------------------------------------------------------------- */

//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for ProspectOpportunities services
    /// </summary>
    public interface IProspectOpportunitiesService : IBaseService
    {
                /// <summary>
        /// Gets all prospect-opportunities
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="ProspectOpportunities">prospectOpportunities</see> objects</returns>          
         Task<Tuple<IEnumerable<Dtos.ProspectOpportunities>, int>> GetProspectOpportunitiesAsync(int offset, int limit, Dtos.ProspectOpportunities criteria, 
             string personFilter, bool bypassCache = false);
             
        /// <summary>
        /// Get a prospectOpportunities by guid.
        /// </summary>
        /// <param name="guid">Guid of the prospectOpportunities in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="ProspectOpportunities">prospectOpportunities</see></returns>
        Task<Dtos.ProspectOpportunities> GetProspectOpportunitiesByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Update a ProspectOpportunitiesSubmissions.
        /// </summary>
        /// <param name="ProspectOpportunitiesSubmissions">The <see cref="ProspectOpportunitiesSubmissions">prospectOpportunitiesSubmissions</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="ProspectOpportunities">ProspectOpportunities</see></returns>
        Task<Dtos.ProspectOpportunities> UpdateProspectOpportunitiesSubmissionsAsync(Dtos.ProspectOpportunitiesSubmissions prospectOpportunitiesSubmissions, bool bypassCache);

        /// <summary>
        /// Create a ProspectOpportunitiesSubmissions.
        /// </summary>
        /// <param name="prospectOpportunitiesSubmissions">The <see cref="ProspectOpportunitiesSubmissions">prospectOpportunitiesSubmissions</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="ProspectOpportunities">ProspectOpportunities</see></returns>
        Task<Dtos.ProspectOpportunities> CreateProspectOpportunitiesSubmissionsAsync(Dtos.ProspectOpportunitiesSubmissions prospectOpportunitiesSubmissions, bool bypassCache);

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ProspectOpportunities from its GUID
        /// </summary>
        /// <returns>ProspectOpportunities DTO object</returns>
        Task<Dtos.ProspectOpportunitiesSubmissions> GetProspectOpportunitiesSubmissionsByGuidAsync(string guid, bool bypassCache = true);
    }
}