// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Ethnicities
    /// </summary>
    [Serializable]
    public class Ethnicity : GuidCodeItem
    {
        private EthnicityType? _type;
        /// <summary>
        /// Ethnicity Type for the ethnicity
        /// </summary>
        public EthnicityType? Type { get { return _type; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ethnicity"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The ethnicity type</param>
        public Ethnicity(string guid, string code, string description, EthnicityType? type)
            : base(guid, code, description)
        {
            _type = type;
        }
    }
}
