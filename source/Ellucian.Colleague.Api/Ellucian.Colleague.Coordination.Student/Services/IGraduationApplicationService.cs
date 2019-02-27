// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IGraduationApplicationService
    {
        /// <summary>
        /// Retrieves a graduation application for the given student Id and program code asynchronously.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">program code that student belongs to</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">Graduation Application</see> object that was created</returns>
        Task<Ellucian.Colleague.Dtos.Student.GraduationApplication> GetGraduationApplicationAsync(string studentId, string programCode);

        /// <summary>
        /// Validates a new graduation application and creates a new application in the database, returning the created application asynchronously.
        /// </summary>
        /// <param name="graduateApplication">New graduate application object to add</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">Graduation Application</see> object that was created</returns>
        Task<Ellucian.Colleague.Dtos.Student.GraduationApplication> CreateGraduationApplicationAsync(Dtos.Student.GraduationApplication graduationApplication);

        /// <summary>
        /// Retrieve list of all  the graduation applications for the given student Id asynchronously.
        /// </summary>
        /// <param name="studentId">Id of the student</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">List of Graduation Application</see></returns>
        Task<PrivacyWrapper<IEnumerable<Dtos.Student.GraduationApplication>>> GetGraduationApplicationsAsync(string studentId);

        /// <summary>
        /// Validates an existing graduation application and updates it in the database, returning the updated application asynchronously.
        /// </summary>
        /// <param name="graduateApplication">Updated graduate application object</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplication">Graduation Application</see> object that was updated</returns>
        Task<Ellucian.Colleague.Dtos.Student.GraduationApplication> UpdateGraduationApplicationAsync(Dtos.Student.GraduationApplication graduationApplication);

        /// <summary>
        /// Retrieves graduation application fee information for the given student Id and program code asynchronously. 
        /// </summary>
        /// <remarks>Users may only request graduation application fee information for themselves.</remarks>
        /// <param name="studentId">Id of the student</param>
        /// <param name="programCode">program code that student for which student is applying for graduation</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.GraduationApplicationFee">Graduation Application</see> object that was created</returns>
        Task<Ellucian.Colleague.Dtos.Student.GraduationApplicationFee> GetGraduationApplicationFeeAsync(string studentId, string programCode);

        /// <summary>
        /// Determines a student's eligibility to apply for graduation in the requested programs
        /// </summary>
        /// <param name="studentId">Id of student to determine eligibility</param>
        /// <param name="programCodes">Programs for which the eligibility is requested</param>
        /// <returns>List of Graduation Application Program Eligibility DTOs.</returns>
        Task<IEnumerable<Dtos.Student.GraduationApplicationProgramEligibility>> GetGraduationApplicationEligibilityAsync(string studentId, IEnumerable<string> programCodes);
    }
}
