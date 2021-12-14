// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A page of sections displayed in the section search results
    /// <see cref="Section4"/>
    /// <see cref="DataPage{T}"/>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")]
    public class SectionPage2 : DataPage<Section4>
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public SectionPage2()
            : base()
        {
            Subjects = new List<Filter>();
            DaysOfWeek = new List<Filter>();
            Locations = new List<Filter>();
            CourseTypes = new List<Filter>();
            Faculty = new List<Filter>();
            TopicCodes = new List<Filter>();
            Terms = new List<Filter>();
            OnlineCategories = new List<Filter>();
            OpenSections = new Filter();

        }

        /// <summary>
        /// SectionPage constructor initializes the basic page data
        /// </summary>
        /// <param name="allItems">List of <see cref="Section4">Section</see> items</param>
        /// <param name="pageSize">Size of page to construct</param>
        /// <param name="pageIndex">Page number</param>
        public SectionPage2(IEnumerable<Section4> allItems, int pageSize, int pageIndex)
                : base(allItems, pageSize, pageIndex)
        {

        }
        /// <summary>
        /// Course <see cref="Filter">filter</see> results for Subjects
        /// </summary>
        public IEnumerable<Filter> Subjects { get; set; }
        /// <summary>
        /// Section <see cref="Filter">filter</see> results for DaysOfWeek
        /// </summary>
        public IEnumerable<Filter> DaysOfWeek { get; set; }
        /// <summary>
        /// Section <see cref="Filter">filter</see> results for Location
        /// </summary>
        public IEnumerable<Filter> Locations { get; set; }
        /// <summary>
        /// Section <see cref="Filter">filter</see> results for Faculty
        /// </summary>
        public IEnumerable<Filter> Faculty { get; set; }
        /// <summary>
        /// Section <see cref="Filter">filter</see> results for CourseType
        /// </summary>
        public IEnumerable<Filter> CourseTypes { get; set; }
        /// <summary>
        /// Section <see cref="Filter">filter</see> results for CourseLevel
        /// </summary>
        public IEnumerable<Filter> CourseLevels { get; set; }
        /// <summary>
        /// Section <see cref="Filter">filter</see> results for AcademicLevel
        /// </summary>
        public IEnumerable<Filter> AcademicLevels { get; set; }
        /// <summary>
        /// Section <see cref="Filter">filter</see> results for TopicCodes
        /// </summary>
        public IEnumerable<Filter> TopicCodes { get; set; }
        /// <summary>
        /// Section <see cref="Filter">filter</see> results for Terms
        /// </summary>
        public IEnumerable<Filter> Terms { get; set; }

        /// <summary>
        /// Start time from midnight (in minutes) of earliest section in result set.  9:45am == 585.
        /// </summary>
        public int EarliestTime { get; set; }

        /// <summary>
        /// End time from midnight (in minutes) of latest section in results set. 9:45am == 585.
        /// </summary>
        public int LatestTime { get; set; }

        /// <summary>
        /// Section <see cref="Filter">filter</see> result for OnlineCategory filtering. Possible filter values:
        ///    Online: Sections that have only online instruction
        ///    NotOnline: Sections that have no online instruction
        ///    Hybrid: Sections that have some instruction that is online and some instruction that is not online
        /// </summary>
        public IEnumerable<Filter> OnlineCategories { get; set; }
        /// <summary>
        /// This will control to filter only sections that are open.
        /// open secctions have seats available.
        /// </summary>
        public Filter OpenSections { get; set; }

        /// <summary>
        /// This will control to filter sections that are open and waitlisted.
        /// open secctions have seats available.
        /// </summary>
        public Filter OpenAndWaitlistSections { get; set; }
    }
}
