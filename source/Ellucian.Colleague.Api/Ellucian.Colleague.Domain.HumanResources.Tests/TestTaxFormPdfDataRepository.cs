// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestTaxFormPdfDataRepository
    {
        public List<FormW2PdfData> FormW2PdfDataObjects;
        public List<Form1095cPdfData> Form1095CPdfDataObjects;

        public TestTaxFormPdfDataRepository()
        {
            this.FormW2PdfDataObjects = new List<FormW2PdfData>()
            {
                new FormW2PdfData("2016", "12-345678", "000-00-0001") { EmployeeId = "000001" },
                new FormW2PdfData("2015", "12-345678", "000-00-0001") { EmployeeId = "000001" },
                new FormW2PdfData("2014", "12-345678", "000-00-0001") { EmployeeId = "000001" },
                new FormW2PdfData("2013", "12-345678", "000-00-0001") { EmployeeId = "000001" },
                new FormW2PdfData("2012", "12-345678", "000-00-0001") { EmployeeId = "000001" },
                new FormW2PdfData("2011", "12-345678", "000-00-0001") { EmployeeId = "000001" },
                new FormW2PdfData("2010", "12-345678", "000-00-0001") { EmployeeId = "000001" },
            };

            var form1095C = new Form1095cPdfData("2015", "12-345678", "000-00-0001")
            {
                EmployeeId = "000001",
                EmployeePostalCode = "20191",
                EmployeeZipExtension = "1234",
                EmployeeCountry = "US",
                IsCorrected = true,
                IsVoided = false,
                LowestCostAmount12Month = 250m,
            };
            form1095C.AddCoveredIndividual(new Form1095cCoveredIndividualsPdfData()
            {
                CoveredIndividualFirstName = "Emma",
                CoveredIndividualLastName = "Kleehammer",
                Covered12Month = true,
            });

            this.Form1095CPdfDataObjects = new List<Form1095cPdfData>()
            {
                new Form1095cPdfData("2016", "12-345678", "000-00-0001"),
                form1095C,
            };
        }

        public FormW2PdfData GetW2PdfAsync(string personId, string recordId)
        {
            return this.FormW2PdfDataObjects.Where(x => x.TaxYear == recordId).FirstOrDefault();
        }

        public Form1095cPdfData Get1095cPdfAsync(string personId, string recordId)
        {
            return this.Form1095CPdfDataObjects.Where(x => x.TaxYear == recordId).FirstOrDefault();
        }
    }
}
