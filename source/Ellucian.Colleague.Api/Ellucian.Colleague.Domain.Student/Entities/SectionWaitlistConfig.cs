// Copyright 2021 Ellucian Company L.P. and its affiliates.
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

        private bool? _sortOrderByRank;
        /// <summary>
        /// Sort waitlist by Rank in faculty roster page
        /// </summary>
        public bool? SortOrderByRank { get { return _sortOrderByRank; } }

        private bool? _waitlistSortByDate;
        /// <summary>
        /// Sort waitlist by Date in faculty roster page
        /// </summary>
        public bool? WaitlistSortByDate { get { return _waitlistSortByDate; } }
        private int? _noOfDaysToEnroll { get; set; }
        /// <summary>
        /// Holds the section waitlist enroll no of days
        /// </summary>
        public int? NoOfDaysToEnroll { get { return _noOfDaysToEnroll; } }

        private bool? _inlcudeCrossListedSections;
        /// <summary>
        /// Include the cross listed sections based on FCWP setting
        /// </summary>
        public bool? InlcudeCrossListedSections { get { return _inlcudeCrossListedSections; } }


        /// <summary>
        /// Creates a new instance of the <see cref="SectionWaitlistConfig"/> class
        /// </summary>
        /// <param name="sectionId">Id of the section </param>
        /// <param name="displayRank">Display Rank</param>
        /// <param name="displayRating">Display Rating</param>
        /// <param name="sortOrderByRank">Sort waitlist by Rank</param>
        /// <param name="noOfDaysToEnroll">Section waitlist enroll no of days</param>
        /// <param name="waitlistSortByDate">Sort waitlist by Date</param>
        /// <param name="inlcudeCrossListedSections">Include the cross listed sections</param>

        public SectionWaitlistConfig(string sectionId,bool displayRank, bool displayRating, int? noOfDaysToEnroll, bool? sortOrderByRank,bool? waitlistSortByDate, bool? includeCrossListedSections)
        {           
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot get a section waitlist config without a section ID.");
            }
            _showRank = displayRank;
            _showRating = displayRating;
            _noOfDaysToEnroll = noOfDaysToEnroll;
            _sortOrderByRank = sortOrderByRank;
            _waitlistSortByDate = waitlistSortByDate;
            _inlcudeCrossListedSections = includeCrossListedSections;


        }
    }
}

