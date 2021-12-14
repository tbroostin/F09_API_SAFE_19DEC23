// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains information that controls how certain academic record information will be displayed
    /// </summary>
    public class AcademicRecordConfiguration
    {
        /// <summary>
        /// when anonymous grading is used random id was assigned by Term or Section
        /// </summary>
        public AnonymousGradingType AnonymousGradingType { get; set; }
    }
}
