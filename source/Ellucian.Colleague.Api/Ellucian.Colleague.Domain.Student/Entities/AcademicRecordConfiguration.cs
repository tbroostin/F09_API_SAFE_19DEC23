// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how certain academic record information will be displayed
    /// </summary>
    [Serializable]
    public class AcademicRecordConfiguration
    {
        /// <summary>
        /// when anonymous grading is used random id was assigned by Term or Section
        /// </summary>
        public AnonymousGradingType AnonymousGradingType { get; set; }

        /// <summary>
        /// Constructor for AcademicRecordConfiguration
        /// </summary>
        public AcademicRecordConfiguration(AnonymousGradingType anonymousGradingType)
        {
            this.AnonymousGradingType = anonymousGradingType;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AcademicRecordConfiguration()
        {
        }
    }
}
