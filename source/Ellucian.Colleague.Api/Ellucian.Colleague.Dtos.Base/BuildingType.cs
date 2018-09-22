using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    ///  This data transfer object contains information about a building type.
    /// </summary>
    public class BuildingType
    {
        /// <summary>
        /// Building Type code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Building Type code description
        /// </summary>
        public string Description { get; set; }
    }
}
