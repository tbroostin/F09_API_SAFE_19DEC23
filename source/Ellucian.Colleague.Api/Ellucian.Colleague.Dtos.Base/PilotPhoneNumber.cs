using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Address 
    /// </summary>
    public class PilotPhoneNumber
    {
        /// <summary>
        /// Person ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Primary phone number for person in Pilot.
        /// </summary>
        public string PrimaryPhoneNumber { get; set; }

        /// <summary>
        /// SMS/text messaging phone number for person in PIlot.
        /// </summary>
        public string SmsPhoneNumber { get; set; }
    }
}
