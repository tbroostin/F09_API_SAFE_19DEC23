//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// ShipToCodes
    /// </summary>
    [Serializable]
    public class ShipToDestination : GuidCodeItem
    {

        /// <summary>
        /// The address lines of the ship to destination location
        /// </summary>
        public List<string> addressLines { get; set; }

		/// <summary>
		/// Country code of place
		/// </summary>
        public string placeCountryCode { get; set; }

        /// <summary>
        /// Country title of place
        /// </summary>
        public string placeCountryTitle { get; set; }

        /// <summary>
        /// Postal title of place
        /// </summary>
        public string placeCountryPostalTitle { get; set; }

        /// <summary>
        /// Country region code of place
        /// </summary>
        public string placeCountryRegionCode { get; set; }

        /// <summary>
        /// Country region title of place
        /// </summary>
        public string placeCountryRegionTitle { get; set; }

        /// <summary>
        /// Country sub-region code of place
        /// </summary>
        public string placeCountrySubRegionCode { get; set; }

        /// <summary>
        /// Country sub-region title of place
        /// </summary>
        public string placeCountrySubRegionTitle { get; set; }

        /// <summary>
        /// The name of the city or town
        /// </summary>
        public string placeCountryLocality { get; set; }

        /// <summary>
        /// The mailing postal code
        /// </summary>
        public string placeCountryPostalCode { get; set; }

        /// <summary>
        /// Contact name
        /// </summary>
        public string contactName { get; set; }

        /// <summary>
        /// The phone number of the contact
        /// </summary>
        public string phoneNumber { get; set; }

        /// <summary>
        /// The extension of the contact
        /// </summary>
        public string phoneExtension { get; set; }

        /// <summary>
        /// Tax code associated with destination
        /// </summary>
        public string taxCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipToDestination"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ShipToDestination(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}