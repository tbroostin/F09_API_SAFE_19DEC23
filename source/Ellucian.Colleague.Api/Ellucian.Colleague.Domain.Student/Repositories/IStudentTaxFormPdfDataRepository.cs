// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentTaxFormPdfDataRepository
    {
        /// <summary>
        /// Get the 1098-T/E data for a PDF.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1098-T.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a 1098-T tax form</param>
        /// <returns>1098-T/E data for a pdf</returns>
        Task<Form1098PdfData> Get1098PdfAsync(string personId, string recordId);

        /// <summary>
        /// Get the T2202A data for a PDF.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T2202A.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a T2202a tax form</param>
        /// <returns>T2202A data for a pdf</returns>
        Task<FormT2202aPdfData> GetT2202aPdfAsync(string personId, string recordId);
    }
}
