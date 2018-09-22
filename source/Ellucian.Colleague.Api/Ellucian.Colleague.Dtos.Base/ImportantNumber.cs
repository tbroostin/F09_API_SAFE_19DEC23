using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base {
    /// <summary>
    /// Institutionally defined important number
    /// </summary>
    public class ImportantNumber {
        /// <summary>
        /// Name of this number
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Category code of important number
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Indicates whether to make publicly visible
        /// </summary>
        public bool OfficeUseOnly { get; set; }
        /// <summary>
        /// Phone number
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Phone extension
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// E-mail address 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Building code
        /// </summary>
        public string BuildingCode { get; set; }
        /// <summary>
        /// Street portion of address
        /// </summary>
        public List<string> AddressLines { get; set; }
        /// <summary>
        /// City portion of address
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// State portion of address
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Postal code of the address
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// Country of address
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Geographical latitude
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// Geographical longitude
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// Institutionally defined location, often campus
        /// </summary>
        public string LocationCode { get; set; }
    }
}
