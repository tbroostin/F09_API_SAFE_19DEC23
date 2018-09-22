// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class UserDefinedExtensions
    {
        public string AttachmentURL { get; set; }
        public string AttachmentFlag { get; set; }
        public string AttachmentSpecialInstructions { get; set; }
        public string HoldForTermId { get; set; }
        public string HoldForProgramId { get; set; }
        public string ReceivingInstitutionFiceId { get; set; }
        public string ReceivingInstitutionCeebId { get; set; }
        public string UnverifiedStudentId { get; set; }
        [XmlElement]
        public List<EnrollmentDetail> EnrollmentDetail { get; set; }
    }
}
