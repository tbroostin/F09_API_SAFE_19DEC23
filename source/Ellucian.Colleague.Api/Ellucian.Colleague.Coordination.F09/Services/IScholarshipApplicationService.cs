using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IScholarshipApplicationService
    {
        Task<ScholarshipApplicationResponseDto> GetScholarshipApplicationAsync(string personId);
        Task<ScholarshipApplicationResponseDto> UpdateScholarshipApplicationAsync(ScholarshipApplicationRequestDto request);

        /// <summary>
        /// Get a student's accounts receivable statement as a byte array representation of a PDF file.  
        /// </summary>
        /// <param name="statementDto">StudentStatement DTO to use as the data source for producing the student statement report.</param>
        /// <param name="pathToReport">The path on the server to the report template</param>
        /// <param name="pathToResourceFile">The path on the server to the resource file</param>
        /// <param name="pathToLogo">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF student statement report.</returns>
        byte[] GetStudentStatementReport(ScholarshipApplicationStudentStatementDto statementDto, string pathToReport, string pathToResourceFile, string pathToLogo);
    }
}