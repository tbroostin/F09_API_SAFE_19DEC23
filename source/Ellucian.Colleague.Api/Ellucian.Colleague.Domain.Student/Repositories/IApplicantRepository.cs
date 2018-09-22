using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface to an Applicant Repository
    /// </summary>
    public interface IApplicantRepository
    {
        /// <summary>
        /// Get an Applicant by id
        /// </summary>
        /// <param name="applicantId">Applicant's Colleague PERSON id</param>
        /// <returns>An Applicant object</returns>
        Task<Applicant> GetApplicantAsync(string applicantId);
        Applicant GetApplicant(string applicantId);
    }
}
