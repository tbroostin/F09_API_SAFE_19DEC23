// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how transcript requests or enrollment requests should be rendered in self-service.
    /// </summary>
    [Serializable]
    public class CourseCatalogConfiguration
    {
        /// <summary>
        /// If provided, this limits catalog searches by date to dates no earlier than this.
        /// </summary>
        public DateTime? EarliestSearchDate
        {
            get
            {
                return earliestSearchDate;
            }
            set
            {
                if (value != null && latestSearchDate != null && value > latestSearchDate)
                {
                    throw new ArgumentException("EarliestSearchDate cannot be later than LatestSearchDate");
                }
                earliestSearchDate = value;
            }
        }
        private DateTime? earliestSearchDate;

        /// <summary>
        ///If provided, this limits catalog searches by date to dates no later than this.
        /// </summary>
        public DateTime? LatestSearchDate
        {
            get
            {
                return latestSearchDate;
            }
            set
            {
                if (value != null && earliestSearchDate != null && value < earliestSearchDate)
                {
                    throw new ArgumentException("LastestSearchDate cannot be earlier than EarliestSearchDate");
                }
                latestSearchDate = value;
            }
        }
        private DateTime? latestSearchDate;

        /// <summary>
        /// The parameters controlling how to display filters and criteria on the course catalog
        /// </summary>
        public ReadOnlyCollection<CatalogFilterOption> CatalogFilterOptions { get; private set; }
        private readonly List<CatalogFilterOption> _catalogFilterOptions = new List<CatalogFilterOption>();

        /// <summary>
        /// The parameters controls how to display headers on the course catalog search result.
        /// </summary>
        public ReadOnlyCollection<CatalogSearchResultHeaderOption> CatalogSearchResultHeaderOptions { get; private set; }
        private readonly List<CatalogSearchResultHeaderOption> _catalogSearchResultHeaderOptions = new List<CatalogSearchResultHeaderOption>();

        /// <summary>
        /// Indicates whether or not course section fee information should be visible in the course catalog
        /// </summary>
        public bool ShowCourseSectionFeeInformation { get; set; }

        /// <summary>
        /// Indicates whether or not course section book information should be visible in the course catalog's section detail.
        /// </summary>
        public bool ShowCourseSectionBookInformation { get; set; }

        /// <summary>
        /// Default Colleague Self-Service search view for Course Catalogs
        /// </summary>
        public SelfServiceCourseCatalogSearchView DefaultSelfServiceCourseCatalogSearchView { get; set; }

        /// <summary>
        /// Default Colleague Self-Service search result view for Course Catalogs
        /// </summary>
        public SelfServiceCourseCatalogSearchResultView DefaultSelfServiceCourseCatalogSearchResultView { get; set; }

        /// <summary>
        /// Indicates whether or not section availablity (Avaliable Seats, Capacity, and Waitlist counts) will use cached section data for the section listing.
        /// Blank or No values will default to false
        /// </summary>
        public bool BypassApiCacheForAvailablityData { get; set; }

        /// <summary>
        /// Constructor for CourseCatalogConfiguration
        /// </summary>
        public CourseCatalogConfiguration(DateTime? earliestSearchDate, DateTime? latestSearchDate)
        {
            if (earliestSearchDate.HasValue && latestSearchDate.HasValue && earliestSearchDate > latestSearchDate)
            {
                throw new ArgumentException("earliestSearchDate cannot be later than latestSearchDate");
            }

            this.LatestSearchDate = latestSearchDate;
            this.EarliestSearchDate = earliestSearchDate;
            CatalogFilterOptions = _catalogFilterOptions.AsReadOnly();
            CatalogSearchResultHeaderOptions = _catalogSearchResultHeaderOptions.AsReadOnly();
            BypassApiCacheForAvailablityData = false;
        }

        public void AddCatalogFilterOption(CatalogFilterType type, bool isHidden)
        {
            // Can only have one filter option in the list with a specific type
            if (!CatalogFilterOptions.Any(a => a.Type.Equals(type)))
            {
                CatalogFilterOption newOption = new CatalogFilterOption(type, isHidden);
                _catalogFilterOptions.Add(newOption);
            }
        }

        //Add Catalog Search Result Header Options
        public void AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType type, bool isHidden)
        {
            // Can only have one filter option in the list with a specific type
            if (!CatalogSearchResultHeaderOptions.Any(a => a.Type.Equals(type)))
            {
                CatalogSearchResultHeaderOption newOption = new CatalogSearchResultHeaderOption(type, isHidden);
                _catalogSearchResultHeaderOptions.Add(newOption);
            }
        }

    }
}
