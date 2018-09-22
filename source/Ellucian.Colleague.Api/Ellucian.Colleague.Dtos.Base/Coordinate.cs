using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base {
    /// <summary>
    /// Geographical coordinates
    /// </summary>
    public class Coordinate {
        /// <summary>
        /// Latitude in decimal
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// Longitude in decimal
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// Indicates whether to display publicly
        /// </summary>
        public bool OfficeUseOnly { get; set; }
    }
}
