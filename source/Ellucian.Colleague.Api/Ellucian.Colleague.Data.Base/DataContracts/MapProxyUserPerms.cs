//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/23/2021 3:59:30 PM by user sushrutcolleague
//
//     Type: ENTITY
//     Entity: MAP.PROXY.USER.PERMS
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
	[DataContract(Name = "MapProxyUserPerms")]
	[ColleagueDataContract(GeneratedDateTime = "3/23/2021 3:59:30 PM", User = "sushrutcolleague")]
	[EntityDataContract(EntityName = "MAP.PROXY.USER.PERMS", EntityType = "PHYS")]
	public class MapProxyUserPerms : IColleagueEntity
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
		/// CDD Name: MPUP.PERMISSION
		/// </summary>
		[DataMember(Order = 0, Name = "MPUP.PERMISSION")]
		public string MpupPermission { get; set; }
		
		/// <summary>
		/// CDD Name: MPUP.PROXY.ACCESS.PERMISSION
		/// </summary>
		[DataMember(Order = 1, Name = "MPUP.PROXY.ACCESS.PERMISSION")]
		public string MpupProxyAccessPermission { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}