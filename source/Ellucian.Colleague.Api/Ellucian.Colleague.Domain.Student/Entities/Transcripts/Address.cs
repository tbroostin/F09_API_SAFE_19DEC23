// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Xml.Serialization;
using System.Collections.Generic;


namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class Address
    {
        [XmlElement]
        public List<string> AddressLine { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string StateProvinceCode { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        [XmlElement]
        public List<string> AttentionLine { get; set; }
    }
}
