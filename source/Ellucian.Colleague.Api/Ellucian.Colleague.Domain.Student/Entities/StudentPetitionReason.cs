﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Reason for a given student petition or faculty consent.
    /// </summary>
    [Serializable]
    public class StudentPetitionReason : CodeItem
    {
        /// <summary>
        /// Constructor for a waiver reason code
        /// </summary>
        /// <param name="code">Reason Code</param>
        /// <param name="description">Description for this code</param>
        public StudentPetitionReason(string code, string description)
            : base(code, description)
        {
        }
    }
}
