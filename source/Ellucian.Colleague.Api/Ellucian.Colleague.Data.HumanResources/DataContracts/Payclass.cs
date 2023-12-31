//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/31/2017 9:18:03 AM by user sbhole1
//
//     Type: ENTITY
//     Entity: PAYCLASS
//     Application: HR
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

namespace Ellucian.Colleague.Data.HumanResources.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Payclass")]
	[ColleagueDataContract(GeneratedDateTime = "10/31/2017 9:18:03 AM", User = "sbhole1")]
	[EntityDataContract(EntityName = "PAYCLASS", EntityType = "PHYS")]
	public class Payclass : IColleagueGuidEntity
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
		/// CDD Name: PCLS.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "PCLS.DESC")]
		public string PclsDesc { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.ACTIVE.FLAG
		/// </summary>
		[DataMember(Order = 1, Name = "PCLS.ACTIVE.FLAG")]
		public string PclsActiveFlag { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.CYCLES.PER.YEAR
		/// </summary>
		[DataMember(Order = 2, Name = "PCLS.CYCLES.PER.YEAR")]
		public int? PclsCyclesPerYear { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.CYCLE.FREQ
		/// </summary>
		[DataMember(Order = 3, Name = "PCLS.CYCLE.FREQ")]
		public string PclsCycleFreq { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.CYCLE.WORK.TIME.AMT
		/// </summary>
		[DataMember(Order = 4, Name = "PCLS.CYCLE.WORK.TIME.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PclsCycleWorkTimeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.CYCLE.WORK.TIME.UNITS
		/// </summary>
		[DataMember(Order = 5, Name = "PCLS.CYCLE.WORK.TIME.UNITS")]
		public string PclsCycleWorkTimeUnits { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.HRLY.OR.SLRY
		/// </summary>
		[DataMember(Order = 8, Name = "PCLS.HRLY.OR.SLRY")]
		public string PclsHrlyOrSlry { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.YEAR.WORK.TIME.AMT
		/// </summary>
		[DataMember(Order = 14, Name = "PCLS.YEAR.WORK.TIME.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? PclsYearWorkTimeAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.PAYCYCLE
		/// </summary>
		[DataMember(Order = 15, Name = "PCLS.PAYCYCLE")]
		public string PclsPaycycle { get; set; }
		
		/// <summary>
		/// CDD Name: PCLS.YEAR.WORK.TIME.UNITS
		/// </summary>
		[DataMember(Order = 16, Name = "PCLS.YEAR.WORK.TIME.UNITS")]
		public string PclsYearWorkTimeUnits { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}