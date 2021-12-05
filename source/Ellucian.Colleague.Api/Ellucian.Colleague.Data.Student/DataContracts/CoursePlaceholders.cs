//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/13/2021 10:34:01 AM by user jmansfield
//
//     Type: ENTITY
//     Entity: COURSE.PLACEHOLDERS
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CoursePlaceholders")]
	[ColleagueDataContract(GeneratedDateTime = "7/13/2021 10:34:01 AM", User = "jmansfield")]
	[EntityDataContract(EntityName = "COURSE.PLACEHOLDERS", EntityType = "PHYS")]
	public class CoursePlaceholders : IColleagueEntity
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
		/// CDD Name: CPH.TITLE
		/// </summary>
		[DataMember(Order = 0, Name = "CPH.TITLE")]
		public string CphTitle { get; set; }
		
		/// <summary>
		/// CDD Name: CPH.START.DATE
		/// </summary>
		[DataMember(Order = 1, Name = "CPH.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CphStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: CPH.END.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "CPH.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? CphEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: CPH.CREDITS
		/// </summary>
		[DataMember(Order = 4, Name = "CPH.CREDITS")]
		public string CphCredits { get; set; }
		
		/// <summary>
		/// CDD Name: CPH.DESCRIPTION
		/// </summary>
		[DataMember(Order = 5, Name = "CPH.DESCRIPTION")]
		public string CphDescription { get; set; }
		
		/// <summary>
		/// CDD Name: CPH.ACAD.REQMT
		/// </summary>
		[DataMember(Order = 6, Name = "CPH.ACAD.REQMT")]
		public string CphAcadReqmt { get; set; }
		
		/// <summary>
		/// CDD Name: CPH.SREQ.ACAD.REQMT.BLOCK
		/// </summary>
		[DataMember(Order = 7, Name = "CPH.SREQ.ACAD.REQMT.BLOCK")]
		public string CphSreqAcadReqmtBlock { get; set; }
		
		/// <summary>
		/// CDD Name: CPH.GROUP.ACAD.REQMT.BLOCK
		/// </summary>
		[DataMember(Order = 8, Name = "CPH.GROUP.ACAD.REQMT.BLOCK")]
		public string CphGroupAcadReqmtBlock { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}