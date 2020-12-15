//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/6/2020 5:00:03 PM by user asainju1
//
//     Type: ENTITY
//     Entity: ACAD.PROGRAMS
//     Application: ST
//     Environment: coldev
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
	[DataContract(Name = "AcadProgramsBase")]
	[ColleagueDataContract(GeneratedDateTime = "1/6/2020 5:00:03 PM", User = "asainju1")]
	[EntityDataContract(EntityName = "ACAD.PROGRAMS", EntityType = "PHYS")]
	public class AcadProgramsBase : IColleagueGuidEntity
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
		/// CDD Name: ACPG.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "ACPG.DESC")]
		public string AcpgDesc { get; set; }
		
		/// <summary>
		/// CDD Name: ACPG.TITLE
		/// </summary>
		[DataMember(Order = 1, Name = "ACPG.TITLE")]
		public string AcpgTitle { get; set; }
		
		/// <summary>
		/// CDD Name: ACPG.ACAD.LEVEL
		/// </summary>
		[DataMember(Order = 8, Name = "ACPG.ACAD.LEVEL")]
		public string AcpgAcadLevel { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}