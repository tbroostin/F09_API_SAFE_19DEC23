﻿// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student Records Release Configuration Information
    /// </summary>
    [Serializable]
    public class StudentRecordsReleaseConfig
    {
        /// <summary>
        /// Student Records Release text
        /// </summary>
        public List<string> Text { get; set; }

        /// <summary>
        /// Is it mandatory to enter PIN in self service to create Student release record
        /// </summary>
        public bool IsPinRequired { get; set; }
    }
}
