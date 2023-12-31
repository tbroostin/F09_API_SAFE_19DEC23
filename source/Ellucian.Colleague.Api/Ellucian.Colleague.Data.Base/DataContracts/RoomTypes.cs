//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:55:01 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: ROOM.TYPES
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
	[DataContract(Name = "RoomTypes")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:55:01 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "ROOM.TYPES", EntityType = "PHYS")]
	public class RoomTypes : IColleagueGuidEntity
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
		/// CDD Name: RMTP.DESCRIPTION
		/// </summary>
		[DataMember(Order = 0, Name = "RMTP.DESCRIPTION")]
		public string RmtpDescription { get; set; }
		
		/// <summary>
		/// CDD Name: RMTP.INTG.ROOM.TYPE
		/// </summary>
		[DataMember(Order = 16, Name = "RMTP.INTG.ROOM.TYPE")]
		public string RmtpIntgRoomType { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}