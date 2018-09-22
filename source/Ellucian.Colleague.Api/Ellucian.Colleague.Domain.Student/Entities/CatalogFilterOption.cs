// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This object defines display options for course catalog filters and criteria.
    /// </summary>
    [Serializable]
    public class CatalogFilterOption
    {
        /// <summary>
        /// Indicates whether the filter should be hidden on display
        /// </summary>
        public bool IsHidden { get; set; }
        /// <summary>
        /// Indicates the type of filter so it can be properly handled in the course catalog
        /// </summary>
        public CatalogFilterType Type { get; set; }

        public CatalogFilterOption(CatalogFilterType type, bool isHidden = false)
        {
            Type = type;
            IsHidden = isHidden;
        }

        /// <summary>
        /// Equality for a catalog filter option is simply the type. Two items of the same type are considered a match.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is CatalogFilterOption))
            {
                return false;
            }
            return (obj as CatalogFilterOption).Type == this.Type;
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode();
        }
    }
}
