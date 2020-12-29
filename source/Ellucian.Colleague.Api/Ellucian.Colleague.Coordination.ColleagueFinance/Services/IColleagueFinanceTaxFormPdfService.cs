// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    public interface IColleagueFinanceTaxFormPdfService
    {
        /// <summary>
        /// Returns the pdf data to print a T4A tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">The record ID where the T4A pdf data is stored</param>
        /// <returns>TaxFormT4APdfData domain entity</returns>
        Task<FormT4aPdfData> GetFormT4aPdfDataAsync(string personId, string recordId);

        /// <summary>
        /// Populates the T4A PDF with the supplied data using an RDLC template.
        /// </summary>
        /// <param name="pdfData">T4A PDF data</param>
        /// <param name="documentPath">Path to the RDLC template</param>
        /// <returns>Byte array containing PDF data for the T4A tax form</returns>
        byte[] PopulateT4aPdf(FormT4aPdfData pdfData, string documentPath);

        /// <summary>
        /// Retrieves a Form1099MIPdfData DTO.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-MISC.</param>
        /// <param name="recordId">The record ID where the 1099-MISC pdf data is stored</param>
        /// <returns>Form1099MIPdfData domain entity</returns>
        Task<Form1099MIPdfData> Get1099MiscPdfDataAsync(string personId, string recordId); 

        /// <summary>
        /// Populates the 1099-MISC PDF with the supplied data.
        /// </summary>
        /// <param name="pdfData">1099-MISC PDF data</param>
        /// <param name="documentPath">Path to the PDF template</param>
        /// <returns>Byte array containing PDF data for the 1099-MISC tax form</returns>
        byte[] Populate1099MiscPdf(Form1099MIPdfData pdfData, string documentPath);

        /// <summary>
        /// Retrieves a Form1099NecPdfData DTO.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-NEC.</param>
        /// <param name="recordId">The record ID where the 1099-NEC pdf data is stored</param>
        /// <returns>Form1099NecPdfData domain entity</returns>
        Task<Form1099NecPdfData> Get1099NecPdfDataAsync(string personId, string recordId);

        /// <summary>
        /// Populates the 1099-NEC PDF with the supplied data.
        /// </summary>
        /// <param name="pdfData">1099-NEC PDF data</param>
        /// <param name="documentPath">Path to the PDF template</param>
        /// <returns>Byte array containing PDF data for the 1099-NEC tax form</returns>
        byte[] Populate1099NecPdf(Form1099NecPdfData pdfData, string documentPath);
    }
}