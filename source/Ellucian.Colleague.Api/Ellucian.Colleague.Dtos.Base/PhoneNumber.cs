using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Address 
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Person residing at this address
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// Phone numbers related to this address
        /// </summary>
        public ICollection<Phone> PhoneNumbers { get; set; }
    }
}
