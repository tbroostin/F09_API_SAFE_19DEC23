/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class LoadPeriod
    {
        public string Id { get; private set; }
        public string Description { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public LoadPeriod(string id, string desc, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id cannot be null or empty");
            }
            if (string.IsNullOrEmpty(desc))
            {
                throw new ArgumentNullException("desc", "description cannot be null or empty");
            }
            if(!startDate.HasValue)
            {
                throw new ArgumentNullException("startDate", "start date cannot be null");
            }
            if(!endDate.HasValue)
            {
                throw new ArgumentNullException("endDate", "end date cannot be null");
            }

            Id = id;
            Description = desc;
            StartDate = startDate.Value;
            EndDate = endDate.Value;
        }
    }
}
