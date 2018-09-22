// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestColleagueFinanceTaxFormPdfDataRepository
    {
        public List<Form1099MIPdfData> Form1099MiPdfDataObjects;

        public TestColleagueFinanceTaxFormPdfDataRepository()
        {
            this.Form1099MiPdfDataObjects = new List<Form1099MIPdfData>()
            {
                CreateForm1099MIPdfData("2017","1"),
                new Form1099MIPdfData("2016", "2")
            };
        }

        public Form1099MIPdfData Get1099MiPdfDataAsync(string personId, string recordId)
        {
            return this.Form1099MiPdfDataObjects.Where(x => x.TaxYear == recordId).FirstOrDefault();
        }

        private Form1099MIPdfData CreateForm1099MIPdfData(string year, string payerName)
        {
            var formData = new Form1099MIPdfData(year, payerName);
            return formData;
        }


    }
}
