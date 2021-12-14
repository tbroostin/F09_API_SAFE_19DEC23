// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Identifies which census date needs to be certified.
    /// </summary>
    public class SectionCensusToCertify
    {
        /// <summary>
        /// Section's Census Date to be certified
        /// </summary>
        public DateTime? CensusCertificationDate { get;  set; }
       
        /// <summary>
        /// Position of the Census  to be certified
        /// </summary>
        public string CensusCertificationPosition { get;  set; }
        /// <summary>
        /// Label of the Census Position to be certified
        /// </summary>
        public string CensusCertificationLabel { get;  set; }
        /// <summary>
        /// Date when Census was being marked as certified by the user
        /// </summary>
        public DateTime? CensusCertificationRecordedDate { get; set; }
        /// <summary>
        /// Time when Census was being marked as certified by the user
        /// </summary>
        public DateTimeOffset? CensusCertificationRecordedTime { get; set; }
    }
}
