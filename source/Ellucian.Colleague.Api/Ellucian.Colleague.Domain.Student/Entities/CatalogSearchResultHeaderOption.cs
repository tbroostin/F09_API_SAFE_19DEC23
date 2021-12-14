// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This object defines display options for course catalog search result headers.
    /// </summary>
    [Serializable]
    public class CatalogSearchResultHeaderOption
    {
        /// <summary>
        /// Indicates whether the header should be hidden on display
        /// </summary>
        public bool IsHidden { get; set; }
        /// <summary>
        /// Indicates the type of header so it can be properly handled in the course catalog
        /// </summary>
        public CatalogSearchResultHeaderType Type { get; set; }

        public CatalogSearchResultHeaderOption(CatalogSearchResultHeaderType type, bool isHidden = false)
        {
            Type = type;
            IsHidden = isHidden;
        }

        /// <summary>
        /// Equality for a catalog search result header option is simply the type. Two items of the same type are considered a match.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is CatalogSearchResultHeaderOption))
            {
                return false;
            }
            return (obj as CatalogSearchResultHeaderOption).Type == this.Type;
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode();
        }
    }
}
