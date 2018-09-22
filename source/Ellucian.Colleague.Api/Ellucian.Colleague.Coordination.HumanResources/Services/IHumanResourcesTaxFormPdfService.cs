﻿// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.HumanResources.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Define the methods signatures for a TaxFormPdfService.
    /// </summary>
    public interface IHumanResourcesTaxFormPdfService
    {
        /// <summary>
        /// Returns the pdf data to print a W-2 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the W-2.</param>
        /// <param name="recordId">The record ID where the W-2 pdf data is stored</param>
        /// <returns>TaxFormW2PdfData domain entity</returns>
        Task<FormW2PdfData> GetW2TaxFormData(string personId, string recordId);
        
        /// <summary>
        /// Populates the W-2 PDF with the supplied data.
        /// </summary>
        /// <param name="pdfData">W-2 PDF data</param>
        /// <param name="documentPath">Path to the PDF template</param>
        /// <returns>Byte array containing PDF data for the W-2 tax form</returns>
        byte[] PopulateW2Pdf(FormW2PdfData pdfData, string documentPath);

        /// <summary>
        /// Populates the W-2 PDF with the supplied data using RDLC instead of PDF Sharp.
        /// </summary>
        /// <param name="pdfData">W-2 PDF data</param>
        /// <param name="pathToReport">Path to the PDF template</param>
        /// <returns>Byte array containing PDF data for the W-2 tax form</returns>
        byte[] PopulateW2PdfReport(FormW2PdfData pdfData, string pathToReport);

        /// <summary>
        /// Returns the pdf data to print a T4 tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4.</param>
        /// <param name="recordId">The record ID where the T-4 pdf data is stored</param>
        /// <returns>FormT4PdfData domain entity</returns>
        Task<FormT4PdfData> GetT4TaxFormData(string personId, string recordId);

        /// <summary>
        /// Populates the T4 PDF with the supplied data using RDLC instead of PDF Sharp.
        /// </summary>
        /// <param name="pdfData">T4 PDF data</param>
        /// <param name="pathToReport">Path to the PDF template</param>
        /// <returns>Byte array containing PDF data for the T4 tax form</returns>
        byte[] PopulateT4PdfReport(FormT4PdfData pdfData, string pathToReport);

        /// <summary>
        /// Returns the pdf data to print a 1095-C tax form.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1095-C.</param>
        /// <param name="ids">The list of record IDs where the W-2 pdf data is stored</param>
        /// <returns>Form1095cPdfData domain entity</returns>
        Task<Form1095cPdfData> Get1095cTaxFormData(string personId, string recordId);

        /// <summary>
        /// Populates the 1095-C PDF with the supplied data using an RDLC report.
        /// </summary>
        /// <param name="pdfData">1095-C PDF data</param>
        /// <param name="documentPath">Path to the RDLC template</param>
        /// <returns>Byte array containing PDF data for the 1095-C tax form</returns>
        byte[] Populate1095tReport(Form1095cPdfData pdfData, string pathToReport);
    }
}
