//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/17/2020 4:48:52 PM by user dvcoll-srm
//
//     Type: ENTITY
//     Entity: AR.INV.ITEMS.INTG
//     Application: ST
//     Environment: dvcoll
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "ArInvItemsIntg")]
	[ColleagueDataContract(GeneratedDateTime = "1/17/2020 4:48:52 PM", User = "dvcoll-srm")]
	[EntityDataContract(EntityName = "AR.INV.ITEMS.INTG", EntityType = "PHYS")]
	public class ArInvItemsIntg : IColleagueGuidEntity
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
		/// Record GUID
		/// </summary>
		[DataMember(Name = "RecordGuid")]
		public string RecordGuid { get; set; }

		/// <summary>
		/// Record Model Name
		/// </summary>
		[DataMember(Name = "RecordModelName")]
		public string RecordModelName { get; set; }	
		
		/// <summary>
		/// CDD Name: INVI.INTG.PERSON.ID
		/// </summary>
		[DataMember(Order = 0, Name = "INVI.INTG.PERSON.ID")]
		public string InviIntgPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.AR.TYPE
		/// </summary>
		[DataMember(Order = 1, Name = "INVI.INTG.AR.TYPE")]
		public string InviIntgArType { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.AR.CODE
		/// </summary>
		[DataMember(Order = 2, Name = "INVI.INTG.AR.CODE")]
		public string InviIntgArCode { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.TERM
		/// </summary>
		[DataMember(Order = 3, Name = "INVI.INTG.TERM")]
		public string InviIntgTerm { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.CHARGE.TYPE
		/// </summary>
		[DataMember(Order = 4, Name = "INVI.INTG.CHARGE.TYPE")]
		public string InviIntgChargeType { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.DUE.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "INVI.INTG.DUE.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? InviIntgDueDate { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.COMMENTS
		/// </summary>
		[DataMember(Order = 6, Name = "INVI.INTG.COMMENTS")]
		public string InviIntgComments { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.AMT
		/// </summary>
		[DataMember(Order = 7, Name = "INVI.INTG.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? InviIntgAmt { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.AMT.CURRENCY
		/// </summary>
		[DataMember(Order = 8, Name = "INVI.INTG.AMT.CURRENCY")]
		public string InviIntgAmtCurrency { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.UNIT.QTY
		/// </summary>
		[DataMember(Order = 9, Name = "INVI.INTG.UNIT.QTY")]
		public long? InviIntgUnitQty { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.UNIT.COST
		/// </summary>
		[DataMember(Order = 10, Name = "INVI.INTG.UNIT.COST")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? InviIntgUnitCost { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.UNIT.CURRENCY
		/// </summary>
		[DataMember(Order = 11, Name = "INVI.INTG.UNIT.CURRENCY")]
		public string InviIntgUnitCurrency { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.AR.INV.ITEM
		/// </summary>
		[DataMember(Order = 12, Name = "INVI.INTG.AR.INV.ITEM")]
		public string InviIntgArInvItem { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.USAGE
		/// </summary>
		[DataMember(Order = 13, Name = "INVI.INTG.USAGE")]
		public string InviIntgUsage { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.ORIGINATED.ON
		/// </summary>
		[DataMember(Order = 14, Name = "INVI.INTG.ORIGINATED.ON")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? InviIntgOriginatedOn { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.OVERRIDE.DESC
		/// </summary>
		[DataMember(Order = 15, Name = "INVI.INTG.OVERRIDE.DESC")]
		public string InviIntgOverrideDesc { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.BILLING.START.DATE
		/// </summary>
		[DataMember(Order = 16, Name = "INVI.INTG.BILLING.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? InviIntgBillingStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: INVI.INTG.BILLING.END.DATE
		/// </summary>
		[DataMember(Order = 17, Name = "INVI.INTG.BILLING.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? InviIntgBillingEndDate { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}