//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:34:08 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: AR.INVOICES
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
	[DataContract(Name = "ArInvoices")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:34:08 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "AR.INVOICES", EntityType = "PHYS")]
	public class ArInvoices : IColleagueEntity
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
		/// CDD Name: INV.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "INV.DESC")]
		public string InvDesc { get; set; }
		
		/// <summary>
		/// CDD Name: INV.PERSON.ID
		/// </summary>
		[DataMember(Order = 1, Name = "INV.PERSON.ID")]
		public string InvPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: INV.AR.TYPE
		/// </summary>
		[DataMember(Order = 2, Name = "INV.AR.TYPE")]
		public string InvArType { get; set; }
		
		/// <summary>
		/// CDD Name: INV.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "INV.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? InvDate { get; set; }
		
		/// <summary>
		/// CDD Name: INV.TERM
		/// </summary>
		[DataMember(Order = 4, Name = "INV.TERM")]
		public string InvTerm { get; set; }
		
		/// <summary>
		/// CDD Name: INV.DUE.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "INV.DUE.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? InvDueDate { get; set; }
		
		/// <summary>
		/// CDD Name: INV.INVOICE.ITEMS
		/// </summary>
		[DataMember(Order = 6, Name = "INV.INVOICE.ITEMS")]
		public List<string> InvInvoiceItems { get; set; }
		
		/// <summary>
		/// CDD Name: INV.AR.POSTED.FLAG
		/// </summary>
		[DataMember(Order = 7, Name = "INV.AR.POSTED.FLAG")]
		public string InvArPostedFlag { get; set; }
		
		/// <summary>
		/// CDD Name: INV.TYPE
		/// </summary>
		[DataMember(Order = 12, Name = "INV.TYPE")]
		public string InvType { get; set; }
		
		/// <summary>
		/// CDD Name: INV.BILLING.START.DATE
		/// </summary>
		[DataMember(Order = 18, Name = "INV.BILLING.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? InvBillingStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: INV.BILLING.END.DATE
		/// </summary>
		[DataMember(Order = 19, Name = "INV.BILLING.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? InvBillingEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: INV.NO
		/// </summary>
		[DataMember(Order = 20, Name = "INV.NO")]
		public string InvNo { get; set; }
		
		/// <summary>
		/// CDD Name: INV.LOCATION
		/// </summary>
		[DataMember(Order = 21, Name = "INV.LOCATION")]
		public string InvLocation { get; set; }
		
		/// <summary>
		/// CDD Name: INV.ADJ.BY.INVOICES
		/// </summary>
		[DataMember(Order = 25, Name = "INV.ADJ.BY.INVOICES")]
		public List<string> InvAdjByInvoices { get; set; }
		
		/// <summary>
		/// CDD Name: INV.ADJ.TO.INVOICE
		/// </summary>
		[DataMember(Order = 26, Name = "INV.ADJ.TO.INVOICE")]
		public string InvAdjToInvoice { get; set; }
		
		/// <summary>
		/// CDD Name: INV.REFERENCE.NOS
		/// </summary>
		[DataMember(Order = 32, Name = "INV.REFERENCE.NOS")]
		public List<string> InvReferenceNos { get; set; }
		
		/// <summary>
		/// CDD Name: INV.ARCHIVE
		/// </summary>
		[DataMember(Order = 53, Name = "INV.ARCHIVE")]
		public string InvArchive { get; set; }
		
		/// <summary>
		/// CDD Name: INV.CHARGE.AMT
		/// </summary>
		[DataMember(Order = 59, Name = "INV.CHARGE.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? InvChargeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: INV.CREDIT.AMT
		/// </summary>
		[DataMember(Order = 60, Name = "INV.CREDIT.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? InvCreditAmt { get; set; }
		
		/// <summary>
		/// CDD Name: INV.TAX.CHG.AMT
		/// </summary>
		[DataMember(Order = 61, Name = "INV.TAX.CHG.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? InvTaxChgAmt { get; set; }
		
		/// <summary>
		/// CDD Name: INV.TAX.CR.AMT
		/// </summary>
		[DataMember(Order = 62, Name = "INV.TAX.CR.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? InvTaxCrAmt { get; set; }
		
		/// <summary>
		/// CDD Name: INV.EXTERNAL.ID
		/// </summary>
		[DataMember(Order = 63, Name = "INV.EXTERNAL.ID")]
		public string InvExternalId { get; set; }
		
		/// <summary>
		/// CDD Name: INV.EXTERNAL.SYSTEM
		/// </summary>
		[DataMember(Order = 64, Name = "INV.EXTERNAL.SYSTEM")]
		public string InvExternalSystem { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}