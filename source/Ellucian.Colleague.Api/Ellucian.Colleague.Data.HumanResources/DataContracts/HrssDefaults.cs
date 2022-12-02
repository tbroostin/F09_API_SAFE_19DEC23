//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 6/27/2022 1:56:11 PM by user yogeshyadav
//
//     Type: ENTITY
//     Entity: HRSS.DEFAULTS
//     Application: HR
//     Environment: DevCol
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
	[DataContract(Name = "HrssDefaults")]
	[ColleagueDataContract(GeneratedDateTime = "6/27/2022 1:56:11 PM", User = "yogeshyadav")]
	[EntityDataContract(EntityName = "HRSS.DEFAULTS", EntityType = "PERM")]
	public class HrssDefaults : IColleagueEntity
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
		/// CDD Name: HRSS.OT.CALC.DEFINITION.ID
		/// </summary>
		[DataMember(Order = 0, Name = "HRSS.OT.CALC.DEFINITION.ID")]
		public string HrssOtCalcDefinitionId { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.HOURLY.RULE
		/// </summary>
		[DataMember(Order = 1, Name = "HRSS.HOURLY.RULE")]
		public string HrssHourlyRule { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.SALARY.RULE
		/// </summary>
		[DataMember(Order = 2, Name = "HRSS.SALARY.RULE")]
		public string HrssSalaryRule { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.DFLT.WORK.WEEK.START.DY
		/// </summary>
		[DataMember(Order = 21, Name = "HRSS.DFLT.WORK.WEEK.START.DY")]
		public string HrssDfltWorkWeekStartDy { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.DFLT.WORK.WEEK.START.TM
		/// </summary>
		[DataMember(Order = 22, Name = "HRSS.DFLT.WORK.WEEK.START.TM")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? HrssDfltWorkWeekStartTm { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.FAC.CONT.WEB.VISIBLE
		/// </summary>
		[DataMember(Order = 24, Name = "HRSS.FAC.CONT.WEB.VISIBLE")]
		public List<string> HrssFacContWebVisible { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.EXCLUDE.LEAVE.PLAN.IDS
		/// </summary>
		[DataMember(Order = 26, Name = "HRSS.EXCLUDE.LEAVE.PLAN.IDS")]
		public List<string> HrssExcludeLeavePlanIds { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.ENABLE.BENEFITS.ENRLMNT
		/// </summary>
		[DataMember(Order = 36, Name = "HRSS.ENABLE.BENEFITS.ENRLMNT")]
		public string HrssEnableBenefitsEnrlmnt { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.LEAVE.LKBK
		/// </summary>
		[DataMember(Order = 45, Name = "HRSS.LEAVE.LKBK")]
		public int? HrssLeaveLkbk { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.LR.UNSUBMIT.WDRW
		/// </summary>
		[DataMember(Order = 50, Name = "HRSS.LR.UNSUBMIT.WDRW")]
		public string HrssLrUnsubmitWdrw { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.LR.ALLOW.SUPRVSR.EDIT
		/// </summary>
		[DataMember(Order = 53, Name = "HRSS.LR.ALLOW.SUPRVSR.EDIT")]
		public string HrssLrAllowSuprvsrEdit { get; set; }
		
		/// <summary>
		/// CDD Name: HRSS.DISPLAY.NAME.HIERARCHY
		/// </summary>
		[DataMember(Order = 55, Name = "HRSS.DISPLAY.NAME.HIERARCHY")]
		public string HrssDisplayNameHierarchy { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}