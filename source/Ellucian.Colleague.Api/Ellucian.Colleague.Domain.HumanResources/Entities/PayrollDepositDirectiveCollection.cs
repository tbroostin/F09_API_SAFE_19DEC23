using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class PayrollDepositDirectiveCollection : IEnumerable<PayrollDepositDirective>
    {
        const int remainderPriority = 999;
        public string EmployeeId { get; private set; }
        private List<PayrollDepositDirective> PayrollDepositDirectives { get; set; } 

        /// <summary>
        /// Collection of PayrollDepositDirectives
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="payrolleDepositDirectives"></param>
        public PayrollDepositDirectiveCollection(string employeeId, IEnumerable<PayrollDepositDirective> payrolleDepositDirectives = null)
        {
            if(string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            EmployeeId = employeeId;
            
            PayrollDepositDirectives = new List<PayrollDepositDirective>();
            
            if(payrolleDepositDirectives != null && payrolleDepositDirectives.Any())
            {
                foreach(var directive in payrolleDepositDirectives)
                {
                    PayrollDepositDirectives.Add(directive);
                }
            }
        }

        /// <summary>
        /// Override function to return a PayrollDepositCollection
        /// </summary>
        /// <param name="lambda"></param>
        /// <returns>PayrollDepositCollection</returns>
        public PayrollDepositDirectiveCollection Where(Func<PayrollDepositDirective, bool> lambda)
        {
            return new PayrollDepositDirectiveCollection(EmployeeId, PayrollDepositDirectives.Where(lambda));
        }

        public void Add(PayrollDepositDirective payrollDepositDirective)
        {
            if (payrollDepositDirective == null || 
                this.Contains(payrollDepositDirective) || 
                payrollDepositDirective.PersonId != EmployeeId)                
            {
                throw new ArgumentException("depositDirective invalid for collection", "depositDirective");
            }

            //check for remainder overlaps
            if (payrollDepositDirective.Priority == remainderPriority)
            {
                var overlapRemainder = PayrollDepositDirectives.Where(d => d.Priority == remainderPriority).FirstOrDefault(existingDirective =>
                {
                    var existingEndDate = existingDirective.EndDate ?? DateTime.MaxValue;
                    var newDirectiveEndDate = payrollDepositDirective.EndDate ?? DateTime.MaxValue;

                    return payrollDepositDirective.StartDate <= existingEndDate && newDirectiveEndDate >= existingDirective.StartDate;
                });

                if (overlapRemainder != null)
                {
                    throw new ArgumentException(string.Format("depositDirective is invalid. cannot overlap with directive {0}", overlapRemainder.Id), "payrollDepositDirective");
                }
            }

            PayrollDepositDirectives.Add(payrollDepositDirective);
        }

        public PayrollDepositDirective this[int index]
        {
            get { return PayrollDepositDirectives[index]; }
        }

        public int Count
        {
            get
            {
                return PayrollDepositDirectives.Count;
            }
        }

        public IEnumerator<PayrollDepositDirective> GetEnumerator()
        {
            return PayrollDepositDirectives.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return PayrollDepositDirectives.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var payrollDepositDirectiveCollection = obj as PayrollDepositDirectiveCollection;

            if (payrollDepositDirectiveCollection.EmployeeId == this.EmployeeId)
            {
                for (var i = 0; i < this.Count; i++) 
                { 
                    if(!this[i].Equals(payrollDepositDirectiveCollection[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }
    }
}
