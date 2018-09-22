using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface to an Applicant Service
    /// </summary>
    public interface IApplicantService
    {
        /// <summary>
        /// Get an Applicant by id
        /// </summary>
        /// <param name="applicantId">Applicant's Colleague PERSON id</param>
        /// <returns>An Applicant DTO</returns>
        Task<Applicant> GetApplicantAsync(string applicantId);
    }
}
