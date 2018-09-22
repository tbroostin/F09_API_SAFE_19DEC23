//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/18/2017 3:23:50 PM by user nickkirschke
//
//     Type: ENTITY
//     Entity: ASGMT.CONTRACT.TYPES
//     Application: HR
//     Environment: coldevwapp01
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
	[DataContract(Name = "AsgmtContractTypes")]
	[ColleagueDataContract(GeneratedDateTime = "10/18/2017 3:23:50 PM", User = "nickkirschke")]
	[EntityDataContract(EntityName = "ASGMT.CONTRACT.TYPES", EntityType = "PHYS")]
	public class AsgmtContractTypes : IColleagueEntity
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
		/// CDD Name: ACTYP.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "ACTYP.DESC")]
		public string ActypDesc { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}