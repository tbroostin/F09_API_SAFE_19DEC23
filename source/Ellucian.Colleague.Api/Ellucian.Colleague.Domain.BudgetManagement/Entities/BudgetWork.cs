// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// This class expose the transactional activities stored within BUD.WORK
    /// </summary>
    [Serializable]
    public class BudgetWork
    {
        public string RecordGuid { get; private set; }
        public string RecordKey { get; private set; }
        public string Description { get; private set; }
        public string AccountingString { get; private set; }
        public string HostCountry { get; set; }
        public string ProjectId { get; set; }
        public string BudgetPhase { get; set; }
        public string AccountingStringComponentValue { get; set; }
        public decimal? LineAmount { get; set; }
        public List<string> Comments { get; set; }

        public BudgetWork(string guid, string id)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Format("Guid is required. Id: {0}", id));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Format("Accounting string is required. Guid: {0}", guid));
            }          
            RecordGuid = guid;
            RecordKey = id;
        }
    }
}