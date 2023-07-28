/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities

{
    [Serializable]
    public class Address
    {
        /// <summary>
        /// The student's permanent mailing address.
        /// </summary>
        public string AddressLine { get; set; }

        /// <summary>
        /// The student's city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The student's state of residence.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The student's zip code.
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// The student's country for this aid application.
        /// </summary>
        public string Country { get; set; }
    }
}
