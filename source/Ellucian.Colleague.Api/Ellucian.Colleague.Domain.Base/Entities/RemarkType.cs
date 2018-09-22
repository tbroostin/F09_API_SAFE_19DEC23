﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Remark Type
    /// </summary>
    [Serializable]
    public class RemarkType : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemarkType"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the RemarkType</param>
        /// <param name="description">Description or Title of the RemarkType</param>
        public RemarkType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}