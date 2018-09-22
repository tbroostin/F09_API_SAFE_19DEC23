//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:58:30 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: STAFF
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
	[DataContract(Name = "Staff")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:58:30 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "STAFF", EntityType = "PHYS")]
	public class Staff : IColleagueGuidEntity
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
		/// CDD Name: STAFF.TYPE
		/// </summary>
		[DataMember(Order = 3, Name = "STAFF.TYPE")]
		public string StaffType { get; set; }
		
		/// <summary>
		/// CDD Name: STAFF.STATUS
		/// </summary>
		[DataMember(Order = 4, Name = "STAFF.STATUS")]
		public string StaffStatus { get; set; }
		
		/// <summary>
		/// CDD Name: STAFF.ADD.DATE
		/// </summary>
		[DataMember(Order = 6, Name = "STAFF.ADD.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? StaffAddDate { get; set; }
		
		/// <summary>
		/// CDD Name: STAFF.CHANGE.DATE
		/// </summary>
		[DataMember(Order = 8, Name = "STAFF.CHANGE.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? StaffChangeDate { get; set; }
		
		/// <summary>
		/// CDD Name: STAFF.LOGIN.ID
		/// </summary>
		[DataMember(Order = 20, Name = "STAFF.LOGIN.ID")]
		public string StaffLoginId { get; set; }
		
		/// <summary>
		/// CDD Name: STAFF.PRIVACY.CODES.ACCESS
		/// </summary>
		[DataMember(Order = 21, Name = "STAFF.PRIVACY.CODES.ACCESS")]
		public List<string> StaffPrivacyCodesAccess { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}