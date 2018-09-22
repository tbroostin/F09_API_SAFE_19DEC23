using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A room based in a building
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Unique system Id of this room
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Descriptive code for this room, such as room number
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// The building this room is in
        /// </summary>
        public string BuildingCode { get; set; }
        /// <summary>
        /// The room number
        /// </summary>
        public string Number { get; set; }
    }
}
