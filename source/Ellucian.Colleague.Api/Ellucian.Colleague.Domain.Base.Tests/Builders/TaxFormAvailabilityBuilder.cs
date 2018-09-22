// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Builders
{
    public class TaxFormAvailabilityBuilder
    {
        public string TaxYear;
        public DateTime? AvailableDate;
        public bool Available;

        public TaxFormAvailabilityBuilder()
        {
            this.TaxYear = "2014";
            this.AvailableDate = null;
            this.Available = false;
        }

        public TaxFormAvailability Build()
        {
            if (this.AvailableDate.HasValue)
            {
                return new TaxFormAvailability(this.TaxYear, this.AvailableDate);
            }

            return new TaxFormAvailability(this.TaxYear, this.Available);
        }

        public TaxFormAvailabilityBuilder WithTaxYear(string taxYear)
        {
            this.TaxYear = taxYear;
            return this;
        }

        public TaxFormAvailabilityBuilder WithAvailabilityDate(DateTime? availabilityDate)
        {
            this.AvailableDate = availabilityDate;
            return this;
        }

        public TaxFormAvailabilityBuilder WithAvailableBool(bool available)
        {
            this.Available = available;
            return this;
        }
    }
}
