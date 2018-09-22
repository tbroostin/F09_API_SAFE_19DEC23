//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/3/2018 8:59:10 AM by user asainju1
//
//     Type: ENTITY
//     Entity: OB.DESCS
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "ObDescs")]
	[ColleagueDataContract(GeneratedDateTime = "5/3/2018 8:59:10 AM", User = "asainju1")]
	[EntityDataContract(EntityName = "OB.DESCS", EntityType = "PHYS")]
	public class ObDescs : IColleagueGuidEntity
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
		/// CDD Name: OB.DESCRIPTION
		/// </summary>
		[DataMember(Order = 0, Name = "OB.DESCRIPTION")]
		public string ObDescription { get; set; }
		
		/// <summary>
		/// CDD Name: OB.EXPLANATION
		/// </summary>
		[DataMember(Order = 1, Name = "OB.EXPLANATION")]
		public string ObExplanation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}