/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class BenefitDeductionType
    {
        /// <summary>
        /// Id of the benefit deduction
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Description of the benefit deduction
        /// </summary>
        public string Description
        {
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
        /// Self-serivce description of the benefit deduction
        /// </summary>
        public string SelfServiceDescription { get { return selfServiceDescription; } }
        private string selfServiceDescription;

        /// <summary>
        /// Institution type of Benefit or Deduction
        /// </summary>
        public BenefitDeductionTypeCategory Category { get { return category; } }
        private BenefitDeductionTypeCategory category;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="category"></param>
        public BenefitDeductionType(string id, string description, string selfServiceDescription, BenefitDeductionTypeCategory category)
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
            this.selfServiceDescription = selfServiceDescription;
            this.category = category;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var earningsType = obj as BenefitDeductionType;
            return earningsType.Id == this.Id;
        }

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
