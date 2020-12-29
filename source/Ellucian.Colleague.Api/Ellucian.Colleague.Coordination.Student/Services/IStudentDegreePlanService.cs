// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.DegreePlans;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentDegreePlanService
    {
        Task<DegreePlan> GetDegreePlanAsync(int id);
        Task<DegreePlan2> GetDegreePlan2Async(int id);
        [Obsolete("Obsolete on version 1.6 of the Api. Use GetDegreePlan4 going forward.")]
        Task<DegreePlan3> GetDegreePlan3Async(int id);
        [Obsolete("Obsolete on version 1.11 of the Api. Use GetDegreePlan5Async going forward.")]
        Task<DegreePlanAcademicHistory> GetDegreePlan4Async(int id, bool validate = true);
        [Obsolete("Obsolete on version 1.18 of the Api. Use GetDegreePlan6Async going forward.")]
        Task<DegreePlanAcademicHistory2> GetDegreePlan5Async(int id, bool validate = true);
        Task<DegreePlanAcademicHistory3> GetDegreePlan6Async(int id, bool validate = true, bool includeDrops = false);

        Task<DegreePlan> CreateDegreePlanAsync(string personId);
        Task<DegreePlan2> CreateDegreePlan2Async(string personId);
        [Obsolete("Obsolete on version 1.6 of the Api. Use CreateDegreePlan4 going forward.")]
        Task<DegreePlan3> CreateDegreePlan3Async(string personId);
        [Obsolete("Obsolete on version 1.11 of the Api. Use CreateDegreePlan5Async going forward.")]
        Task<DegreePlanAcademicHistory> CreateDegreePlan4Async(string personId);
        [Obsolete("Obsolete on version 1.18 of the Api. Use CreateDegreePlan6Async going forward.")]
        Task<DegreePlanAcademicHistory2> CreateDegreePlan5Async(string personId);
        Task<DegreePlanAcademicHistory3> CreateDegreePlan6Async(string studentId);
        /// <summary>
        /// Accept an updated DegreePlan4 DTO and apply it to the Colleague database.
        /// </summary>
        /// <param name="degreePlan">Degree plan to update</param>
        /// <returns>The updated <see cref="DegreePlanAcademicHistory3">DegreePlan4</see> DTO - if successful, in a combined DTO with the AcademicHistory2 dto</returns>
        Task<DegreePlanAcademicHistory3> UpdateDegreePlan6Async(DegreePlan4 degreePlan);

        Task<DegreePlan> UpdateDegreePlanAsync(DegreePlan degreePlan);
        Task<DegreePlan2> UpdateDegreePlan2Async(DegreePlan2 degreePlan);
        [Obsolete("Obsolete on version 1.6 of the Api. Use UpdateDegreePlan4 going forward.")]
        Task<DegreePlan3> UpdateDegreePlan3Async(DegreePlan3 degreePlan);
        [Obsolete("Obsolete on version 1.11 of the Api. Use UpdateDegreePlan5Async going forward.")]
        Task<DegreePlanAcademicHistory> UpdateDegreePlan4Async(DegreePlan4 degreePlan);
        [Obsolete("Obsolete on version 1.18 of the Api. Use UpdateDegreePlan6Async going forward.")]
        Task<DegreePlanAcademicHistory2> UpdateDegreePlan5Async(DegreePlan4 degreePlan);

        Task<RegistrationResponse> RegisterAsync(int degreePlanId, string termId);
        Task<RegistrationResponse> RegisterSectionsAsync(int degreePlanId, IEnumerable<SectionRegistration> sectionRegistrations);

        Task CheckUpdatePermissionsAsync(Domain.Student.Entities.Student student, Domain.Student.Entities.DegreePlans.DegreePlan degreePlanToUpdate);
        Task CheckUpdatePermissions2Async(Domain.Student.Entities.DegreePlans.DegreePlan degreePlanToUpdate, Domain.Student.Entities.DegreePlans.DegreePlan storedDegreePlan, Domain.Student.Entities.PlanningStudent student);
    }
}
