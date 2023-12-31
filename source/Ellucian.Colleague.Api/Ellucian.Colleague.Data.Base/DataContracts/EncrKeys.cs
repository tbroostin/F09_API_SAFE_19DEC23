//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/19/2019 1:06:18 PM by user sxs
//
//     Type: ENTITY
//     Entity: ENCR.KEYS
//     Application: UT
//     Environment: dvetk_wstst01
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
	[DataContract(Name = "EncrKeys")]
	[ColleagueDataContract(GeneratedDateTime = "4/19/2019 1:06:18 PM", User = "sxs")]
	[EntityDataContract(EntityName = "ENCR.KEYS", EntityType = "PHYS")]
	public class EncrKeys : IColleagueEntity
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
		/// CDD Name: ENCRK.NAME
		/// </summary>
		[DataMember(Order = 0, Name = "ENCRK.NAME")]
		public string EncrkName { get; set; }
		
		/// <summary>
		/// CDD Name: ENCRK.DESC
		/// </summary>
		[DataMember(Order = 1, Name = "ENCRK.DESC")]
		public string EncrkDesc { get; set; }
		
		/// <summary>
		/// CDD Name: ENCRK.KEY
		/// </summary>
		[DataMember(Order = 2, Name = "ENCRK.KEY")]
		public string EncrkKey { get; set; }
		
		/// <summary>
		/// CDD Name: ENCRK.VERSION
		/// </summary>
		[DataMember(Order = 3, Name = "ENCRK.VERSION")]
		public int? EncrkVersion { get; set; }
		
		/// <summary>
		/// CDD Name: ENCRK.STATUS
		/// </summary>
		[DataMember(Order = 4, Name = "ENCRK.STATUS")]
		public string EncrkStatus { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}