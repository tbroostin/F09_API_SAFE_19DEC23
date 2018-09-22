// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// A Student Finance Link
    /// </summary>
    [Serializable]
    public class StudentFinanceLink
    {
        private string _title;
        private string _url;

        /// <summary>
        /// Title displayed to users
        /// </summary>
        public string Title { get { return _title; } }

        /// <summary>
        /// URL to which users are taken when clicking on the link
        /// </summary>
        public string Url { get { return _url; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="StudentFinanceLink"/> class
        /// </summary>
        /// <param name="title">Title displayed to users</param>
        /// <param name="url">URL to which users are taken when clicking on the link</param>
        public StudentFinanceLink(string title, string url)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "The link must have a title.");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url", "The link must have a URL.");
            }

            _title = title;
            _url = url;
        }
    }
}
