// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface ICoursePlaceholderService
    {
        /// <summary>
        /// Retrieve a collection of course placeholders by ID
        /// </summary>
        /// <param name="coursePlaceholderIds">Unique identifiers for course placeholders to retrieve</param>
        /// <param name="bypassCache">Flag indicating whether or not to bypass the API's cached course placeholder data and retrieve the data directly from Colleague; defaults to false</param>
        /// <returns>Collection of <see cref="CoursePlaceholder"/></returns>
        Task<IEnumerable<CoursePlaceholder>> GetCoursePlaceholdersByIdsAsync(IEnumerable<string> coursePlaceholderIds, bool bypassCache = false);
    }
}
