// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentWaiverService
    {
        /// <summary>
        /// Gets the list of waivers that exist for the given section Id
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns>List of <see cref="Ellucian.Colleague.Dtos.Student.StudentWaiver">Waiver</see> dto objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentWaiver>> GetSectionStudentWaiversAsync(string sectionId);

        /// <summary>
        /// Validates a new waiver and creates waiver in the database, returning the created waiver object
        /// </summary>
        /// <param name="studentWaiver">New waiver object to add</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentWaiver">Waiver</see> object that was created</returns>
        Task<Ellucian.Colleague.Dtos.Student.StudentWaiver> CreateStudentWaiverAsync(Dtos.Student.StudentWaiver studentWaiver);

        /// <summary>
        /// Returns the requested waiver
        /// </summary>
        /// <param name="studentWaiverId">If of the waiver to retrieve</param>
        /// <returns>Waiver dto object</returns>
        Task<Ellucian.Colleague.Dtos.Student.StudentWaiver> GetStudentWaiverAsync(string studentWaiverId);

        /// <summary>
        /// Gets the list of waivers that exist for the given student Id
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns>List of <see cref="Ellucian.Colleague.Dtos.Student.StudentWaiver">Waiver</see> dto objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentWaiver>> GetStudentWaiversAsync(string studentId);
    }
}
