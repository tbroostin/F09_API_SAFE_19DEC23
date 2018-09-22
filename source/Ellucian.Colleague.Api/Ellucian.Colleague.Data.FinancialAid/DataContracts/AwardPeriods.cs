//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:52:06 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: AWARD.PERIODS
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

namespace Ellucian.Colleague.Data.FinancialAid.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "AwardPeriods")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:52:06 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "AWARD.PERIODS", EntityType = "PHYS")]
	public class AwardPeriods : IColleagueGuidEntity
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
		/// CDD Name: AWDP.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "AWDP.DESC")]
		public string AwdpDesc { get; set; }
		
		/// <summary>
		/// CDD Name: AWDP.START.DATE
		/// </summary>
		[DataMember(Order = 1, Name = "AWDP.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? AwdpStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: AWDP.END.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "AWDP.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? AwdpEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: AWDP.ACAD.TERMS
		/// </summary>
		[DataMember(Order = 6, Name = "AWDP.ACAD.TERMS")]
		public List<string> AwdpAcadTerms { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}