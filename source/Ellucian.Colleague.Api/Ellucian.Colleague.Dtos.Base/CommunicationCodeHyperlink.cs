/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A CommunicationCodeHyperlink describes a hyperlink to an online communication resource
    /// </summary>
    public class CommunicationCodeHyperlink
    {
        /// <summary>
        /// The URL of the hyperlink
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The Title of the hyperlink that should be displayed to users
        /// </summary>
        public string Title { get; set; }
    }
}
