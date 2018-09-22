// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class RequestorReceiverOrganization
    {
        public string OrganizationName { get; set; }
        [XmlElement]
        public List<Contacts> Contacts { get; set; }
        public string OPEID { get; set; }
    }
}
