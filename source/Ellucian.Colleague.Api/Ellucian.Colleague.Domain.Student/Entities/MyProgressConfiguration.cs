// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
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
        public bool ShowAcademicLevelsStanding { get; private set; }
        /// <summary>
        /// Flag to hide the display of progress bar that indicates Total Progress of requirments completion on MyProgress page.
        /// </summary>
        public bool HideProgressBarOverallProgress { get; private set; }
        /// <summary>
        /// Flag to hide the display of progress bar that indicates Total Credits on MyProgress page.
        /// </summary>
        public bool HideProgressBarTotalCredits { get; private set; }
        /// <summary>
        /// Flag to hide the display of progress bar that indicates Total Credits from this School on MyProgress page.
        /// </summary>
        public bool HideProgressBarTotalInstitutionalCredits { get; private set; }
        /// <summary>
        /// This flag is used to control whether or not Self-Service should show pseudo courses that are referenced in academic program requirements. 
        /// <remarks>For reference, a pseudo course is any course with a Course Type with a special processing code 1 value of "P."
        /// When this flag is false then Self-Service will not show pseudo courses that are referenced in academic program requirements.
        /// When this flag is true then Self-Service will show pseudo courses that are referenced in academic program requirements.
        /// </remarks>
        /// </summary>
        public bool ShowPseudoCoursesInRequirements { get; private set; }

        /// <summary>
        /// Constructor for MyProgressConfiguration
        /// </summary>
        public MyProgressConfiguration(bool showAcadLevelStandingFlag, bool hideProgressBarOverallProgress, bool hideProgressBarTotalCredits, bool hideProgressBarTotalInstitutionalCredits, bool showPseudoCoursesInRequirements)
        {
            this.ShowAcademicLevelsStanding = showAcadLevelStandingFlag;
            this.HideProgressBarOverallProgress = hideProgressBarOverallProgress;
            this.HideProgressBarTotalCredits = hideProgressBarTotalCredits;
            this.HideProgressBarTotalInstitutionalCredits = hideProgressBarTotalInstitutionalCredits;
            this.ShowPseudoCoursesInRequirements = showPseudoCoursesInRequirements;
        }
    }
}
