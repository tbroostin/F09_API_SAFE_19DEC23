//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:37:04 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: REG.AR.POSTINGS
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
	[DataContract(Name = "RegArPostings")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:37:04 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "REG.AR.POSTINGS", EntityType = "PHYS")]
	public class RegArPostings : IColleagueEntity
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
		/// CDD Name: RGAR.STUDENT
		/// </summary>
		[DataMember(Order = 0, Name = "RGAR.STUDENT")]
		public string RgarStudent { get; set; }
		
		/// <summary>
		/// CDD Name: RGAR.REG.AR.POSTING.ITEMS
		/// </summary>
		[DataMember(Order = 3, Name = "RGAR.REG.AR.POSTING.ITEMS")]
		public List<string> RgarRegArPostingItems { get; set; }
		
		/// <summary>
		/// CDD Name: RGAR.AR.TYPE
		/// </summary>
		[DataMember(Order = 5, Name = "RGAR.AR.TYPE")]
		public string RgarArType { get; set; }
		
		/// <summary>
		/// CDD Name: RGAR.INVOICE
		/// </summary>
		[DataMember(Order = 6, Name = "RGAR.INVOICE")]
		public string RgarInvoice { get; set; }
		
		/// <summary>
		/// CDD Name: RGAR.BILLING.START.DATE
		/// </summary>
		[DataMember(Order = 8, Name = "RGAR.BILLING.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RgarBillingStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: RGAR.BILLING.END.DATE
		/// </summary>
		[DataMember(Order = 9, Name = "RGAR.BILLING.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RgarBillingEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: RGAR.TERM
		/// </summary>
		[DataMember(Order = 21, Name = "RGAR.TERM")]
		public string RgarTerm { get; set; }
		
		/// <summary>
		/// CDD Name: RGAR.ADJ.BY.REG.AR.POSTING
		/// </summary>
		[DataMember(Order = 34, Name = "RGAR.ADJ.BY.REG.AR.POSTING")]
		public string RgarAdjByRegArPosting { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}