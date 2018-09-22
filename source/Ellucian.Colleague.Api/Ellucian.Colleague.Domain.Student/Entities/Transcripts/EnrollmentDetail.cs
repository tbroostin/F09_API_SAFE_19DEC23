// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class EnrollmentDetail
    {
        public string NameOfProgram { get; set; }
        public string BeginYear { get; set; }
        public string EndYear { get; set; }
    }
}
