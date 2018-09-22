// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class Organization
    {
        public string DUNS { get; set; }
        public string OPEID { get; set; }
        [XmlElement]
        public List<string> OrganizationName { get; set; }        
    }
}
