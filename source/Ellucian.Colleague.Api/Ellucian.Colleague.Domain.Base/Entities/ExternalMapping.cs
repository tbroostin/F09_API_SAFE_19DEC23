// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class ExternalMapping : CodeItem
    {
        private readonly List<ExternalMappingItem> _items = new List<ExternalMappingItem>();

        /// <summary>
        /// ERP field against which original code values are validated
        /// </summary>
        public string OriginalCodeValidationField { get; set; }

        /// <summary>
        /// ERP field against which new code values are validated
        /// </summary>
        public string NewCodeValidationField { get; set; }

        /// <summary>
        /// Collection of mapping items on this mapping
        /// </summary>
        public ReadOnlyCollection<ExternalMappingItem> Items { get; private set; }

        /// <summary>
        /// Constructor for the external mapping
        /// </summary>
        /// <param name="code">Code of the external mapping</param>
        /// <param name="description">Description of the external mapping</param>
        public ExternalMapping(string code, string description)
            : base(code, description)
        {
            Items = _items.AsReadOnly();
            // no additional work to do
        }

        /// <summary>
        /// Add a mapping item to the mapping
        /// </summary>
        /// <param name="item">Mapping item</param>
        public void AddItem(ExternalMappingItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "A mapping item must be specified.");
            }

            // Prevent duplicates
            if (!Items.Contains(item))
            {
                _items.Add(item);
            }
        }

        /// <summary>
        /// Get the mapping for an external code value
        /// </summary>
        /// <param name="externalCode">External Code</param>
        /// <returns>Mapping for the external code</returns>
        public ExternalMappingItem GetInternalCodeMapping(string externalCode)
        {
            ExternalMappingItem item = null;
            if (string.IsNullOrEmpty(externalCode))
            {
                throw new ArgumentNullException("externalCode", "An external code value must be specified.");
            }
            try
            {
                item = _items.First(i => i.OriginalCode == externalCode);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("No internal code mapping found for external code " + externalCode);
            }
            return item;
        }

        /// <summary>
        /// Get the mappings for an internal code value
        /// </summary>
        /// <param name="internalCode">Internal Code</param>
        /// <returns>Mappings for the internal code</returns>
        public IEnumerable<ExternalMappingItem> GetExternalCodeMappings(string internalCode)
        {
            List<ExternalMappingItem> items = new List<ExternalMappingItem>();
            if (string.IsNullOrEmpty(internalCode))
            {
                throw new ArgumentNullException("internalCode", "An internal code value must be specified.");
            }
            items.AddRange(_items.Where(i => i.NewCode == internalCode));

            if (items.Count == 0)
            {
                throw new InvalidOperationException("No external code mappings found for internal code " + internalCode);
            }
            return items;
        }
    }
}
