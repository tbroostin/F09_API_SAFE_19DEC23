// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IGradeService : IBaseService
    {
        /// <summary>
        /// Gets all grades
        /// </summary>
        /// <returns>Collection of grades DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Grade>> GetAsync(bool bypassCache);
        /// <summary>
        /// Get grade by Id
        /// </summary>
        /// <returns>Collection of grades DTO objects</returns>
        Task<Dtos.Grade> GetGradeByIdAsync(string id);
        /// <summary>
        /// Gets all grades-definitions-maximum
        /// </summary>
        /// <returns>Collection of grades-definitions-maximum DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.GradeDefinitionsMaximum>> GetGradesDefinitionsMaximumAsync(bool bypassCache);
        /// <summary>
        /// Get grades-definitions-maximum by Id
        /// </summary>
        /// <returns>Collection of grades-definitions-maximum DTO objects</returns>
        Task<Dtos.GradeDefinitionsMaximum> GetGradesDefinitionsMaximumIdAsync(string id);
        /// <summary>
        /// Gets grades for a list of students, optionally filtered by term.
        /// </summary>
        /// <param name="studentIds">List of student ids.</param>
        /// <param name="term">Optional term for filtering results.</param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.PilotGrade>> GetPilotGradesAsync(IEnumerable<string> studentIds, string term);   
        /// <summary>
        /// Gets grades for a list of students, optionally filtered by term.
        /// </summary>
        /// <param name="studentIds">List of student ids.</param>
        /// <param name="term">Optional term for filtering results.</param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.PilotGrade>> GetPilotGrades2Async(IEnumerable<string> studentIds, string term);
    }
}
