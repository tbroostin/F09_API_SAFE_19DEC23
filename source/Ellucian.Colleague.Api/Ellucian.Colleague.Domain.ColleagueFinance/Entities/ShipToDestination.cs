//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Entities;

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
        public List<string> addressLines;

		/// <summary>
		/// Country code of place
		/// </summary>
        public string placeCountryCode;

        /// <summary>
        /// Country title of place
        /// </summary>
        public string placeCountryTitle;

        /// <summary>
        /// Postal title of place
        /// </summary>
        public string placeCountryPostalTitle;

        /// <summary>
        /// Country region code of place
        /// </summary>
        public string placeCountryRegionCode;

        /// <summary>
        /// Country region title of place
        /// </summary>
        public string placeCountryRegionTitle;

        /// <summary>
        /// Country sub-region code of place
        /// </summary>
        public string placeCountrySubRegionCode;

        /// <summary>
        /// Country sub-region title of place
        /// </summary>
        public string placeCountrySubRegionTitle;

        /// <summary>
        /// The name of the city or town
        /// </summary>
        public string placeCountryLocality;

        /// <summary>
        /// The mailing postal code
        /// </summary>
        public string placeCountryPostalCode;

        /// <summary>
        /// Contact name
        /// </summary>
        public string contactName;

        /// <summary>
        /// The phone number of the contact
        /// </summary>
        public string phoneNumber;

        /// <summary>
        /// The extension of the contact
        /// </summary>
        public string phoneExtension;

        /// <summary>
        /// Tax code associated with destination
        /// </summary>
        public string taxCode;

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