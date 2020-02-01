// Copyright 2013-2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.DegreePlans
{
    /// <summary>
    /// Warnings applied to planned courses on a course plan depending on various situations: missing requisite, time conflict, etc.
    /// </summary>
    [Serializable]
    public class PlannedCourseWarning
    {
        /// <summary>
        /// PlannedCourseWarning Constructor
        /// </summary>
        /// <param name="typeype">Enumerable warning type, must also be in business-layer degree plan resource file</param>
        public PlannedCourseWarning(PlannedCourseWarningType type)
        {
            _Type = type;
        }

        /// <summary>
        /// Warning type, which equates to actual warning text in presentation layer resource file DegreePlan
        /// </summary>
        private PlannedCourseWarningType _Type;
        public PlannedCourseWarningType Type
        {
            get { return _Type; }
        }


        /// <summary>
        /// Id of section to reference in warning (Used in time conflicts)
        /// May also be the ID of a recommended section requisite
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// If the warning is an unmet course requisite - this is the requisite that is unmet.
        /// </summary>
        public Requisite Requisite { get; set; }

        /// <summary>
        /// If the warning is an unmet section requisite - this is the section requisite that is unmet.
        /// </summary>
        public SectionRequisite SectionRequisite { get; set; }

    }

    // Currently leaving the Coreq section messages here until section work is complete
    [Serializable]
    public enum PlannedCourseWarningType
    {
        InvalidPlannedCredits,
        NegativePlannedCredits,
        TimeConflict,
        UnmetRequisite,
        CourseOfferingConflict
    }
}
