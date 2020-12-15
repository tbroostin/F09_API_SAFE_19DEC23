//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/12/2020 2:26:43 PM by user balvano3
//
//     Type: ENTITY
//     Entity: ORG.ENTITY
//     Application: UT
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
	[DataContract(Name = "OrgEntity")]
	[ColleagueDataContract(GeneratedDateTime = "8/12/2020 2:26:43 PM", User = "balvano3")]
	[EntityDataContract(EntityName = "ORG.ENTITY", EntityType = "PHYS")]
	public class OrgEntity : IColleagueEntity
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
		/// CDD Name: OE.ORG.ENTITY.ENV
		/// </summary>
		[DataMember(Order = 13, Name = "OE.ORG.ENTITY.ENV")]
		public string OeOrgEntityEnv { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}