// Copyright 2017 Ellucian Company L.P. and its affiliates
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contact measures valcode Table
    /// </summary>
    [Serializable]
    public class ContactMeasure : CodeItem
    {
        private ContactPeriod _contactPeriodType;
        public ContactPeriod ContactPeriod { get { return _contactPeriodType; } }

        /// <summary>
        /// Overloaded constructor for contact measures
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="periodType"></param>

        public ContactMeasure(string code, string description, string periodType) : base(code, description)
        {
            ContactPeriod period;
            // Customers must map the contact values or courses v12 will not work.  This 
            // was written for v11, but will be temporarily removed until v12.
            //if (string.IsNullOrEmpty(periodType)) throw new ApplicationException("Contact measure '" + code + "'  is not mapped to an integration value.  Please check your setup.");

            switch (periodType.ToLowerInvariant())
            {
                case "day": period = ContactPeriod.day; break;
                case "week": period = ContactPeriod.week; break;
                case "month": period = ContactPeriod.month; break;
                case "term": period = ContactPeriod.term; break;

                // Customers must map the contact values or courses v12 will not work.  This 
                // was written for v11, but will be temporarily removed until v12.
                //default: throw new ApplicationException("Contact measure '" + code + "'  is not mapped to a valid integration value.  Please check your setup.");
                default: period = ContactPeriod.notSet; break;
            }
            _contactPeriodType = period;
        }
    }
}
