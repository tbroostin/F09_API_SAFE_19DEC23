// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for Course Placeholder Repository
    /// </summary>
    public interface ICoursePlaceholderRepository
    {
        Task<IEnumerable<CoursePlaceholder>> GetCoursePlaceholdersByIdsAsync(IEnumerable<string> coursePlaceholderIds, bool bypassCache = false);
    }
}
