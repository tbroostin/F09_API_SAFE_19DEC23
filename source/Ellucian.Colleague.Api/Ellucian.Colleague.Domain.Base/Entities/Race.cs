// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Races
    /// </summary>
    [Serializable]
    public class Race : GuidCodeItem
    {
        private RaceType? _type;
        /// <summary>
        /// Race Type for the race
        /// </summary>
        public RaceType? Type { get { return _type; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Race"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The race type</param>
        public Race(string guid, string code, string description, RaceType? type)
            : base(guid, code, description)
        {
            _type = type;
        }
    }
}
