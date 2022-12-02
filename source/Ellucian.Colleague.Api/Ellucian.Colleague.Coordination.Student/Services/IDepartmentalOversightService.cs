// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Service interface for all things related to Departmental Oversight
    /// </summary>
    public interface IDepartmentalOversightService: IBaseService
    {
        Task<IEnumerable<Dtos.Student.DeptOversightSearchResult>> SearchAsync( DeptOversightSearchCriteria criteria, int pageSize = int.MaxValue, int pageIndex = 1);

        /// <summary>
        /// Gets the departmental oversight permissions asynchronous.
        /// </summary>
        /// <returns><see cref="DepartmentalOversightPermissions">Departmental Oversight permissions</see> object containing permission information</returns>
        Task<DepartmentalOversightPermissions> GetDepartmentalOversightPermissionsAsync();

        /// <summary>
        /// Gets the departmental oversight and faculty details
        /// </summary>
        /// <returns><see cref="DepartmentalOversight">Departmental Oversight</see> object containing details information</returns>
        Task<IEnumerable<DepartmentalOversight>> QueryDepartmentalOversightAsync(IEnumerable<string> Ids, string SectionId, bool useCache = true);
    }
}
