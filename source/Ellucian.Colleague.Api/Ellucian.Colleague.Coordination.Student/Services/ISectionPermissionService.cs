// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for section permission to retrieve petitions and consent
    /// </summary>
    public interface ISectionPermissionService
    {
        /// <summary>
        /// retrieves section petitions & consent asynchronously
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        Task<Dtos.Student.SectionPermission> GetAsync(string sectionId);
        /// <summary>
        /// Validates a new student petition and adds it in the database, returning the student petition id added/updated
        /// </summary>
        /// <param name="studentPetition">New student petition object to add</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentPetition">StudentPetition</see> object that was created</returns>
        Task<Ellucian.Colleague.Dtos.Student.StudentPetition> AddStudentPetitionAsync(Dtos.Student.StudentPetition studentPetition);

        /// <summary>
        /// Validates student petition and updates it in the database, returning the student petition id updated
        /// </summary>
        /// <param name="studentPetition">Student petition object to update</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentPetition">StudentPetition</see> object that was updated</returns>
        Task<Ellucian.Colleague.Dtos.Student.StudentPetition> UpdateStudentPetitionAsync(Dtos.Student.StudentPetition studentPetition);

        /// <summary>
        /// Gets a new student petition 
        /// </summary>
        /// <param name="type">Type of student petition to return</param>
        /// <param name="studentPetitionId">Id of requested StudentPetition</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentPetition">StudentPetition</see> object that was created</returns>
        Task<Ellucian.Colleague.Dtos.Student.StudentPetition> GetStudentPetitionAsync(string studentPetitionId, string sectionId, StudentPetitionType type);
    }
}
