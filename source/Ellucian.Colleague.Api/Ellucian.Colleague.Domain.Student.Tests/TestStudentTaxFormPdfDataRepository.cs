// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentTaxFormPdfDataRepository
    {
        public List<Form1098PdfData> Form1098PdfDataObjects;
        public List<FormT2202aPdfData> FormT2202aPdfDataObjects;

        public TestStudentTaxFormPdfDataRepository()
        {
            this.Form1098PdfDataObjects = new List<Form1098PdfData>()
            {
                new Form1098PdfData("2016", "1"),
                new Form1098PdfData("2015", "2"),
                new Form1098PdfData("2014", "3"),
                new Form1098PdfData("2013", "4"),
                new Form1098PdfData("2012", "5"),
                new Form1098PdfData("2011", "6"),
                new Form1098PdfData("2010", "7"),
                new Form1098PdfData("2017", "8")
            };
            this.FormT2202aPdfDataObjects = new List<FormT2202aPdfData>()
            {
                new FormT2202aPdfData("2016", "1"),
                new FormT2202aPdfData("2015", "2"),
                new FormT2202aPdfData("2014", "3"),
                new FormT2202aPdfData("2013", "4"),
                new FormT2202aPdfData("2012", "5"),
                new FormT2202aPdfData("2011", "6"),
                new FormT2202aPdfData("2010", "7")
            };
        }

        public Form1098PdfData Get1098PdfAsync(string personId, string recordId)
        {
            return this.Form1098PdfDataObjects.Where(x => x.TaxYear == recordId).FirstOrDefault();
        }

        public FormT2202aPdfData GetFormT2202aPdfDataAsync(string personId, string recordId)
        {
            return this.FormT2202aPdfDataObjects.Where(x => x.TaxYear == recordId).FirstOrDefault();
        }
    }
}
