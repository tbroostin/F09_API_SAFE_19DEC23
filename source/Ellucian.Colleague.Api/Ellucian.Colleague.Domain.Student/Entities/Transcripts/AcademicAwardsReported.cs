﻿// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class AcademicAwardsReported
    {
        public string AcademicAwardTitle { get; set; }
        public DateTime AcademicAwardDate { get; set; }
    }
}
