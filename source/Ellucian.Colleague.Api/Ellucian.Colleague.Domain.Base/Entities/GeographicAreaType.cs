// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// GeographicAreaType
    /// </summary>
    [Serializable]
    public class GeographicAreaType : GuidCodeItem
    {

        private GeographicAreaTypeCategory _geographicAreaTypeCategory;
        public GeographicAreaTypeCategory GeographicAreaTypeCategory { get { return _geographicAreaTypeCategory; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeographicAreaType"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the GeographicAreaType</param>
        /// <param name="description">Description or Title of the GeographicAreaType</param>
        /// <param name="geographicAreaTypeCategory">Geographic Area Type Category of Geographic Area Type</param>
        public GeographicAreaType(string guid, string code, string description, GeographicAreaTypeCategory geographicAreaTypeCategory)
            : base (guid, code, description)
        {
            _geographicAreaTypeCategory = geographicAreaTypeCategory;
        }
    }
}
