//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:58:56 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: SUFFIXES
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
	[DataContract(Name = "Suffixes")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:58:56 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "SUFFIXES", EntityType = "PERM")]
	public class Suffixes : IColleagueEntity
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
		/// CDD Name: SUFFIXES.CODES
		/// </summary>
		[DataMember(Order = 0, Name = "SUFFIXES.CODES")]
		public List<string> SuffixesCodes { get; set; }
		
		/// <summary>
		/// CDD Name: SUFFIXES.DESCS
		/// </summary>
		[DataMember(Order = 1, Name = "SUFFIXES.DESCS")]
		public List<string> SuffixesDescs { get; set; }
		
		/// <summary>
		/// CDD Name: SUFFIXES.INTERNAL.CODES
		/// </summary>
		[DataMember(Order = 3, Name = "SUFFIXES.INTERNAL.CODES")]
		public List<string> SuffixesInternalCodes { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}