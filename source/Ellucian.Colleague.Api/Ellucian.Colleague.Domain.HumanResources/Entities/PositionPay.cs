/* Copyright 2016 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class PositionPay
    {
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id
        {
            get { return _id; }
        }

        private readonly string _id;

        /// <summary>
        /// To record the date the pay related position defaults became affective.
        /// </summary
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// To record the last date the pay related position defaults are affective.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The authorized date indicates when the decision was made  
        /// concerning the pay information
        /// </summary>
        public DateTime? AuthorizedDate { get; set; }

        /// <summary>
        /// This field identifies funding sources associated with the projects for a position.
        /// </summary>
        public List<string> PospayFndgSource { get; set; }

        /// <summary>
        /// CDD Name: POSPAY.FNDG.GL.NO
        /// </summary>
        public List<string> PospayFndgGlNo { get; set; }

        /// <summary>
        /// These are the GL numbers associated with the funding source for a project.
        /// </summary>
        public List<string> PospayPrjFndgGlNo { get; set; }

       
        /// <summary>
       /// This field contains the funding percentages for each GL number for each project    
        /// that is being funded for this position.
        /// </summary>
        public List<Decimal?> PospayFndgPct { get; set; }

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
        /// The minimum salary at which the position can  be filled.
        /// </summary>		
        public string SalaryMinimum { get; set; }

        /// <summary>
        /// The maximum salary at which the position can  be filled.
        /// </summary>		
        public string SalaryMaximum { get; set; }

        /// <summary>
        /// To display the name of the bargaining unit responsible for   
        /// the contract negotiation of this position
        /// </summary>	
        public string BargainingUnit { get; set; }


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
        /// HOST.COUNTRY from INTL 
        /// </summary>
        public string HostCountry { get; set; }


        public List<PositionFundingSource> FundingSource { get; set; }
    

        /// <summary>
        /// PosPay constructor
        /// </summary>
        /// <param name="id"></param>
        public PositionPay(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            this._id = id;

            this.PospayFndgGlNo = new List<string>();
            this.PospayPrjFndgGlNo = new List<string>();

        }

    }
}

/*
		/// <summary>
		/// CDD Name: POSPAY.PRJ.FNDG.SOURCE
		/// </summary>
		
		public List<string> PospayPrjFndgSource { get; set; }
		
		/// <summary>
		/// CDD Name: POSPAY.PRJ.FNDG.GL.NO
		/// </summary>
		
		public List<string> PospayPrjFndgGlNo { get; set; }
		
		/// <summary>
		/// CDD Name: POSPAY.PRJ.FNDG.PROJ.ID
		/// </summary>
		
		public List<string> PospayPrjFndgProjId { get; set; }
		
		/// <summary>
		/// CDD Name: POSPAY.PRJ.FNDG.PRJ.ITEM.ID
		/// </summary>
		
		public List<string> PospayPrjFndgPrjItemId { get; set; }
		
		/// <summary>
		/// CDD Name: POSPAY.PRJ.FNDG.PCT
		/// </summary>
		
		public List<Decimal?> PospayPrjFndgPct { get; set; }
		
		/// <summary>
		/// CDD Name: POSPAY.FNDG.PROJ.ID
		/// </summary>
		
		public List<string> PospayFndgProjId { get; set; }
		
		/// <summary>
		/// CDD Name: POSPAY.FNDG.PRJ.ITEM.ID
		/// </summary>
	
		public List<string> PospayFndgPrjItemId { get; set; }
        */

        /*
        /// <summary>
        /// PayCycles in a year
        /// </summary>
        public int AnnualPayFrequency { get; set; }

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
        public PayCycle(string id, string description)
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
         * */
