// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Institution defined Restriction Styling configuration options
    /// </summary>
    [Serializable]
    public class RestrictionConfiguration
    {
        /// <summary>
        /// private storage for list of SeverityStyleMapping objects
        /// </summary>
        private readonly List<SeverityStyleMapping> _mapping = new List<SeverityStyleMapping>();

        /// <summary>
        /// Publicly exposed collection of SeverityStyleMapping objects that indicate what style to use for a given severity range.
        /// </summary>
        public ReadOnlyCollection<SeverityStyleMapping> Mapping { get; private set; }

        /// <summary>
        /// Adds an item to the list of Mappings.
        /// </summary>
        /// <param name="item">The item to add</param>
        public void AddItem(SeverityStyleMapping item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Item cannot be null");
            }
            
            if(_mapping.Any(i => item.SeverityEnd >= i.SeverityStart && item.SeverityEnd <= i.SeverityEnd))
            {
                throw new ArgumentOutOfRangeException("SeverityEnd", "SeverityEnd cannot fall within another severity range");
            }
            if(_mapping.Any(i => item.SeverityStart >= i.SeverityStart && item.SeverityStart <= i.SeverityEnd))
            {
                throw new ArgumentOutOfRangeException("SeverityStart", "SeverityStart cannot fall within another severity range");
            }
            _mapping.Add(item);
        }

        /// <summary>
        /// Removes an item from the list of Mappings.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if successful, false if not successful</returns>
        public bool RemoveItem(SeverityStyleMapping item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Item cannot be null");
            }
            
            if (_mapping.Any(i => i.SeverityStart == item.SeverityStart))
            {
                _mapping.RemoveAll(i => i.SeverityStart == item.SeverityStart);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Default constructor - links up the public Read Only property.
        /// </summary>
        public RestrictionConfiguration()
        {
            Mapping = _mapping.AsReadOnly();
        }
    }
}
