//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:46:12 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: RELATIONSHIP
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
	[DataContract(Name = "Relationship")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:46:12 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "RELATIONSHIP", EntityType = "PHYS")]
	public class Relationship : IColleagueGuidEntity
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
		/// CDD Name: RS.ID1
		/// </summary>
		[DataMember(Order = 0, Name = "RS.ID1")]
		public string RsId1 { get; set; }
		
		/// <summary>
		/// CDD Name: RS.ID2
		/// </summary>
		[DataMember(Order = 1, Name = "RS.ID2")]
		public string RsId2 { get; set; }
		
		/// <summary>
		/// CDD Name: RS.RELATION.TYPE
		/// </summary>
		[DataMember(Order = 2, Name = "RS.RELATION.TYPE")]
		public string RsRelationType { get; set; }
		
		/// <summary>
		/// CDD Name: RS.PRIMARY.RELATIONSHIP.FLAG
		/// </summary>
		[DataMember(Order = 3, Name = "RS.PRIMARY.RELATIONSHIP.FLAG")]
		public string RsPrimaryRelationshipFlag { get; set; }
		
		/// <summary>
		/// CDD Name: RS.START.DATE
		/// </summary>
		[DataMember(Order = 9, Name = "RS.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RsStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: RS.END.DATE
		/// </summary>
		[DataMember(Order = 10, Name = "RS.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? RsEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: RS.STATUS
		/// </summary>
		[DataMember(Order = 11, Name = "RS.STATUS")]
		public string RsStatus { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}