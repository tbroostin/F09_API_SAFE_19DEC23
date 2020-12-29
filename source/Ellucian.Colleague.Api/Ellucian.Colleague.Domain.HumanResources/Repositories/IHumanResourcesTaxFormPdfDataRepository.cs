// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    /// <summary>
    ///  Define the method signatures for a TaxFormPdfDataRepository
    /// </summary>
    public interface IHumanResourcesTaxFormPdfDataRepository
    {

        /// <summary>
        /// Gets the boolean value that indicates if the client is set up to use the Guam version of the W2 form.
        /// </summary>
        /// <returns>Boolean value where true = Guam and false = USA</returns>
        Task<bool> GetW2GuamFlag();
        /// <summary>
        /// Get the W-2 data for a PDF.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a W-2 tax form</param>
        /// <returns>W-2 data for a pdf</returns>
        Task<FormW2PdfData> GetW2PdfAsync(string personId, string recordId);

        /// <summary>
        /// Get the 1095-C data for a PDF.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1095-C.</param>
        /// <param name="recordId">ID of the 1095-C record.</param>
        /// <returns>1095-C data for a pdf</returns>
        Task<Form1095cPdfData> Get1095cPdfAsync(string personId, string recordId);

        /// <summary>
        /// Get the T4 data for a PDF.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4.</param>
        /// <param name="recordId">ID of the T4 record/param>
        /// <returns>T4 data for a PDF</returns>
        Task<FormT4PdfData> GetT4PdfAsync(string personId, string recordId);

        /// <summary>
        /// Get the W-2c data for a PDF.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        Task<FormW2cPdfData> GetW2cPdfAsync(string personId, string recordId);
    }
}
