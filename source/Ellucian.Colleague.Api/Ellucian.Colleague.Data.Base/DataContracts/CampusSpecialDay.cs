//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/2/2018 3:46:53 PM by user mdediana
//
//     Type: ENTITY
//     Entity: CAMPUS.SPECIAL.DAY
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
	[DataContract(Name = "CampusSpecialDay")]
	[ColleagueDataContract(GeneratedDateTime = "5/2/2018 3:46:53 PM", User = "mdediana")]
	[EntityDataContract(EntityName = "CAMPUS.SPECIAL.DAY", EntityType = "PHYS")]
	public class CampusSpecialDay : IColleagueEntity
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
		/// CDD Name: CMSD.CAMPUS.CALENDAR
		/// </summary>
		[DataMember(Order = 0, Name = "CMSD.CAMPUS.CALENDAR")]
		public string CmsdCampusCalendar { get; set; }
		
		/// <summary>
		/// CDD Name: CMSD.DESC
		/// </summary>
		[DataMember(Order = 1, Name = "CMSD.DESC")]
		public string CmsdDesc { get; set; }
		
		/// <summary>
		/// CDD Name: CMSD.START.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "CMSD.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CmsdStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: CMSD.END.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "CMSD.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CmsdEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: CMSD.START.TIME
		/// </summary>
		[DataMember(Order = 4, Name = "CMSD.START.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CmsdStartTime { get; set; }
		
		/// <summary>
		/// CDD Name: CMSD.END.TIME
		/// </summary>
		[DataMember(Order = 5, Name = "CMSD.END.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CmsdEndTime { get; set; }
		
		/// <summary>
		/// CDD Name: CMSD.TYPE
		/// </summary>
		[DataMember(Order = 14, Name = "CMSD.TYPE")]
		public string CmsdType { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}