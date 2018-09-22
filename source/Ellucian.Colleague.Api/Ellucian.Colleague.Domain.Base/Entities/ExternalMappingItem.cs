// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class ExternalMappingItem
    {
        private string _originalCode;

        /// <summary>
        /// Original Code prior to mapping
        /// </summary>
        public string OriginalCode { get { return _originalCode; }}

        /// <summary>
        /// Code to which original code is mapped
        /// </summary>
        public string NewCode { get; set; }

        /// <summary>
        /// Special processing code 1 used in ERP processing logic
        /// </summary>
        public string ActionCode1 { get; set; }

        /// <summary>
        /// Special processing code 2 used in ERP processing logic
        /// </summary>
        public string ActionCode2 { get; set; }

        /// <summary>
        /// Special processing code 1 used in ERP processing logic
        /// </summary>
        public string ActionCode3 { get; set; }

        /// <summary>
        /// Special processing code 2 used in ERP processing logic
        /// </summary>
        public string ActionCode4 { get; set; }

        /// <summary>
        /// Constructor for the external mapping item
        /// </summary>
        /// <param name="originalCode">Original Code prior to mapping</param>
        public ExternalMappingItem(string originalCode)
        {
            if (string.IsNullOrEmpty(originalCode))
            {
                throw new ArgumentNullException("originalCode", "An original Code must be specified.");
            }

            _originalCode = originalCode;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ExternalMappingItem other = obj as ExternalMappingItem;
            if (other == null)
            {
                return false;
            }

            return other.OriginalCode.Equals(OriginalCode);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return OriginalCode.GetHashCode();
        }
    }
}
