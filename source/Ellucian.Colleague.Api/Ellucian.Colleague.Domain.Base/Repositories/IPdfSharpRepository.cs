// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.IO;
using PdfSharp.Pdf;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// This interface specifies the methods to be implemented in order to use the PdfSharp utility.
    /// This interface exists so any code using PdfSharp can be unit tested via the Mock framework.
    /// </summary>
    public interface IPdfSharpRepository
    {
        /// <summary>
        /// Open a PDF document using the PdfSharp library.
        /// </summary>
        /// <param name="documentPath">Path to the PDF document.</param>
        /// <returns>PdfSharp PdfDocument</returns>
        PdfDocument OpenDocument(string documentPath);

        /// <summary>
        /// Populate the PDF document with the supplied data.
        /// </summary>
        /// <param name="document">PDF Document</param>
        /// <param name="taxFormData">Dictionary of information</param>
        void PopulatePdfDocument(ref PdfDocument document, Dictionary<string, string> taxFormData);

        /// <summary>
        /// Close the PDF document and convert it into a memory stream.
        /// </summary>
        /// <param name="document">PDF Document</param>
        /// <returns>Memory stream</returns>
        MemoryStream FinalizePdfDocument(PdfDocument document);
    }
}
