//Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for InstitutionJobs Service
    /// </summary>
    public interface IInstitutionJobsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs>, int>> GetInstitutionJobsAsync(int offset, int limit,
            string person = "", string employer = "", string position = "", string department = "", string startOn = "", 
            string endOn = "", string status = "", string classification = "", string preference = "", bool bypassCache = false);

        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs2>, int>> GetInstitutionJobs2Async(int offset, int limit,
           string person = "", string employer = "", string position = "", string department = "", string startOn = "",
           string endOn = "", string status = "", string classification = "", string preference = "", bool bypassCache = false);

        /// <summary>
        /// Get all institutionJobs
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="person"></param>
        /// <param name="employer"></param>
        /// <param name="position"></param>
        /// <param name="department"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="status"></param>
        /// <param name="classification"></param>
        /// <param name="preference"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.InstitutionJobs3>, int>> GetInstitutionJobs3Async(int offset, int limit,
          string person = "", string employer = "", string position = "", string department = "", string startOn = "",
          string endOn = "", string status = "", string classification = "", string preference = "", bool bypassCache = false);


        /// <summary>
        /// Get a institutionJobs by guid.
        /// </summary>
        /// <param name="guid">Guid of the institutionJobs in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="InstitutionJobs">institutionJobs</see></returns>
        Task<Ellucian.Colleague.Dtos.InstitutionJobs> GetInstitutionJobsByGuidAsync(string guid, bool bypassCache = false);

        /// <summary>
        /// Get a institutionJobs by guid.
        /// </summary>
        /// <param name="guid">Guid of the institutionJobs in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="InstitutionJobs2">institutionJobs</see></returns>
        Task<Ellucian.Colleague.Dtos.InstitutionJobs2> GetInstitutionJobsByGuid2Async(string guid, bool bypassCache = false);

        /// <summary>
        /// Get a institutionJobs by guid.
        /// </summary>
        /// <param name="guid">Guid of the institutionJobs in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="InstitutionJobs3">institutionJobs</see></returns>
        Task<Ellucian.Colleague.Dtos.InstitutionJobs3> GetInstitutionJobsByGuid3Async(string guid, bool bypassCache = false);


        /// <summary>
        /// Update an institutionJobs.
        /// </summary>
        /// <param name="institutionJobs">The <see cref="InstitutionJobs2">institutionJobs</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="InstitutionJobs">institutionJobs</see></returns>
        Task<Ellucian.Colleague.Dtos.InstitutionJobs3> PutInstitutionJobsAsync(Ellucian.Colleague.Dtos.InstitutionJobs3 institutionJobs);

        /// <summary>
        /// Create an institutionJobs.
        /// </summary>
        /// <param name="institutionJobs">The <see cref="InstitutionJobs">institutionJobs</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="InstitutionJobs2">institutionJobs</see></returns>
        Task<Ellucian.Colleague.Dtos.InstitutionJobs3> PostInstitutionJobsAsync(Ellucian.Colleague.Dtos.InstitutionJobs3 institutionJobs);

    }
}