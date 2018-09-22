/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class EarningsType
    {
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The earnings type description
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
        /// Is this earnings type active or not?
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The category specifies whether earnings are regular, overtime, leave, college work study or miscellaneous
        /// </summary>
        public EarningsCategory Category { get { return category; } }
        private EarningsCategory category;

        /// <summary>
        /// The method specifies whether earnings are for leave accrued, leave taken, or time not paid. Can be null.
        /// </summary>
        public EarningsMethod Method { get { return method; } }
        private EarningsMethod method;

        /// <summary>
        /// The leave type indicates the type of leave (if any) that applies to this earnings type. Can be null, but if specified, the earnings category must be set to Leave.
        /// </summary>
        public LeaveType EarningsLeaveType { get { return earningsLeaveType; } }
        private LeaveType earningsLeaveType;

        /// <summary>
        /// An associated Leave Type Category
        /// </summary>
        public LeaveTypeCategory LeaveTypeCategory
        {
            get { if (earningsLeaveType == null) return LeaveTypeCategory.None; else return earningsLeaveType.TimeType; }
        }

        /// <summary>
        /// A multipler that can be applied to base earnings for overtime and compensatory earnings. The value must be between 0.0001 and 9.9999. The default factor is 1 (no earnings multiplier).
        /// </summary>
        public decimal Factor {
            get
            {
                return factor;
            }
            private set
            {
                if (value < 0.0001m || value > 9.9999m)
                {
                    throw new ArgumentOutOfRangeException(string.Format("{0} is invalid. The EarningsType Factor must be between 0.0001 and 9.9999, inclusive", value));
                }
                factor = value;
            }
        }
        private decimal factor;

        /// <summary>
        /// Instantiate an EarningsType object
        /// </summary>
        /// <param name="id">The Id of the earnings type.  Can be null or empty if creating a new earnings type.</param>
        /// <param name="description">The description of the earnings type.  Required.</param>
        /// <param name="isActive">True: earnings type is active; False: earnings type is inactive.</param>
        /// <param name="category">The earnings type category (Regular, Overtime, Leave, College Work Study, Miscellaneous)</param>
        /// <param name="method">The method in which leave type earnings are acrued or taken</param>
        /// <param name="earningsLeaveType">The category of leave for leave type earnings</param>
        /// <param name="factor">A multipler that can be applied to base earnings for overtime and compensatory earnings. The value must be between 0.0001 and 9.9999. The default factor is 1 if argument is null.</param>
        /// </summary>
        public EarningsType(string id, string description, bool isActive, EarningsCategory category, EarningsMethod method, decimal? factor, LeaveType earningsLeaveType = null)
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
            IsActive = isActive;
            this.category = category;
            this.method = method;
            this.earningsLeaveType = earningsLeaveType;
            Factor = factor ?? 1;
        }

        /// <summary>
        /// Two earnings types are equal when their ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals (object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var earningsType = obj as EarningsType;
            return earningsType.Id == Id;
        }
        
        /// <summary>
        /// Hascode representation of EarningsType (Id)
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
