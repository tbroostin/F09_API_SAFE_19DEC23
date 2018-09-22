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
    public class TaxFormStatementBuilder
    {
        public string RecordId;
        public string PersonId;
        public string TaxYear;
        public string Notation;
        public TaxForms TaxForm;

        public TaxFormStatementBuilder()
        {
            this.RecordId = "396";
            this.PersonId = "103194";
            this.TaxYear = "2015";
            this.TaxForm = TaxForms.FormW2;
        }

        public TaxFormStatement Build()
        {
            return new TaxFormStatement(this.PersonId, this.TaxYear, this.TaxForm, this.RecordId);
        }

        public TaxFormStatementBuilder WithPersonId(string personId)
        {
            this.PersonId = personId;
            return this;
        }

        public TaxFormStatementBuilder WithTaxYear(string taxYear)
        {
            this.TaxYear = taxYear;
            return this;
        }

        public TaxFormStatementBuilder WithTaxForm(TaxForms taxForm)
        {
            this.TaxForm = taxForm;
            return this;
        }

        public TaxFormStatementBuilder WithRecordId(string recordId)
        {
            this.RecordId = recordId;
            return this;
        }
    }
}
