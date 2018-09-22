// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentConfiguration
    {
        public string FacultyEmailTypeCode { get; set; }
        public string FacultyPhoneTypeCode { get; set; }
        public bool EnforceTranscriptRestriction { get; set; }
    }
}
