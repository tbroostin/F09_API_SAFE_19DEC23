// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Contains INCOMING advisee search/filter request
    /// Search criteria must either contain an AdviseeKeyword or an AdvisorKeyword value of at least 2 characters. It cannot contain both.
    /// </summary>
    public class AdviseeSearchCriteria
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public AdviseeSearchCriteria()
        {

        }

        /// <summary>
        /// Used when requesting a search of advisees by name or Id. [last, first middle] or [first middle last] or [last] or [Id] expected.
        /// Must contain at least 2 characters when provided.
        /// </summary>
        public string AdviseeKeyword { get; set; }
        /// <summary>
        /// Used when requesting a search of advisees by advisor name or Id. [last, first middle] or [first middle last] or [last] or [Id] expected.
        /// Must contain at least 2 characters when provided.
        /// </summary>
        public string AdvisorKeyword { get; set; }
        /// <summary>
        /// Indicates that only the current, active advisees should be returned, excluding former advisees and advisees with future advisement start dates.
        /// </summary>
        public bool IncludeActiveAdviseesOnly { get; set; }

    }
}
