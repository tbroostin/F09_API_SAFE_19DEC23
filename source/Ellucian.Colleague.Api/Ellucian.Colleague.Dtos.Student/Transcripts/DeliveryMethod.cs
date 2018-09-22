using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Delivery method based on PESC XML transcript request standards
    /// </summary>
    public enum DeliveryMethod
    {
        /// <summary>
        /// Electronic
        /// </summary>
        Electronic,
        /// <summary>
        /// Express
        /// </summary>
        Express,
        /// <summary>
        /// Express Canada or Mexico
        /// </summary>
        ExpressCanadaMexico,
        /// <summary>
        /// Express International
        /// </summary>
        ExpressInternational,
        /// <summary>
        /// Express United States
        /// </summary>
        ExpressUnitedStates,
        /// <summary>
        /// Fax
        /// </summary>
        Fax,
        /// <summary>
        /// Fax Express
        /// </summary>
        FaxExpress,
        /// <summary>
        /// Fax Mail
        /// </summary>
        FaxMail,
        /// <summary>
        /// Fax Overnight
        /// </summary>
        FaxOvernight,
        /// <summary>
        /// Hold for Pick up
        /// </summary>
        HoldForPickup,
        /// <summary>
        /// Mail
        /// </summary>
        Mail,
        /// <summary>
        /// Overnight
        /// </summary>
        Overnight
    }
}
