// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Builders
{
    public class FormW2PdfDataBuilder
    {
        public string TaxYear;
        public string EmployerId;
        public string EmployeeSsn;

        public FormW2PdfDataBuilder()
        {
            this.TaxYear = "2015";
            this.EmployerId = "77-77777777";
            this.EmployeeSsn = "000-00-0000";          
        }

        public FormW2PdfData Build()
        {
            return new FormW2PdfData(this.TaxYear, this.EmployerId, this.EmployeeSsn);
        }

        public FormW2PdfDataBuilder WithTaxYear(string taxYear)
        {
            this.TaxYear = taxYear;
            return this;
        }

        public FormW2PdfDataBuilder WithEmployerId(string employerId)
        {
            this.EmployerId = employerId;
            return this;
        }

        public FormW2PdfDataBuilder WithEmployeeSsn(string employeeSsn)
        {
            this.EmployeeSsn = employeeSsn;
            return this;
        }
    }
}