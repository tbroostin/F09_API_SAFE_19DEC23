// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Waitlist Section setting to set the display of rank and rating columns
    /// </summary>
    [Serializable]
    public class SectionWaitlistConfig
    {
        private bool _showRank;

        /// <summary>
        /// Display rank
        /// </summary>
        public bool ShowRank { get { return _showRank; } }

        private bool _showRating;

        /// <summary>
        /// Display rating
        /// </summary>
        public bool ShowRating { get { return _showRating; } }

        private int? _noOfDaysToEnroll { get; set; }
        /// <summary>
        /// Holds the section waitlist enroll no of days
        /// </summary>
        public int? NoOfDaysToEnroll { get { return _noOfDaysToEnroll; } }


        /// <summary>
        /// Creates a new instance of the <see cref="SectionWaitlistConfig"/> class
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        public SectionWaitlistConfig(string sectionId,bool displayRank, bool displayRating, int? noOfDaysToEnroll)
        {           
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot get a section waitlist config without a section ID.");
            }
            _showRank = displayRank;
            _showRating = displayRating;
            _noOfDaysToEnroll = noOfDaysToEnroll;
        }
    }
}

