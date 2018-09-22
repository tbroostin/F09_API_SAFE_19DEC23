// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class FinancialAidPayment : ReceivablePayment
    {
        private readonly int _year;
        private readonly string _awardId;
        private readonly int _priority;

        public int Year { get { return _year; } }

        public string AwardId { get { return _awardId; } }

        public int Priority { get { return _priority; } }

        public FinancialAidPayment(string id, string referenceNumber, string personId, string receivableType, string termId, DateTime date, decimal amount, 
            int year, string awardId, int priority)
            : base(id, referenceNumber, personId, receivableType, termId, date, amount)
        {
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }
            _year = year;
            _awardId = awardId;
            _priority = priority;
        }
    }
}
