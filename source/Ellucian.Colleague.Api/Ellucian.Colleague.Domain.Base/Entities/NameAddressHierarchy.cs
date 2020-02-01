// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Contains information defining which of a person's names and/or addresses should be retrieved
    /// </summary>
    [Serializable]
    public class NameAddressHierarchy
    {
        /// <summary>
        /// Code for the hierarchy
        /// </summary>
        private string _Code;
        public string Code { get { return _Code; } }

        /// <summary>
        /// List of name types defining the order of prcedence in determining the name to use and the format
        /// </summary>
        private readonly List<string> _NameTypeHierarchy = new List<string>();
        public ReadOnlyCollection<string> NameTypeHierarchy { get; private set; }

        public NameAddressHierarchy(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "Must have a code for a name address hierarchy");
            }
            _Code = code;
            NameTypeHierarchy = _NameTypeHierarchy.AsReadOnly();
            AddressTypeHierarchy = _AddressTypeHierarchy.AsReadOnly();
        }

        /// <summary>
        /// Add a name type code to the existing list.
        /// </summary>
        public void AddNameTypeHierarchy(string nameType)
        {
            if (string.IsNullOrEmpty(nameType))
            {
                throw new ArgumentNullException("nameType");
            }
            if (!_NameTypeHierarchy.Contains(nameType))
            {
                _NameTypeHierarchy.Add(nameType);
            }
        }

        /// <summary>
        /// List of address types defining the order of precedence 
        /// </summary>
        private readonly List<string> _AddressTypeHierarchy = new List<string>();
        public ReadOnlyCollection<string> AddressTypeHierarchy { get; private set; }

        /// <summary>
        /// Add a address type code to the existing list.
        /// </summary>
        public void AddAddressTypeHierarchy(string addressType)
        {
            if (string.IsNullOrEmpty(addressType))
            {
                throw new ArgumentNullException("addressType");
            }
            if (!_AddressTypeHierarchy.Contains(addressType))
            {
                _AddressTypeHierarchy.Add(addressType);
            }
        }
    }
}
