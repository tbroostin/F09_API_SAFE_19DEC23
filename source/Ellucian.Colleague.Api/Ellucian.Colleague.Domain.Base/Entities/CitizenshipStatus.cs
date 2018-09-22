﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// CitizenshipStatus
    /// </summary>
    [Serializable]
    public class CitizenshipStatus : GuidCodeItem
    {

        private CitizenshipStatusType _citizenshipStatusType;
        public CitizenshipStatusType CitizenshipStatusType { get { return _citizenshipStatusType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CitizenshipStatus"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the CitizenshipStatus</param>
        /// <param name="description">Description or Title of the CitizenshipStatus</param>
        /// <param name="citizenshipStatusType">Citizenship Status Category of the CitizenshipStatus</param>
        public CitizenshipStatus(string guid, string code, string description, CitizenshipStatusType citizenshipStatusType)
            : base (guid, code, description)
        {
            _citizenshipStatusType = citizenshipStatusType;
        }
    }
}
