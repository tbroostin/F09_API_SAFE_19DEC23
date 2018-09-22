// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestHumanResourcesTaxFormPdfDataRepository
    {
        public List<FormT4PdfData> Form1098PdfDataObjects;

        public TestHumanResourcesTaxFormPdfDataRepository()
        {
            this.Form1098PdfDataObjects = new List<FormT4PdfData>()
            {
                new FormT4PdfData() { TaxYear = "2016" },
                new FormT4PdfData() { TaxYear = "2015" },
                new FormT4PdfData() { TaxYear = "2014" },
                new FormT4PdfData() { TaxYear = "2013" },
                new FormT4PdfData() { TaxYear = "2012" },
                new FormT4PdfData() { TaxYear = "2011" },
                new FormT4PdfData() { TaxYear = "2010" },
            };
        }

        public FormT4PdfData GetT4PdfAsync(string personId, string recordId)
        {
            return this.Form1098PdfDataObjects.Where(x => x.TaxYear == recordId).FirstOrDefault();
        }
    }
}
