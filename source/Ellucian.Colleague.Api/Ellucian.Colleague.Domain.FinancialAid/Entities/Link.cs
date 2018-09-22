//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Links are web links to help the student get to resources outside of Colleague
    /// </summary>
    [Serializable]
    public class Link
    {
        private readonly string _Title;
        private LinkTypes _Type;
        private readonly string _LinkUrl;

        /// <summary>
        /// Title to display for the link
        /// </summary>
        public string Title { get { return _Title; } }

        /// <summary>
        /// Web URL for this link
        /// </summary>
        public string LinkUrl { get { return _LinkUrl; } }

        /// <summary>
        /// Type of Link; possibly this should be an enumeration
        /// </summary>
        public LinkTypes LinkType { get { return _Type; } set { _Type = value; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Title">Title to use when displaying the link</param>
        /// <param name="LinkType">Type of Link</param>
        /// <param name="LinkUrl">Web URL of the link</param>
        public Link(string Title, LinkTypes LinkType, string LinkUrl)
        {
            if (string.IsNullOrEmpty(Title))
            {
                throw new ArgumentNullException("Title");
            }

            if (string.IsNullOrEmpty(LinkUrl))
            {
                throw new ArgumentNullException("Url");
            }

            _Title = Title;
            _Type = LinkType;
            _LinkUrl = LinkUrl;

        }
    }
}
