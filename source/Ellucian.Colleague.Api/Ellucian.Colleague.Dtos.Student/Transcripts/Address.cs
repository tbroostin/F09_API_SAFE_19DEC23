using System.Xml.Serialization;
using System.Collections.Generic;


namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Address information
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Address line
        /// </summary>
        [XmlElement(Namespace = "")]  // this list needs XmlElement even if not the namespace decoration
        public List<string> AddressLine { get; set; }
        /// <summary>
        /// City
        /// </summary>
        [XmlElement(Namespace = "")]
        public string City { get; set; }
        /// <summary>
        /// State or Province
        /// </summary>
        [XmlElement(Namespace = "")]
        public string StateProvince { get; set; }
        /// <summary>
        /// State or Province code
        /// </summary>
        [XmlElement(Namespace = "")]
        public string StateProvinceCode { get; set; }
        /// <summary>
        /// Postal Code
        /// </summary>
        [XmlElement(Namespace = "")]
        public string PostalCode { get; set; }
        /// <summary>
        /// Country Code
        /// </summary>
        [XmlElement(Namespace = "")]
        public string CountryCode { get; set; }
        /// <summary>
        /// Attention line
        /// </summary>
        [XmlElement(Namespace = "")] // this list needs XmlElement even if not the namespace decoration
        public List<string> AttentionLine { get; set; }
    }
}
