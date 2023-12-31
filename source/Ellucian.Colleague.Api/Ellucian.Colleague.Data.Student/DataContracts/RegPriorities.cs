//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 3:14:43 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: REG.PRIORITIES
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "RegPriorities")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 3:14:43 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "REG.PRIORITIES", EntityType = "PHYS")]
	public class RegPriorities : IColleagueEntity
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
		/// CDD Name: RGPR.STUDENT
		/// </summary>
		[DataMember(Order = 0, Name = "RGPR.STUDENT")]
		public string RgprStudent { get; set; }
		
		/// <summary>
		/// CDD Name: RGPR.TERM
		/// </summary>
		[DataMember(Order = 1, Name = "RGPR.TERM")]
		public string RgprTerm { get; set; }
		
		/// <summary>
		/// CDD Name: RGPR.START.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "RGPR.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RgprStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: RGPR.START.TIME
		/// </summary>
		[DataMember(Order = 4, Name = "RGPR.START.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RgprStartTime { get; set; }
		
		/// <summary>
		/// CDD Name: RGPR.END.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "RGPR.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RgprEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: RGPR.END.TIME
		/// </summary>
		[DataMember(Order = 6, Name = "RGPR.END.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RgprEndTime { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}