// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Section's Census Certification- Identifies when the census on particular date was certified. 
    /// </summary>
    public class SectionCensusCertification
    {
        /// <summary>
        /// Section's Census Date
        /// </summary>
        public DateTime? CensusCertificationDate { get;  set; }
        /// <summary>
        /// Position of the Census
        /// </summary>
        public string CensusCertificationPosition { get;  set; }
        /// <summary>
        /// Label of the Census Position
        /// </summary>
        public string CensusCertificationLabel { get;  set; }
        /// <summary>
        /// Date when Census was certified
        /// </summary>
        public DateTime? CensusCertificationRecordedDate { get; set; }
        /// <summary>
        /// Time when Census was certified
        /// </summary>
        public DateTime? CensusCertificationRecordedTime { get; set; }

    }
}
