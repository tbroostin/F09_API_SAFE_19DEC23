// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Builders
{
    public class TaxFormConsentBuilder2
    {
        public string PersonId;
        public bool HasConsented2;
        public string TaxForm;
        public DateTimeOffset TimeStamp;

        public TaxFormConsentBuilder2()
        {
            this.PersonId = "0003946";
            this.HasConsented2 = true;
            this.TimeStamp = new DateTimeOffset(new DateTime(2015, 07, 09, 01, 58, 31));
        }

        public TaxFormConsent2 Build2()
        {
            return new TaxFormConsent2(this.PersonId, this.TaxForm, this.HasConsented2, this.TimeStamp);
        }

        public TaxFormConsentBuilder2 WithHasConsented2(bool hasConsented2)
        {
            this.HasConsented2 = hasConsented2;
            return this;
        }

        public TaxFormConsentBuilder2 WithTaxForm2(string taxForm)
        {
            this.TaxForm = taxForm;
            return this;
        }
    }
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
