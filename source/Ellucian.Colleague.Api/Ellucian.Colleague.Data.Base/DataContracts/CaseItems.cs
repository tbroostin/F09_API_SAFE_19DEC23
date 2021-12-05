//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/19/2020 4:37:31 PM by user spirest
//
//     Type: ENTITY
//     Entity: CASE.ITEMS
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CaseItems")]
	[ColleagueDataContract(GeneratedDateTime = "11/19/2020 4:37:31 PM", User = "spirest")]
	[EntityDataContract(EntityName = "CASE.ITEMS", EntityType = "PHYS")]
	public class CaseItems : IColleagueEntity
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
		/// CDD Name: CIT.CASE
		/// </summary>
		[DataMember(Order = 0, Name = "CIT.CASE")]
		public string CitCase { get; set; }
		
		/// <summary>
		/// CDD Name: CIT.SUMMARY
		/// </summary>
		[DataMember(Order = 1, Name = "CIT.SUMMARY")]
		public string CitSummary { get; set; }
		
		/// <summary>
		/// CDD Name: CIT.INTERNAL.NOTES
		/// </summary>
		[DataMember(Order = 2, Name = "CIT.INTERNAL.NOTES")]
		public string CitInternalNotes { get; set; }
		
		/// <summary>
		/// CDD Name: CASE.ITEMS.ADDOPR
		/// </summary>
		[DataMember(Order = 3, Name = "CASE.ITEMS.ADDOPR")]
		public string CaseItemsAddopr { get; set; }
		
		/// <summary>
		/// CDD Name: CASE.ITEMS.ADDDATE
		/// </summary>
		[DataMember(Order = 4, Name = "CASE.ITEMS.ADDDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CaseItemsAdddate { get; set; }
		
		/// <summary>
		/// CDD Name: CASE.ITEMS.CHGOPR
		/// </summary>
		[DataMember(Order = 5, Name = "CASE.ITEMS.CHGOPR")]
		public string CaseItemsChgopr { get; set; }
		
		/// <summary>
		/// CDD Name: CASE.ITEMS.CHGDATE
		/// </summary>
		[DataMember(Order = 6, Name = "CASE.ITEMS.CHGDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CaseItemsChgdate { get; set; }
		
		/// <summary>
		/// CDD Name: CIT.METHOD.OF.CONTACT
		/// </summary>
		[DataMember(Order = 7, Name = "CIT.METHOD.OF.CONTACT")]
		public List<string> CitMethodOfContact { get; set; }
		
		/// <summary>
		/// CDD Name: CIT.REMINDER.DATE
		/// </summary>
		[DataMember(Order = 8, Name = "CIT.REMINDER.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CitReminderDate { get; set; }
		
		/// <summary>
		/// CDD Name: CIT.ADDOPR.ID
		/// </summary>
		[DataMember(Order = 9, Name = "CIT.ADDOPR.ID")]
		public string CitAddoprId { get; set; }
		
		/// <summary>
		/// CDD Name: CIT.ADD.TIME
		/// </summary>
		[DataMember(Order = 10, Name = "CIT.ADD.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CitAddTime { get; set; }
		
		/// <summary>
		/// CDD Name: CIT.ITEM.TYPE
		/// </summary>
		[DataMember(Order = 11, Name = "CIT.ITEM.TYPE")]
		public string CitItemType { get; set; }
		
		/// <summary>
		/// CDD Name: CIT.CONTRIBUTION.CASE.TYPE
		/// </summary>
		[DataMember(Order = 12, Name = "CIT.CONTRIBUTION.CASE.TYPE")]
		public string CitContributionCaseType { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}