// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestTaxFormConsentRepository : ITaxFormConsentRepository
    {
        private List<TaxFormConsent> consents = new List<TaxFormConsent>()
        {
            new TaxFormConsent("0003946", TaxForms.FormW2, false, new DateTimeOffset(new DateTime(2015, 07, 09, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormW2, true, new DateTimeOffset(new DateTime(2015, 07, 10, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormW2, false, new DateTimeOffset(new DateTime(2015, 07, 11, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormW2, true, new DateTimeOffset(new DateTime(2015, 07, 12, 02, 42, 19))),

            new TaxFormConsent("0003946", TaxForms.Form1095C, false, new DateTimeOffset(new DateTime(2015, 08, 09, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1095C, true, new DateTimeOffset(new DateTime(2015, 08, 10, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1095C, false, new DateTimeOffset(new DateTime(2015, 08, 11, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1095C, true, new DateTimeOffset(new DateTime(2015, 08, 12, 02, 42, 19))),

            new TaxFormConsent("0003946", TaxForms.Form1098, false, new DateTimeOffset(new DateTime(2015, 09, 09, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1098, true, new DateTimeOffset(new DateTime(2015, 09, 10, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1098, false, new DateTimeOffset(new DateTime(2015, 09, 11, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1098, true, new DateTimeOffset(new DateTime(2015, 09, 12, 02, 42, 19))),

            new TaxFormConsent("0003946", TaxForms.FormT4, false, new DateTimeOffset(new DateTime(2015, 09, 09, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT4, true, new DateTimeOffset(new DateTime(2015, 09, 10, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT4, false, new DateTimeOffset(new DateTime(2015, 09, 11, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT4, true, new DateTimeOffset(new DateTime(2015, 09, 12, 02, 42, 19))),

            new TaxFormConsent("0003946", TaxForms.FormT4A, false, new DateTimeOffset(new DateTime(2015, 09, 09, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT4A, true, new DateTimeOffset(new DateTime(2015, 09, 10, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT4A, false, new DateTimeOffset(new DateTime(2015, 09, 11, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT4A, true, new DateTimeOffset(new DateTime(2015, 09, 12, 02, 42, 19))),

            new TaxFormConsent("0003946", TaxForms.FormT2202A, false, new DateTimeOffset(new DateTime(2015, 09, 09, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT2202A, true, new DateTimeOffset(new DateTime(2015, 09, 10, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT2202A, false, new DateTimeOffset(new DateTime(2015, 09, 11, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.FormT2202A, true, new DateTimeOffset(new DateTime(2015, 09, 12, 02, 42, 19))),

            new TaxFormConsent("0003946", TaxForms.Form1099MI, false, new DateTimeOffset(new DateTime(2015, 09, 09, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1099MI, true, new DateTimeOffset(new DateTime(2015, 09, 10, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1099MI, false, new DateTimeOffset(new DateTime(2015, 09, 11, 02, 42, 19))),
            new TaxFormConsent("0003946", TaxForms.Form1099MI, true, new DateTimeOffset(new DateTime(2015, 09, 12, 02, 42, 19))),


        };

        public TestTaxFormConsentRepository()
        {

        }

        public async Task<IEnumerable<TaxFormConsent>> GetAsync(string personId, TaxForms taxForm)
        {
            return await Task.FromResult(this.consents.Where(x => x.TaxForm == taxForm).ToList());
        }

        public async Task<TaxFormConsent> PostAsync(TaxFormConsent inputConsent)
        {
            var returnedConsent = await Task.FromResult(inputConsent);
            return returnedConsent;
        }
    }
}
