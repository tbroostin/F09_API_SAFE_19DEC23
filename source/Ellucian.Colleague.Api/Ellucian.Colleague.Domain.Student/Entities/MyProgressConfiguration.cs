// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how certain information displayed on My Progress page is controlled
    /// </summary>
    [Serializable]
    public class MyProgressConfiguration
    {
       
        /// <summary>
        /// Flag indicating whether or not current and active academic levels standing will be shown on MyProgress page
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

        /// <summary>
        /// Constructor for MyProgressConfiguration
        /// </summary>
        public MyProgressConfiguration(bool showAcadLevelStandingFlag, bool hideProgressBarOverallProgress, bool hideProgressBarTotalCredits, bool hideProgressBarTotalInstitutionalCredits)
        {
            this.ShowAcademicLevelsStanding = showAcadLevelStandingFlag;
            this.HideProgressBarOverallProgress = hideProgressBarOverallProgress;
            this.HideProgressBarTotalCredits = hideProgressBarTotalCredits;
            this.HideProgressBarTotalInstitutionalCredits = hideProgressBarTotalInstitutionalCredits;
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public MyProgressConfiguration()
        {

        }
    }
}
