// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
        /// Indicates whether or not course section fee information should be visible in the course catalog
        /// </summary>
        public bool ShowCourseSectionFeeInformation { get; set; }

        /// <summary>
        /// Indicates whether or not course section book information should be visible in the course catalog's section detail.
        /// </summary>
        public bool ShowCourseSectionBookInformation { get; set; }

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


    }
}
