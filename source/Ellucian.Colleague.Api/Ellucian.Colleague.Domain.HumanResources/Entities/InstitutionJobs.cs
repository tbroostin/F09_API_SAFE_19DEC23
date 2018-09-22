/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// InstitutionJobs
    /// </summary>
    [Serializable]
    public class InstitutionJobs
    {
        /// <summary>
        /// The global identifier for the employee
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// The database ID of the PersonPosition
        /// The ID will be empty if this entity is a Non-Employee Position as noted by the NonEmployeePosition field
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        private readonly string id;

        /// <summary>
        /// The PERSON Id
        /// </summary>
        public string PersonId
        {
            get { return personId; }
        }

        private readonly string personId;

        /// <summary>
        /// The PositionId. <see cref="Position"/>
        /// </summary>
        public string PositionId
        {
            get { return positionId; }
        }

        private readonly string positionId;

        /// <summary>
        /// The Id of the person's supervisor for this position
        /// </summary>
        public string SupervisorId { get; set; }

        /// <summary>
        /// The Id of the person's alternate supervisor for this position
        /// </summary>
        public string AlternateSupervisorId { get; set; }

        /// <summary>
        /// The date this person begins in this position.
        /// </summary>
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                if (EndDate.HasValue && EndDate.Value < value)
                {
                    throw new ArgumentOutOfRangeException("Start Date cannot be after the EndDate");
                }
                startDate = value;
            }
        }

        private DateTime startDate;

        /// <summary>
        /// The date this person ends being in this position.
        /// </summary>
        public DateTime? EndDate
        {
            get { return endDate; }
            set
            {
                if (value.HasValue && value.Value < StartDate)
                {
                    throw new ArgumentOutOfRangeException("End Date cannot be before Start Date");
                }
                endDate = value;
            }
        }

        private DateTime? endDate;

        /// <summary>
        /// The operational unit of the institution to which the job belongs
        /// </summary>
        public string Employer { get; set; }


        /// <summary>
        /// The operational unit of the institution to which the job belongs
        /// </summary>
        public string EndReason { get; set; }

        /// <summary>
        /// The department of the institution to which the job belongs
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Classification
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// Funding source expense Gl account number
        /// </summary>
        public List<string> PpwgGlAccountNumber { get; set; }

        /// <summary>
        /// Funding source expense Gl projectids
        /// </summary>
        public List<string> PpwgProjectIds { get; set; }

        /// <summary>
        /// Benefits Status
        /// </summary>
        public BenefitsStatus? BenefitsStatus { get; set; }

        /// <summary>
        /// Pay Status 
        /// </summary>
        public PayStatus? PayStatus { get; set; }

        /// <summary>
        /// FTE
        /// </summary>
        public decimal? FullTimeEquivalent { get; set; }

        /// <summary>
        /// Grade
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// Step
        /// </summary>
        public string Step { get; set; }

        /// <summary>
        /// Pay Rate
        /// </summary>
        public string PayRate { get; set; }

        /// <summary>
        /// The amount of time that an employee in the position would  
        /// normally work and for which the employee would be paid during the pay  
        /// period, assuming no holidays, vacation, sick leave, etc
        /// </summary>
        public Decimal? CycleWorkTimeAmount { get; set; }

        /// <summary>
        /// The amount of time for which an employee in the position would   
        /// be paid during a year, including holidays, vacation, sick leave, etc.
        /// </summary>
        public Decimal? YearWorkTimeAmount { get; set; }

        /// <summary>
        /// Displays the description of the pay period work time unit as defined  
        ///  for the pay class entered in the Pay Class field; 
        ///  for example, "Hours"   or "Months."
        /// </summary>
        public string CycleWorkTimeUnits { get; set; }

        /// <summary>
        /// Displays the description of the yearly work time unit as defined   
        /// for the pay class entered in the Pay Class field; for example,  
        ///  "Hours" or "Months." 
        /// </summary>
        public string YearWorkTimeUnits { get; set; }

        /// <summary>
        /// Host Country
        /// </summary>
        public string HostCountry { get; set; }

        /// <summary>
        /// The preference for a job 
        /// </summary>
        public bool Primary { get; set; }

        /// <summary>
        /// Whether this is a salaried or hourly position. If true, the position is salaried
        /// </summary>
        public bool IsSalary { get; set; }

        /// <summary>
        /// Accounting Strings
        /// </summary>
        public List<string> AccountingStrings { get; set; }

        /// <summary>
        /// Currency Code
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Perposwg Items
        /// </summary>
        public List<PersonPositionWageItem> PerposwgItems { get; set; }
        
        /// <summary>
        /// Current position PayClass 
        /// </summary>
        public string PayClass { get; set; }

        /// <summary>
        /// Current position PayCycle
        /// </summary>
        public string PayCycle { get; set; }

        /// <summary>
        /// Create a InstitutionJobs object
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id">The Id of the PersonPosition</param>
        /// <param name="personId">The Colleague PERSON id of the person in this position</param>
        /// <param name="positionId">The Id of the Position assigned to this person</param>
        /// <param name="startDate">The date on which the person begins this position</param>
        public InstitutionJobs(string guid, string id, string personId, string positionId, DateTime startDate)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("id");
            }
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

            this.id = id;
            this.personId = personId;
            this.positionId = positionId;
            this.startDate = startDate;

            this.Guid = guid;
        }

        /// <summary>
        /// A InstitutionJobs 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="personId"></param>
        /// <param name="positionId"></param>
        public InstitutionJobs(string guid, string personId, string positionId)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId");
            }
            this.Guid = guid;
            this.personId = personId;
            this.positionId = positionId;

        }

        /// <summary>
        /// String representation of object (Id)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Id;
        }
    }
}