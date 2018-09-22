// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    [Serializable]
    public class FiscalPeriodsIntg
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
                    throw new InvalidOperationException("FiscalPeriodsIntg Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// GUID for the FiscalPeriodsIntg; not required, but cannot be changed once assigned.
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
                    throw new InvalidOperationException("FiscalPeriodsIntg Guid cannot be changed.");
                }
            }
        }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
      
        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }
     
        /// <summary>
        /// Fiscal Year 
        /// </summary>    
        public int? FiscalYear { get; set; }

        /// <summary>
        /// Month 
        /// </summary> 
        public int? Month { get; set; }
        public int Year { get; set; }

        public FiscalPeriodsIntg(string guid, string id)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Concat("Guid can not be empty, Entity: ‘FISCAL.PERIODS.INTG', Record ID: ", id));
            }
            _guid = guid;
            _id = id;
        }
    }
}