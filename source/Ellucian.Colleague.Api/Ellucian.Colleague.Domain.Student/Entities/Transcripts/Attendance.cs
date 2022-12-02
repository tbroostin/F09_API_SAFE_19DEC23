﻿// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class Attendance
    {
        public School School { get; set; }
        public DateTime? EnrollDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public string CurrentEnrollmentIndicator { get; set; }
        [XmlElement]
        private List<AcademicAwardsReported> academicAwardsReported = new List<AcademicAwardsReported>();
        public List<AcademicAwardsReported> AcademicAwardsReported
        {
            get { return academicAwardsReported; }
            set { if (value != null) { academicAwardsReported = value; } }
        }
    }
}
