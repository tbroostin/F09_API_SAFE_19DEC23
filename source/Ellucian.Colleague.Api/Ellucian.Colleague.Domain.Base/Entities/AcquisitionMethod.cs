// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Acquisition Methods
    /// </summary>
    [Serializable]
    public class AcquisitionMethod : CodeItem
    {
        public string AcquisitionType { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCondition"/> class.
        /// </summary>
        /// <param name="guid">Unique Identifier.</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public AcquisitionMethod(string code, string description, string type = null) : base(code, description)
        {
            this.AcquisitionType = type;
        }
    }
}