// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class RegistrationBillingItem
    {
        private readonly string _Id;
        private readonly string _AcademicCreditId;

        public string Id { get { return _Id; } }
        public string AcademicCreditId { get { return _AcademicCreditId; } }

        public RegistrationBillingItem(string id, string acadCreditId)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID cannot be null");
            }

            if (String.IsNullOrEmpty(acadCreditId))
            {
                throw new ArgumentNullException("acadCreditId", "Academic credit ID cannot be null");
            }

            _Id = id;
            _AcademicCreditId = acadCreditId;
        }
    }
}
