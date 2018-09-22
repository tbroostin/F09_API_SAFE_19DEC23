//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:34:35 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: AR.PAY.PLANS
//     Application: ST
//     Environment: dvColl
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Ellucian.Dmi.Runtime;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.Finance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "ArPayPlans")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:34:35 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "AR.PAY.PLANS", EntityType = "PHYS")]
	public class ArPayPlans : IColleagueEntity
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		/// <summary>
		/// Record Key
		/// </summary>
		[DataMember]
		public string Recordkey { get; set; }
		
		public void setKey(string key)
		{
			Recordkey = key;
		}
		
		/// <summary>
		/// CDD Name: ARPL.PERSON.ID
		/// </summary>
		[DataMember(Order = 0, Name = "ARPL.PERSON.ID")]
		public string ArplPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.AR.TYPE
		/// </summary>
		[DataMember(Order = 1, Name = "ARPL.AR.TYPE")]
		public string ArplArType { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.AMT
		/// </summary>
		[DataMember(Order = 2, Name = "ARPL.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArplAmt { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.FREQUENCY
		/// </summary>
		[DataMember(Order = 3, Name = "ARPL.FREQUENCY")]
		public string ArplFrequency { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.NO.PAYMENTS
		/// </summary>
		[DataMember(Order = 4, Name = "ARPL.NO.PAYMENTS")]
		public int? ArplNoPayments { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.FIRST.DUE.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "ARPL.FIRST.DUE.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ArplFirstDueDate { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.GRACE.NO.DAYS
		/// </summary>
		[DataMember(Order = 6, Name = "ARPL.GRACE.NO.DAYS")]
		public int? ArplGraceNoDays { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.INVOICE.ITEMS
		/// </summary>
		[DataMember(Order = 7, Name = "ARPL.INVOICE.ITEMS")]
		public List<string> ArplInvoiceItems { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.PAY.PLAN.ITEMS
		/// </summary>
		[DataMember(Order = 8, Name = "ARPL.PAY.PLAN.ITEMS")]
		public List<string> ArplPayPlanItems { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.DOWN.PAY.PCT
		/// </summary>
		[DataMember(Order = 9, Name = "ARPL.DOWN.PAY.PCT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArplDownPayPct { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.CHARGE.AMT
		/// </summary>
		[DataMember(Order = 10, Name = "ARPL.CHARGE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArplChargeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.LATE.CHARGE.AMT
		/// </summary>
		[DataMember(Order = 11, Name = "ARPL.LATE.CHARGE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArplLateChargeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.TERM
		/// </summary>
		[DataMember(Order = 18, Name = "ARPL.TERM")]
		public string ArplTerm { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.CHARGE.PCT
		/// </summary>
		[DataMember(Order = 19, Name = "ARPL.CHARGE.PCT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArplChargePct { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.ORIG.AMT
		/// </summary>
		[DataMember(Order = 20, Name = "ARPL.ORIG.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArplOrigAmt { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.CANCEL.DATE
		/// </summary>
		[DataMember(Order = 21, Name = "ARPL.CANCEL.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? ArplCancelDate { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.LOCATION
		/// </summary>
		[DataMember(Order = 35, Name = "ARPL.LOCATION")]
		public string ArplLocation { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.STATUS
		/// </summary>
		[DataMember(Order = 36, Name = "ARPL.STATUS")]
		public List<string> ArplStatus { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.STATUS.DATE
		/// </summary>
		[DataMember(Order = 37, Name = "ARPL.STATUS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> ArplStatusDate { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.PLAN.LATE.CHRG.PCT
		/// </summary>
		[DataMember(Order = 38, Name = "ARPL.PLAN.LATE.CHRG.PCT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? ArplPlanLateChrgPct { get; set; }
		
		/// <summary>
		/// CDD Name: ARPL.PAY.PLAN.TEMPLATE
		/// </summary>
		[DataMember(Order = 39, Name = "ARPL.PAY.PLAN.TEMPLATE")]
		public string ArplPayPlanTemplate { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<ArPayPlansArplStatuses> ArplStatusesEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: ARPL.STATUSES
			
			ArplStatusesEntityAssociation = new List<ArPayPlansArplStatuses>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(ArplStatus != null)
			{
				int numArplStatuses = ArplStatus.Count;
				if (ArplStatusDate !=null && ArplStatusDate.Count > numArplStatuses) numArplStatuses = ArplStatusDate.Count;

				for (int i = 0; i < numArplStatuses; i++)
				{

					string value0 = "";
					if (ArplStatus != null && i < ArplStatus.Count)
					{
						value0 = ArplStatus[i];
					}


					DateTime? value1 = null;
					if (ArplStatusDate != null && i < ArplStatusDate.Count)
					{
						value1 = ArplStatusDate[i];
					}

					ArplStatusesEntityAssociation.Add(new ArPayPlansArplStatuses( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class ArPayPlansArplStatuses
	{
		public string ArplStatusAssocMember;	
		public DateTime? ArplStatusDateAssocMember;	
		public ArPayPlansArplStatuses() {}
		public ArPayPlansArplStatuses(
			string inArplStatus,
			DateTime? inArplStatusDate)
		{
			ArplStatusAssocMember = inArplStatus;
			ArplStatusDateAssocMember = inArplStatusDate;
		}
	}
}