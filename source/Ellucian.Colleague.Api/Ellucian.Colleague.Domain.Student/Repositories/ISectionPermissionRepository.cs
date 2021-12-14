// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// interface for section permission
    /// </summary>
    public interface ISectionPermissionRepository
    {
        /// <summary>
        /// get section permissions async
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        Task<SectionPermission> GetSectionPermissionAsync(string sectionId);
        /// <summary>
        /// Add a student petition
        /// </summary>
        /// <param name="studentPetition">The student petition to add</param>
        /// <returns>Newly created student petition</returns>
        Task<StudentPetition> AddStudentPetitionAsync(StudentPetition studentPetition);

        /// <summary>
        /// Update a student petition
        /// </summary>
        /// <param name="studentPetition">The student petition to update</param>
        /// <returns>Updated student petition</returns>
        Task<StudentPetition> UpdateStudentPetitionAsync(StudentPetition studentPetition);

        /// <summary>
        /// Get a student petition by Id
        /// </summary>
        /// <param name="studentPetitionId">The id of the student petition to retrieve</param>
        /// <param name="type">Indicates whether you want the faculty consent or the student petition for the specific Id.</param>
        /// <param name="sectionId">Id of the section for the student petition.  One Colleague StudentPetition record can have multiple sections</param>
        /// <returns>The student petition</returns>
        Task<StudentPetition> GetAsync(string studentPetitionId, string sectionId, StudentPetitionType type);
    }
}
