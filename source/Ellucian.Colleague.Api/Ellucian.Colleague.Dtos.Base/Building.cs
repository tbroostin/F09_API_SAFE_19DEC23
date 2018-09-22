using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// This data transfer object contains the information about a building
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Unique building Id
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Brief building description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Institutionally defined building location (often campus)
        /// </summary>
        public string LocationId { get; set; }
        /// <summary>
        /// Type of building, such as residence hall or administration
        /// </summary>
        public string BuildingType { get; set; }
        /// <summary>
        /// Detailed building description
        /// </summary>
        public string LongDescription { get; set; }
        /// <summary>
        /// Street portion of building address
        /// </summary>
        public List<string> AddressLines { get; set; }
        /// <summary>
        /// City of building address
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// State of building address
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Postal code of building address
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// Country of building address
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// URL containing building image
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// Additional building services, such as accessibility
        /// </summary>
        public string AdditionalServices { get; set; }
        /// <summary>
        /// Geographical <see cref="Coordinate">coordinates</see> of the building
        /// </summary>
        public Coordinate BuildingCoordinate { get; set; }
    }
}
