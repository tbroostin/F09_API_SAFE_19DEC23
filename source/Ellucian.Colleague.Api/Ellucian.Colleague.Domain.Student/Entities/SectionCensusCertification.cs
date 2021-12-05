// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section's Census Certification- Identifies when the census on particular date was certified. 
    /// </summary>
    [Serializable]
    public class SectionCensusCertification
    {
        /// <summary>
        /// Section's Census Date that got verified 
        /// </summary>
        public DateTime? CensusCertificationDate { get; private set; }
        /// <summary>
        /// Position of the Census that got certified 
        /// </summary>
        public string CensusCertificationPosition { get; private set; }
        /// <summary>
        /// Label of the Census Position that got certified 
        /// </summary>
        public string CensusCertificationLabel { get; private set; }
        /// <summary>
        /// Date when Census was certified
        /// </summary>
        public DateTime? CensusCertificationRecordedDate { get; private set; }
        /// <summary>
        /// Time when Census was certified 
        /// </summary>
        public DateTime? CensusCertificationRecordedTime { get; private set; }
        /// <summary>
        /// Person Id of the person who certified the census 
        /// </summary>
        public string PersonId { get; private set; }
        

        public SectionCensusCertification()
        {

        }
        /// <summary>
        /// Heavy load constructor when retrieving or populating census certified dates for the section
        /// </summary>
        /// <param name="SecCertCensusDate"></param>
        /// <param name="SecCertPosition"></param>
        /// <param name="SecCertPositionLabel"></param>
        /// <param name="SecCertRecordedDate"></param>
        /// <param name="SecCertRecordedTime"></param>
        /// <param name="SecCertPersonId"></param>
        public SectionCensusCertification(DateTime? SecCertCensusDate, string SecCertPosition, string SecCertPositionLabel, DateTime? SecCertRecordedDate, DateTime? SecCertRecordedTime, string SecCertPersonId)
        {
            this.CensusCertificationDate = SecCertCensusDate;
            this.CensusCertificationPosition = SecCertPosition;
            this.CensusCertificationLabel = SecCertPositionLabel;
            this.CensusCertificationRecordedDate = SecCertRecordedDate;
            this.CensusCertificationRecordedTime = SecCertRecordedTime;
            this.PersonId = SecCertPersonId;
        }
        
    }
}
