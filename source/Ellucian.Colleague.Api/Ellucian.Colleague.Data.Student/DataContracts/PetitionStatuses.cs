//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 3:14:02 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: PETITION.STATUSES
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
	[DataContract(Name = "PetitionStatuses")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 3:14:02 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "PETITION.STATUSES", EntityType = "PHYS")]
	public class PetitionStatuses : IColleagueEntity
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
		/// CDD Name: PET.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "PET.DESC")]
		public string PetDesc { get; set; }
		
		/// <summary>
		/// CDD Name: PET.GRANTED.FLAG
		/// </summary>
		[DataMember(Order = 2, Name = "PET.GRANTED.FLAG")]
		public string PetGrantedFlag { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}