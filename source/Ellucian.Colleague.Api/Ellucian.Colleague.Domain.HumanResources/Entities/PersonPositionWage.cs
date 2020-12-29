/* Copyright 2016-2019 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// A PersonPositionWage object describes the parameters used to determine how a 
    /// person is paid for their particular position.
    /// </summary>
    [Serializable]
    public class PersonPositionWage
    {
        /// <summary>
        /// The DB Id of the object
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The Colleague PersonId of the object
        /// </summary>
        public string PersonId { get { return personId; } }
        private readonly string personId;

        /// <summary>
        /// The PositionId of the object
        /// </summary>
        public string PositionId { get { return positionId; } }
        private readonly string positionId;

        /// <summary>
        /// The Id of the PersonPosition this wage object is attached to
        /// </summary>
        public string PersonPositionId { get { return personPositionId; } }
        private readonly string personPositionId;

        /// <summary>
        /// The Id of the PositionPayDefault object that contains the default parameters used to build
        /// this object
        /// </summary>
        public string PositionPayDefaultId { get { return positionPayDefaultId; } }
        private readonly string positionPayDefaultId;

        /// <summary>
        /// The Id of the PayClass which describes when and how the employee is paid for the work done in the position.
        /// PayCycles contain a list of PayClasses.
        /// </summary>
        public string PayClassId { get { return payClassId; } }
        private string payClassId;

        /// <summary>
        /// The Id of the Paycycle, which is the effective PayCycle based on the PayClassId, of this object
        /// </summary>
        public string PayCycleId { get { return payCycleId; } }
        private string payCycleId;


        /// <summary>
        /// The Id of the EarningsType the person tracks time against for regular work for this position
        /// </summary>
        public string RegularWorkEarningsTypeId { get { return regularWorkEarningsTypeId; } }
        private string regularWorkEarningsTypeId;

        /// <summary>
        /// The start date of when these wage parameters take effect
        /// </summary>
        public DateTime StartDate 
        { 
            get { return startDate; }
            set
            {
                if (EndDate.HasValue && EndDate.Value < value)
                {
                    throw new ArgumentOutOfRangeException("value", "Start Date must be before End Date");
                }
                startDate = value;
            }
        }
        private DateTime startDate;

        /// <summary>
        /// The end date of when these wage parameters stop taking effect
        /// </summary>
        public DateTime? EndDate 
        { 
            get { return endDate; }
            set
            {
                if (value.HasValue && value.Value < StartDate)
                {
                    throw new ArgumentOutOfRangeException("value", "End Date must be after Start Date");
                }
                endDate = value;
            }
        }
        private DateTime? endDate;

        /// <summary>
        /// Indicates whether this person's pay is suspended for this position. Defaults to false
        /// </summary>
        public bool IsPaySuspended { get; set; }

        /// <summary>
        /// List of PositionFundingSources
        /// </summary>
        public List<PositionFundingSource> FundingSources { get; set; }

        /// <summary>
        /// The ID of the earnings type group associated to the position pay record for this employee's wage
        /// </summary>
        public string EarningsTypeGroupId { get; private set; }


        /// <summary>
        /// Build a PersonPositionWage object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personId"></param>
        /// <param name="positionId"></param>
        /// <param name="personPositionId"></param>
        /// <param name="positionPayDefaultId"></param>
        /// <param name="payClassId"></param>
        /// <param name="payCycleId"></param>
        /// <param name="regularWorkEarningsTypeId"></param>
        /// <param name="startDate"></param>
        /// <param name="earningsTypeGroupId"></param>
        public PersonPositionWage(
            string id,
            string personId,
            string positionId,
            string personPositionId,
            string positionPayDefaultId,
            string payClassId,
            string payCycleId,
            string regularWorkEarningsTypeId,
            DateTime startDate,
            string earningsTypeGroupId)            
        {
            if (string.IsNullOrEmpty(id)) 
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(personId)) 
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId");
            }
            if (string.IsNullOrEmpty(personPositionId)) 
            {
                throw new ArgumentNullException("personPositionId");
            }
            if (string.IsNullOrEmpty(positionPayDefaultId)) 
            {
                throw new ArgumentNullException("positionPayDefaultId");
            }          
            if (string.IsNullOrEmpty(payClassId)) 
            {
                throw new ArgumentNullException("payClassId");
            }
            if (string.IsNullOrEmpty(payCycleId)) 
            {
                throw new ArgumentNullException("payCycleId");
            }
            if (string.IsNullOrEmpty(regularWorkEarningsTypeId)) 
            {
                throw new ArgumentNullException("regularWorkEarningsTypeId");
            }

            this.id = id;
            this.personId = personId;
            this.positionId = positionId;
            this.personPositionId = personPositionId;
            this.positionPayDefaultId = positionPayDefaultId;
            this.payClassId = payClassId;
            this.payCycleId = payCycleId;
            this.regularWorkEarningsTypeId = regularWorkEarningsTypeId;
            this.startDate = startDate;
            this.IsPaySuspended = false;
            this.EarningsTypeGroupId = earningsTypeGroupId;
            
            FundingSources = new List<PositionFundingSource>();
        }

        /// <summary>
        /// Two PersonPositionWage objects are equal when their Ids are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var personPositionWage = obj as PersonPositionWage;

            return personPositionWage.Id == this.Id;
        }

        /// <summary>
        /// Hashcode based on the Id of this object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// String representation based on the Id and PositionId of the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}*{1}", Id, PositionId);
        }
    }
}
