using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    public interface IGetF09StuTrackingSheetService
    {
        Task<GetF09StuTrackingSheetResponseDto> GetF09StuTrackingSheetAsync(string Id);

        // F09 added here on 05-20-2019
        Task<PdfTrackingSheetResponseDto> GetPdfStudentTrackingSheetAsync(string personId);

        /// <summary>
        /// Get a student's tracking sheet as a byte array representation of a PDF file.  
        /// </summary>
        /// <param name="responseDto">Response DTO to use as the data source for producing the student tracking sheet report.</param>
        /// <param name="pathToReport">The path on the server to the report template</param>
        /// <param name="pathToResourceFile">The path on the server to the resource file</param>
        /// <param name="pathToLogo">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF student tracking sheet report.</returns>
        byte[] GetStudentTrackingSheetReport(PdfTrackingSheetResponseDto responseDto, string pathToReport, string pathToResourceFile, string pathToLogo);
    }
}


