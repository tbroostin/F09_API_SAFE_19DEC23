// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains information that controls how certain information displayed on My Progress page is controlled
    /// </summary>
    public class MyProgressConfiguration
    {
       
        /// <summary>
        /// Flag indicating whether or not current and active academic levels standing will be shown on My progress page
        /// </summary>
        public bool ShowAcademicLevelsStanding { get; set; }
        /// <summary>
        /// Flag to hide the display of progress bar that indicates Total Progress of requirments completion on MyProgress page.
        /// </summary>
        public bool HideProgressBarOverallProgress { get; set; }
        /// <summary>
        /// Flag to hide the display of progress bar that indicates Total Credits on MyProgress page.
        /// </summary>
        public bool HideProgressBarTotalCredits { get; set; }
        /// <summary>
        /// Flag to hide the display of progress bar that indicates Total Credits from this School on MyProgress page.
        /// </summary>
        public bool HideProgressBarTotalInstitutionalCredits { get; set; }
    }
}
