// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.IO;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using slf4net;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Implements the IPdfSharpRepository so PDF documents can be consumed via the PdfSharp library.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PdfSharpUtility : BaseColleagueRepository, IPdfSharpRepository
    {
        public PdfSharpUtility(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Open a PDF document using the PdfSharp library.
        /// </summary>
        /// <param name="documentPath">Path to the PDF document.</param>
        /// <returns>PdfSharp PdfDocument</returns>
        public PdfDocument OpenDocument(string documentPath)
        {
            return PdfReader.Open(documentPath, PdfDocumentOpenMode.Modify);
        }

        /// <summary>
        /// Populate the PDF document with the supplied data.
        /// </summary>
        /// <param name="document">PDF Document</param>
        /// <param name="taxFormData">Dictionary of information</param>
        public void PopulatePdfDocument(ref PdfDocument document, Dictionary<string, string> taxFormData)
        {
            // Loop through the dictionary of fields for the tax form
            foreach (var data in taxFormData)
            {
                if (document.AcroForm.Fields[data.Key] is PdfTextField)
                {
                    PdfTextField textField = (PdfTextField)document.AcroForm.Fields[data.Key];
                    textField.Value = new PdfString(data.Value);
                    textField.ReadOnly = true;
                }
            }
        }

        /// <summary>
        /// Close the PDF document and convert it into a memory stream.
        /// </summary>
        /// <param name="document">PDF Document</param>
        /// <returns>Memory stream</returns>
        public MemoryStream FinalizePdfDocument(PdfDocument document)
        {
            string needAppearanceKey = "/NeedAppearances";
            if (document.AcroForm.Elements.ContainsKey(needAppearanceKey))
                document.AcroForm.Elements[needAppearanceKey] = new PdfSharp.Pdf.PdfBoolean(true);
            else
                document.AcroForm.Elements.Add(needAppearanceKey, new PdfSharp.Pdf.PdfBoolean(true));

            var memoryStream = new MemoryStream();
            document.Save(memoryStream, false);
            document.Close();

            return memoryStream;
        }
    }
}
