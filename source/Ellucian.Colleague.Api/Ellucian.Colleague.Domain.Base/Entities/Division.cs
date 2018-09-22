// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Institutional Divisions within a school (institutional hierarchy)
    /// </summary>
    [Serializable]
    public class Division : GuidCodeItem
    {
        /// <summary>
        /// Return Division School Code
        /// </summary>
        public string SchoolCode { get; set; }

        /// <summary>
        /// Return Division InstitutionID
        /// </summary>
        public string InstitutionId { get; set; }

        public Division(string guid, string code, string description)
            : base(guid, code, description)
        {

        }
    }
}