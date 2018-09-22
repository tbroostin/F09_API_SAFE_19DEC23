using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PayableDepositDirectiveCollection : IEnumerable<PayableDepositDirective>
    {
        public string PayeeId { get; private set; }


        private List<PayableDepositDirective> depositDirectives;

        public PayableDepositDirectiveCollection(string payeeId)
        {
            if (string.IsNullOrEmpty(payeeId))
            {
                throw new ArgumentNullException("payeeId");
            }

            PayeeId = payeeId;
            depositDirectives = new List<PayableDepositDirective>();
        }

        public bool CanAdd(PayableDepositDirective depositDirective)
        {
            return depositDirective != null &&
                !Contains(depositDirective) &&
                depositDirective.PayeeId == this.PayeeId;
        }

        public bool Contains(PayableDepositDirective depositDirective)
        {
            return depositDirectives.Contains(depositDirective);
        }

        public void Add(PayableDepositDirective depositDirective)
        {
            if (!CanAdd(depositDirective))
            {
                throw new ArgumentException("depositDirective invalid for collection", "depositDirective");
            }

            depositDirectives.Add(depositDirective);
            adjustCollectionAfterAdd(depositDirective.AddressId);
        }

        public PayableDepositDirective this[int index]
        {
            get { return depositDirectives[index]; }            
        }

        public int Count
        {
            get
            {
                return depositDirectives.Count;
            }
        }

        
        public IEnumerator<PayableDepositDirective> GetEnumerator()
        {
            return depositDirectives.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return depositDirectives.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var payableDepositDirectiveCollection = obj as PayableDepositDirectiveCollection;

            if (payableDepositDirectiveCollection.PayeeId == this.PayeeId)
            {
                for (var i = 0; i < this.Count; i++)
                {
                    if (!this[i].Equals(payableDepositDirectiveCollection[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        private void adjustCollectionAfterAdd(string addressId) 
        {
            //select the directives with the same address as the added one.
            //then order them by start date.
            //if multiple directives have the same start date, secondary sort them by AddDateTime
            var addressSortedPayableDeposits = depositDirectives
                .Where(d => d.AddressId == addressId)
                .OrderBy(d => d.StartDate)
                .ThenBy(d => d.Timestamp.AddDateTime)
                .ToList();

            //reset payable deposit end dates
            for (int i = 0; i < addressSortedPayableDeposits.Count - 1; i++)
            {
                //if two deposits have same start date, end date of first deposit is the same as start date
                if (addressSortedPayableDeposits[i].StartDate == addressSortedPayableDeposits[i + 1].StartDate)
                {
                    addressSortedPayableDeposits[i].EndDate = addressSortedPayableDeposits[i].StartDate;
                }
                else
                {
                    //else set end date to the day before next start date
                    addressSortedPayableDeposits[i].EndDate = addressSortedPayableDeposits[i + 1].StartDate.AddDays(-1);
                }
            }
        }

    }
}
