using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Organization
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// DUNS id
        /// </summary>
        [XmlElement(Namespace = "")]
        public string DUNS { get; set; }
        /// <summary>
        /// OPE ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string OPEID { get; set; }
        /// <summary>
        /// Organization name
        /// </summary>
        [XmlElement(Namespace = "")]
        public List<string> OrganizationName { get; set; }        
    }
}
