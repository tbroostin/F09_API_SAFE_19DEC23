//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:44:48 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: PROJECTS
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Projects")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:44:48 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "PROJECTS", EntityType = "PHYS")]
	public class Projects : IColleagueGuidEntity
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
		/// CDD Name: PRJ.TITLE
		/// </summary>
		[DataMember(Order = 0, Name = "PRJ.TITLE")]
		public string PrjTitle { get; set; }
		
		/// <summary>
		/// CDD Name: PRJ.REF.NO
		/// </summary>
		[DataMember(Order = 23, Name = "PRJ.REF.NO")]
		public string PrjRefNo { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}