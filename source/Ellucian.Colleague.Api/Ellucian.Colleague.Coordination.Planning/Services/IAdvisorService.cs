// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Planning;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Planning.Services
{
    public interface IAdvisorService
    {
        Task<Advisor> GetAdvisorAsync(string id);
        Task<PrivacyWrapper<List<Advisee>>> GetAdviseesAsync(string id, int pageSize = int.MaxValue, int pageIndex = 1, bool activeAdviseesOnly = false);
        Task<IEnumerable<string>> SearchAsync(string searchString, int pageSize = int.MaxValue, int pageIndex = 1);
        Task<IEnumerable<Advisee>> Search2Async(string searchString, int pageSize = int.MaxValue, int pageIndex = 1);
        Task<PrivacyWrapper<List<Advisee>>> Search3Async(Dtos.Planning.AdviseeSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1);
        Task<PrivacyWrapper<List<Advisee>>> SearchForExactMatchAsync(Dtos.Planning.AdviseeSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1);
        [Obsolete("Obsolete as of Colleague Web API 1.21. Use GetAdvisorPermissions2Async")]
        Task<IEnumerable<string>> GetAdvisorPermissionsAsync();
        /// <summary>
        /// Returns the advising permissions for the authenticated user.
        /// </summary>
        /// <returns>Advising permissions for the authenticated user.</returns>
        Task<Dtos.Planning.AdvisingPermissions> GetAdvisingPermissions2Async();
        Task<PrivacyWrapper<Advisee>> GetAdviseeAsync(string advisorId, string adviseeId);
        [Obsolete("Obsolete as API 1.19, user QueryAdvisorsByPostAsync instead.")]
        Task<IEnumerable<Advisor>> GetAdvisorsAsync(AdvisorQueryCriteria advisorQueryCriteria);
        /// <summary>
        /// Retrieves basic advisor information for a the given Advisor query criteria (list of advisor ids). 
        /// Use for name identification only. Does not confirm authorization to perform as an advisor.
        /// This is intended to retrieve merely reference information (name, email) for any person who may have currently 
        /// or previously performed the functions of an advisor. If a specified ID not found to be a potential advisor,
        /// does not cause an exception, item is simply not returned in the list.
        /// </summary>
        /// <param name="advisorIds">Advisor IDs for whom data will be retrieved</param>
        /// <returns>A list of <see cref="Advisor">Advisors</see> object containing advisor name</returns>
        Task<IEnumerable<Dtos.Planning.Advisor>> QueryAdvisorsByPostAsync(IEnumerable<string> advisorIds);
        Task<PrivacyWrapper<Dtos.Planning.Advisee>> PostCompletedAdvisementAsync(string studentId, Dtos.Student.CompletedAdvisement completeAdvisement);
    }
}
