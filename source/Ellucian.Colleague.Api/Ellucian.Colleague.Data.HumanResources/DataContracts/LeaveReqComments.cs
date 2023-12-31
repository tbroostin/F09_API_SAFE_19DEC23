//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2019 12:48:47 PM by user ganeshrajan
//
//     Type: ENTITY
//     Entity: LEAVE.REQ.COMMENTS
//     Application: HR
//     Environment: Colleage Studio Dev
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
	[DataContract(Name = "LeaveReqComments")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2019 12:48:47 PM", User = "ganeshrajan")]
	[EntityDataContract(EntityName = "LEAVE.REQ.COMMENTS", EntityType = "PHYS")]
	public class LeaveReqComments : IColleagueEntity
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
		/// CDD Name: LRC.LEAVE.REQUEST.ID
		/// </summary>
		[DataMember(Order = 0, Name = "LRC.LEAVE.REQUEST.ID")]
		public string LrcLeaveRequestId { get; set; }
		
		/// <summary>
		/// CDD Name: LRC.COMMENTS
		/// </summary>
		[DataMember(Order = 1, Name = "LRC.COMMENTS")]
		public string LrcComments { get; set; }
		
		/// <summary>
		/// CDD Name: LRC.EMPLOYEE.ID
		/// </summary>
		[DataMember(Order = 2, Name = "LRC.EMPLOYEE.ID")]
		public string LrcEmployeeId { get; set; }
		
		/// <summary>
		/// CDD Name: LRC.ADD.OPERNAME
		/// </summary>
		[DataMember(Order = 3, Name = "LRC.ADD.OPERNAME")]
		public string LrcAddOpername { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQ.COMMENTS.ADDDATE
		/// </summary>
		[DataMember(Order = 4, Name = "LEAVE.REQ.COMMENTS.ADDDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LeaveReqCommentsAdddate { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQ.COMMENTS.ADDTIME
		/// </summary>
		[DataMember(Order = 5, Name = "LEAVE.REQ.COMMENTS.ADDTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LeaveReqCommentsAddtime { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQ.COMMENTS.ADDOPR
		/// </summary>
		[DataMember(Order = 6, Name = "LEAVE.REQ.COMMENTS.ADDOPR")]
		public string LeaveReqCommentsAddopr { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQ.COMMENTS.CHGDATE
		/// </summary>
		[DataMember(Order = 7, Name = "LEAVE.REQ.COMMENTS.CHGDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LeaveReqCommentsChgdate { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQ.COMMENTS.CHGTIME
		/// </summary>
		[DataMember(Order = 8, Name = "LEAVE.REQ.COMMENTS.CHGTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LeaveReqCommentsChgtime { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQ.COMMENTS.CHGOPR
		/// </summary>
		[DataMember(Order = 9, Name = "LEAVE.REQ.COMMENTS.CHGOPR")]
		public string LeaveReqCommentsChgopr { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}