// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentTaxFormPdfService
    {
        /// <summary>
        /// Returns the pdf data to print a 1098 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098.</param>
        /// <param name="recordId">The record ID where the 1098 pdf data is stored</param>
        /// <returns>Form1098PdfData domain entity</returns>
        Task<Form1098PdfData> Get1098TaxFormData(string personId, string recordId);

        /// <summary>
        /// Populates the 1098 PDF with the supplied data using an RDLC template.
        /// </summary>
        /// <param name="pdfData">1098 PDF data</param>
        /// <param name="documentPath">Path to the RDLC template</param>
        /// <returns>Byte array containing PDF data for the 1098 tax form</returns>
        byte[] Populate1098Report(Form1098PdfData pdfData, string documentPath);

        /// <summary>
        /// Returns the pdf data to print a T2202A tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202A.</param>
        /// <param name="recordId">The record ID where the T2202A pdf data is stored</param>
        /// <returns>FormT2202aPdfData domain entity</returns>
        Task<FormT2202aPdfData> GetT2202aTaxFormData(string personId, string recordId);

        /// <summary>
        /// Populates the T2202A PDF with the supplied data using an RDLC template.
        /// </summary>
        /// <param name="pdfData">T2202A PDF data</param>
        /// <param name="documentPath">Path to the RDLC template</param>
        /// <returns>Byte array containing PDF data for the T2202A tax form</returns>
        byte[] PopulateT2202aReport(FormT2202aPdfData pdfData, string documentPath);
    }
}
