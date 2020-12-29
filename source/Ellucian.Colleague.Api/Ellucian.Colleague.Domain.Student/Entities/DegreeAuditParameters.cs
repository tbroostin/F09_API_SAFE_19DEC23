// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class DegreeAuditParameters
    {
        public bool ModifiedDefaultSort { get; set; }

        /// <summary>
        /// Extra course handling method (D - display, I - Ignore, S - Semi-apply, A - Apply Fully
        /// </summary>
        public ExtraCourses ExtraCourseHandling { get; set; }

        /// <summary>
        /// Include failures
        /// </summary>
        public bool UseLowGrade { get; set; }
        /// <summary>
        /// A flag to indicate if related courses should be shown with applied courses on a group.
        /// If the value is Y then it will display related courses.
        /// </summary>
        public bool ShowRelatedCourses { get; set; }
        /// <summary>
        /// Constructor for DegreeAuditParameters
        /// </summary>
        /// <param name="extraCourses">What method to use for extra courses</param>
        /// <param name="useLowGrade">Should low grades be used</param>
        /// <param name="modifiedDefaultSort">Has default sort specification been modified</param>
        public DegreeAuditParameters(ExtraCourses extraCourses, bool showRelatedCourses=false,  bool useLowGrade = false, bool modifiedDefaultSort = false)
        {
            ExtraCourseHandling = extraCourses;
            UseLowGrade = useLowGrade;
            ModifiedDefaultSort = modifiedDefaultSort;
            ShowRelatedCourses = showRelatedCourses;
        }
    }
}
