// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CipCode :GuidCodeItem
    {
        /// <summary>
        /// Revision Year from CIP code in Colleague
        /// </summary>
        public int? RevisionYear { get; private set; }

        /// <summary>
        /// Constructor for CipCode
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        /// <param name="revisionYear">Revision Year</param>
        public CipCode(string guid, string code, string description, int? revisionYear)
            : base(guid, code, description)
        {
            RevisionYear = revisionYear;
        }
    }
}
