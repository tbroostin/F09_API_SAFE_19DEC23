//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/1/2017 9:47:17 AM by user sbhole1
//
//     Type: ENTITY
//     Entity: GLA.FYR
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "GlaFyr")]
	[ColleagueDataContract(GeneratedDateTime = "11/1/2017 9:47:17 AM", User = "sbhole1")]
	[EntityDataContract(EntityName = "GLA.FYR", EntityType = "PHYS")]
	public class GlaFyr : IColleagueGuidEntity
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
		/// CDD Name: GLA.SOURCE
		/// </summary>
		[DataMember(Order = 0, Name = "GLA.SOURCE")]
		public string GlaSource { get; set; }
		
		/// <summary>
		/// CDD Name: GLA.REF.NO
		/// </summary>
		[DataMember(Order = 1, Name = "GLA.REF.NO")]
		public string GlaRefNo { get; set; }
		
		/// <summary>
		/// CDD Name: GLA.DESCRIPTION
		/// </summary>
		[DataMember(Order = 2, Name = "GLA.DESCRIPTION")]
		public string GlaDescription { get; set; }
		
		/// <summary>
		/// CDD Name: GLA.DEBIT
		/// </summary>
		[DataMember(Order = 3, Name = "GLA.DEBIT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? GlaDebit { get; set; }
		
		/// <summary>
		/// CDD Name: GLA.CREDIT
		/// </summary>
		[DataMember(Order = 4, Name = "GLA.CREDIT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? GlaCredit { get; set; }
		
		/// <summary>
		/// CDD Name: GLA.TR.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "GLA.TR.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? GlaTrDate { get; set; }
		
		/// <summary>
		/// CDD Name: GLA.SYS.DATE
		/// </summary>
		[DataMember(Order = 6, Name = "GLA.SYS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? GlaSysDate { get; set; }
		
		/// <summary>
		/// CDD Name: GLA.ACCT.ID
		/// </summary>
		[DataMember(Order = 7, Name = "GLA.ACCT.ID")]
		public string GlaAcctId { get; set; }
		
		/// <summary>
		/// CDD Name: GLA.PROJECTS.IDS
		/// </summary>
		[DataMember(Order = 12, Name = "GLA.PROJECTS.IDS")]
		public string GlaProjectsIds { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}