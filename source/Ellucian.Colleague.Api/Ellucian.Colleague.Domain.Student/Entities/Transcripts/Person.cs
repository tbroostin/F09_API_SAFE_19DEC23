// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class Person
    {
        public string SchoolAssignedPersonID { get; set; }
        public string SSN { get; set; }
        public Birth Birth { get; set; }
        public Name Name { get; set; }
        public AlternateName AlternateName { get; set; }
        [XmlElement]
        public List<Contacts> Contacts { get; set; }
    }
}
