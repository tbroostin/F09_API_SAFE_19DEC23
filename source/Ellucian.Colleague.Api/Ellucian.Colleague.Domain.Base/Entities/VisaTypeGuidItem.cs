// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class VisaTypeGuidItem : GuidCodeItem
    {
        public VisaTypeCategory VisaTypeCategory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisaTypeGuidItem"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The visa type category</param>
        public VisaTypeGuidItem(string guid, string code, string description, VisaTypeCategory type)
            : base(guid, code, description)
        {
            VisaTypeCategory = type;
        }
    }
}
