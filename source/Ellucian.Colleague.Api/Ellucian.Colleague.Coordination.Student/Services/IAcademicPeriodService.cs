// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IAcademicPeriodService : IBaseService
    {
        /// <remarks>FOR USE WITH ELLUCIAN HEDM 4 </remarks>
        /// <summary>
        /// Gets all academic periods
        /// </summary>
        /// <returns>Collection of Academic Period DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriod2>> GetAcademicPeriods2Async(bool bypassCache);

        /// <remarks>FOR USE WITH ELLUCIAN HEDM 8 </remarks>
        /// <summary>
        /// Gets all academic periods
        /// </summary>
        /// <param name="registration"></param>
        /// <returns>Collection of Academic Period DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriod3>> GetAcademicPeriods3Async(bool bypassCache, string registration = "");

        /// <remarks>FOR USE WITH ELLUCIAN HEDM 4</remarks>
        /// <summary>
        /// Get an academic period from its GUID
        /// </summary>
        /// <returns>Academic Period DTO object</returns>
        Task<Ellucian.Colleague.Dtos.AcademicPeriod2> GetAcademicPeriodByGuid2Async(string guid);

        /// <remarks>FOR USE WITH ELLUCIAN HEDM 8</remarks>
        /// <summary>
        /// Get an academic period from its GUID
        /// including census dates and registration status.
        /// </summary>
        /// <returns>Academic Period DTO object</returns>
        Task<Ellucian.Colleague.Dtos.AcademicPeriod3> GetAcademicPeriodByGuid3Async(string guid, bool bypassCache = false);


        /// <remarks>FOR USE WITH ELLUCIAN HEDM 8</remarks>
        /// <summary>
        /// Get an academic period from its GUID
        /// including census dates and registration status.
        /// </summary>
        /// <returns>Academic Period DTO object</returns>
        Task<Ellucian.Colleague.Dtos.AcademicPeriod4> GetAcademicPeriodByGuid4Async(string guid, bool bypassCache = false);

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all Academic Periods
        /// including census dates and registration status
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="registration">Registration status (open, closed)</param>
        /// <param name="termCode">Specific term filter</param>
        /// <param name="category">Specific category (term, subterm, year)</param>
        /// <returns>Collection of AcademicPeriod DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriod4>> GetAcademicPeriods4Async(bool bypassCache, string registration = "", string termCode = "", 
            string category = "", DateTimeOffset? startOn = null, DateTimeOffset? endOn = null, Dictionary<string, string> filterQualifiers = null);
    }
}