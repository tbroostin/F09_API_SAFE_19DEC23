// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an Social Media Type
    /// </summary>
    [Serializable]
    public enum SocialMediaTypeCategory
    {
        /// <summary>
        /// Windows Live
        /// </summary>
        windowsLive,
        /// <summary>
        /// Yahoo
        /// </summary>
        yahoo,
        /// <summary>
        /// Skype
        /// </summary>
        skype,
        /// <summary>
        /// QQ
        /// </summary>
        qq,
        /// <summary>
        /// Hangouts
        /// </summary>
        hangouts,
        /// <summary>
        /// ICQ
        /// </summary>
        icq,
        /// <summary>
        /// Jabber
        /// </summary>
        jabber,
        /// <summary>
        /// Facebook
        /// </summary>
        facebook,
        /// <summary>
        /// Twitter
        /// </summary>
        twitter,
        /// <summary>
        /// Instagram
        /// </summary>
        instagram,
        /// <summary>
        /// Tumblr
        /// </summary>
        tumblr,
        /// <summary>
        /// Pinterest
        /// </summary>
        pinterest,
        /// <summary>
        /// LinkedIn
        /// </summary>
        linkedin,
        /// <summary>
        /// FourSquare
        /// </summary>
        foursquare,
        /// <summary>
        /// Youtube
        /// </summary>
        youtube,
        /// <summary>
        /// Blog
        /// </summary>
        blog,
        /// <summary>
        /// Website
        /// </summary>
        website,
        /// <summary>
        /// Other
        /// </summary>
        other
    }
}