// Copyright 2018 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A catalog listing of the courses offered by an organization.
    /// </summary>
    public class Catalog
    {
        /// <summary>
        /// Catalog year
        /// </summary>
        public string CatalogYear { get; set; }

        /// <summary>
        /// Flag indicating if this catalog year should be hidden in What If
        /// </summary>
        public bool HideInWhatIf { get; set; }

        /// <summary>
        /// Catalog start date
        /// </summary>
        public string CatalogStartDate { get; set; }
    }
}
