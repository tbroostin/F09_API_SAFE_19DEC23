//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/13/2019 3:38:28 PM by user tylerchristiansen
//
//     Type: ENTITY
//     Entity: AWARDS
//     Application: ST
//     Environment: dvcoll_wstst01
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

namespace Ellucian.Colleague.Data.FinancialAid.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Awards")]
	[ColleagueDataContract(GeneratedDateTime = "10/13/2019 3:38:28 PM", User = "tylerchristiansen")]
	[EntityDataContract(EntityName = "AWARDS", EntityType = "PHYS")]
	public class Awards : IColleagueGuidEntity
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
		/// CDD Name: AW.DESCRIPTION
		/// </summary>
		[DataMember(Order = 4, Name = "AW.DESCRIPTION")]
		public string AwDescription { get; set; }
		
		/// <summary>
		/// CDD Name: AW.CATEGORY
		/// </summary>
		[DataMember(Order = 5, Name = "AW.CATEGORY")]
		public string AwCategory { get; set; }
		
		/// <summary>
		/// CDD Name: AW.TYPE
		/// </summary>
		[DataMember(Order = 6, Name = "AW.TYPE")]
		public string AwType { get; set; }
		
		/// <summary>
		/// CDD Name: AW.DL.LOAN.TYPE
		/// </summary>
		[DataMember(Order = 58, Name = "AW.DL.LOAN.TYPE")]
		public string AwDlLoanType { get; set; }
		
		/// <summary>
		/// CDD Name: AW.NEED.COST
		/// </summary>
		[DataMember(Order = 61, Name = "AW.NEED.COST")]
		public string AwNeedCost { get; set; }
		
		/// <summary>
		/// CDD Name: AW.REPORTING.FUNDING.TYPE
		/// </summary>
		[DataMember(Order = 88, Name = "AW.REPORTING.FUNDING.TYPE")]
		public string AwReportingFundingType { get; set; }
		
		/// <summary>
		/// CDD Name: AW.SHOPSHEET.GROUP
		/// </summary>
		[DataMember(Order = 93, Name = "AW.SHOPSHEET.GROUP")]
		public string AwShopsheetGroup { get; set; }
		
		/// <summary>
		/// CDD Name: AW.EXPLANATION.TEXT
		/// </summary>
		[DataMember(Order = 94, Name = "AW.EXPLANATION.TEXT")]
		public string AwExplanationText { get; set; }
		
		/// <summary>
		/// CDD Name: AW.RENEWABLE.FLAG
		/// </summary>
		[DataMember(Order = 99, Name = "AW.RENEWABLE.FLAG")]
		public string AwRenewableFlag { get; set; }
		
		/// <summary>
		/// CDD Name: AW.RENEWABLE.TEXT
		/// </summary>
		[DataMember(Order = 100, Name = "AW.RENEWABLE.TEXT")]
		public string AwRenewableText { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}