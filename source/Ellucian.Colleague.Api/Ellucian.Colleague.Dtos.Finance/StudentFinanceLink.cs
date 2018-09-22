// Copyright 2016 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A Student Finance Link
    /// </summary>
    public class StudentFinanceLink
    {
        /// <summary>
        /// Title displayed to users
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL to which users are taken when clicking on the link
        /// </summary>
        public string Url { get; set; }
    }
}
