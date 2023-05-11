﻿// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    [XmlRoot("TranscriptRequest", Namespace = "urn:org:pesc:message:TranscriptRequest:v1.2.0")]
    public class TranscriptRequest
    {
        [XmlNamespaceDeclarations]
        private XmlSerializerNamespaces _xmlns = new XmlSerializerNamespaces();
        public XmlSerializerNamespaces xmlns
        {
            get { return _xmlns; }
            set { if (value != null) { _xmlns = value; } }
        }

        public TransmissionData TransmissionData { get; set; }
        public string DocumentID { get; set; }
        public Request Request { get; set; }
        public string NoteMessage { get; set; }
        public UserDefinedExtensions UserDefinedExtensions { get; set; }
        

        public TranscriptRequest()
        {
            _xmlns.Add("core", "urn:org:pesc:core:CoreMain:v1.0.0");
            _xmlns.Add("AcRec", "urn:org:pesc:sector:AcademicRecord:v1.0.0");
            _xmlns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        }

    }
}
