using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The response received from a registration request
    /// </summary>
    public class RegistrationResponse
    {
        /// <summary>
        /// A list of <see cref="RegistrationMessage">registration messages</see>
        /// </summary>
        public List<RegistrationMessage> Messages { get; set; }

        /// <summary>
        /// The ID of the related registration payment control
        /// </summary>
        public string PaymentControlId { get; set; }
    }
}
