// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Builders
{
    public class Form1095cPdfDataBuilder
    {
        public string TaxYear;
        public string Ein;
        public string Ssn;

        public Form1095cPdfDataBuilder()
        {
            this.TaxYear = "2015";
            this.Ein = "39-2587489";
            this.Ssn = "654-999-7777";
        }

        public Form1095cPdfData Build()
        {
            return new Form1095cPdfData(this.TaxYear, this.Ein, this.Ssn);
        }

        public Form1095cPdfDataBuilder WithTaxYear(string taxYear)
        {
            this.TaxYear = taxYear;
            return this;
        }

        public Form1095cPdfDataBuilder WithEin(string ein)
        {
            this.Ein = ein;
            return this;
        }

        public Form1095cPdfDataBuilder WithSsn(string ssn)
        {
            this.Ssn = ssn;
            return this;
        }
    }
}
