// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Social Media Type
    /// </summary>
    [Serializable]
    public class SocialMediaType : GuidCodeItem
    {
        private SocialMediaTypeCategory _type;
        /// <summary>
        /// Social Media Type Category for the Social Media Type
        /// </summary>
        public SocialMediaTypeCategory Type { get { return _type; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The social media type category</param>
        public SocialMediaType(string guid, string code, string description, SocialMediaTypeCategory type)
            : base(guid, code, description)
        {
            _type = type;
        }
    }
}
