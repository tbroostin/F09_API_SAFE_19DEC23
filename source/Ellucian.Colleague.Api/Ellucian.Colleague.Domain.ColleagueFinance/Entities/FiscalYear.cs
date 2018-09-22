// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    [Serializable]
    public class FiscalYear
    {

        private string _guid;
        private string _id;
      
        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new InvalidOperationException("FiscalYear Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// GUID for the FiscalYear; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _guid; }
            set
            {
                if (string.IsNullOrEmpty(_guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("FiscalYear Guid cannot be changed.");
                }
            }
        }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Host Organization ID 
        /// </summary>
        public string HostCorpId { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Fiscal Start Month
        /// </summary>
        public int? FiscalStartMonth { get; set; }

        /// <summary>
        /// Institution Name
        /// </summary>
        public string InstitutionName { get; set; }
        public int? CurrentFiscalYear { get; set; }

        public FiscalYear(string guid, string id)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Concat("Guid can not be empty, Entity: ‘GEN.LDGR', Record ID: ", id));
            }
            _guid = guid;
            _id = id;
        }
    }
}