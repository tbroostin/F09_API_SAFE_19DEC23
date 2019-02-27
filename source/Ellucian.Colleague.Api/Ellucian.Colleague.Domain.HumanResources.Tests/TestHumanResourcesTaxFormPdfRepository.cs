// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

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
                new FormT4PdfData() { TaxYear = "2016", EmployeeId = "000001" },
                new FormT4PdfData() { TaxYear = "2015", EmployeeId = "000001" },
                new FormT4PdfData() { TaxYear = "2014", EmployeeId = "000001" },
                new FormT4PdfData() { TaxYear = "2013", EmployeeId = "000001" },
                new FormT4PdfData() { TaxYear = "2012", EmployeeId = "000001" },
                new FormT4PdfData() { TaxYear = "2011", EmployeeId = "000001" },
                new FormT4PdfData() { TaxYear = "2010", EmployeeId = "000001" },
            };
        }

        public FormT4PdfData GetT4PdfAsync(string personId, string recordId)
        {
            return this.Form1098PdfDataObjects.Where(x => x.TaxYear == recordId).FirstOrDefault();
        }
    }
}
