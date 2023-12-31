//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/1/2022 12:56:18 PM by user Sushrutcolleague
//
//     Type: ENTITY
//     Entity: LEAVE.REQUEST
//     Application: HR
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

namespace Ellucian.Colleague.Data.HumanResources.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "LeaveRequest")]
	[ColleagueDataContract(GeneratedDateTime = "4/1/2022 12:56:18 PM", User = "Sushrutcolleague")]
	[EntityDataContract(EntityName = "LEAVE.REQUEST", EntityType = "PHYS")]
	public class LeaveRequest : IColleagueEntity
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
		/// CDD Name: LR.PERLEAVE.ID
		/// </summary>
		[DataMember(Order = 0, Name = "LR.PERLEAVE.ID")]
		public string LrPerleaveId { get; set; }
		
		/// <summary>
		/// CDD Name: LR.START.DATE
		/// </summary>
		[DataMember(Order = 1, Name = "LR.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LrStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: LR.END.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "LR.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LrEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: LR.EMPLOYEE.ID
		/// </summary>
		[DataMember(Order = 3, Name = "LR.EMPLOYEE.ID")]
		public string LrEmployeeId { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQUEST.ADDDATE
		/// </summary>
		[DataMember(Order = 4, Name = "LEAVE.REQUEST.ADDDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LeaveRequestAdddate { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQUEST.ADDTIME
		/// </summary>
		[DataMember(Order = 5, Name = "LEAVE.REQUEST.ADDTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LeaveRequestAddtime { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQUEST.ADDOPR
		/// </summary>
		[DataMember(Order = 6, Name = "LEAVE.REQUEST.ADDOPR")]
		public string LeaveRequestAddopr { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQUEST.CHGDATE
		/// </summary>
		[DataMember(Order = 7, Name = "LEAVE.REQUEST.CHGDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LeaveRequestChgdate { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQUEST.CHGTIME
		/// </summary>
		[DataMember(Order = 8, Name = "LEAVE.REQUEST.CHGTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? LeaveRequestChgtime { get; set; }
		
		/// <summary>
		/// CDD Name: LEAVE.REQUEST.CHGOPR
		/// </summary>
		[DataMember(Order = 9, Name = "LEAVE.REQUEST.CHGOPR")]
		public string LeaveRequestChgopr { get; set; }
		
		/// <summary>
		/// CDD Name: LR.APPROVER.ID
		/// </summary>
		[DataMember(Order = 10, Name = "LR.APPROVER.ID")]
		public string LrApproverId { get; set; }
		
		/// <summary>
		/// CDD Name: LR.APPROVER.NAME
		/// </summary>
		[DataMember(Order = 11, Name = "LR.APPROVER.NAME")]
		public string LrApproverName { get; set; }
		
		/// <summary>
		/// CDD Name: LR.IS.WITHDRAWN
		/// </summary>
		[DataMember(Order = 12, Name = "LR.IS.WITHDRAWN")]
		public string LrIsWithdrawn { get; set; }
		
		/// <summary>
		/// CDD Name: LR.WITHDRAW.OPTION
		/// </summary>
		[DataMember(Order = 13, Name = "LR.WITHDRAW.OPTION")]
		public string LrWithdrawOption { get; set; }
		
		/// <summary>
		/// CDD Name: LR.IS.WDRW.PENDING.APPROVAL
		/// </summary>
		[DataMember(Order = 14, Name = "LR.IS.WDRW.PENDING.APPROVAL")]
		public string LrIsWdrwPendingApproval { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}