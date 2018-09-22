/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class PayCycle
    {
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// PayCycles in a year
        /// </summary>
        public int AnnualPayFrequency { get; set; }

        /// <summary>
        /// The day of the week on which the pay cycle starts
        /// </summary>
        public DayOfWeek WorkWeekStartDay { get; set; }

        /// <summary>
        /// The pay cycle description
        /// </summary>
        public string Description {
            get { return description; } 
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                description = value;

            }
        }           
        private string description;
        
        /// <summary>
        /// List of pay classes associated with the pay cycle
        /// </summary>
        public List<string> PayClassIds { get; set; }
        
        /// <summary>
        /// List of pay period date ranges associated with the pay cycle
        /// </summary>
        public List<PayPeriod> PayPeriods { get; set; }

        /// <summary>
        /// PayCycle constructor
        /// </summary>
        /// <param name="id"></param>
        public PayCycle(string id, string description, DayOfWeek startDay)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("description");
            }

            this.id = id;
            this.description = description;
            this.WorkWeekStartDay = startDay;
            this.PayClassIds = new List<string>();
            this.PayPeriods = new List<PayPeriod>();
        }

        /// <summary>
        /// Two pay cycles are equal when their ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var payCycle = obj as PayCycle;
            return payCycle.Id == this.Id;
        }

        /// <summary>
        /// Hashcode representation of PayCycle (Id)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Description + "-" + Id;
        }
    }
}
