// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Builders
{
    public class TaxFormConsentBuilder
    {
        public string PersonId;
        public TaxForms TaxForm;
        public bool HasConsented;
        public DateTimeOffset TimeStamp;

        public TaxFormConsentBuilder()
        {
            this.PersonId = "0003946";
            this.TaxForm = TaxForms.FormW2;
            this.HasConsented = true;
            this.TimeStamp = new DateTimeOffset(new DateTime(2015, 07, 09, 01, 58, 31));
        }

        public TaxFormConsent Build()
        {
            return new TaxFormConsent(this.PersonId, this.TaxForm, this.HasConsented, this.TimeStamp);
        }

        public TaxFormConsentBuilder WithHasConsented(bool hasConsented)
        {
            this.HasConsented = hasConsented;
            return this;
        }

        public TaxFormConsentBuilder WithTaxForm(TaxForms taxForm)
        {
            this.TaxForm = taxForm;
            return this;
        }
    }
}
