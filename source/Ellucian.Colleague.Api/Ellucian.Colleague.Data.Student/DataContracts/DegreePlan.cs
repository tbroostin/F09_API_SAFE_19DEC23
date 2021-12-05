//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/26/2021 3:49:46 PM by user ritikakumari
//
//     Type: ENTITY
//     Entity: DEGREE_PLAN
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "DegreePlan")]
	[ColleagueDataContract(GeneratedDateTime = "8/26/2021 3:49:46 PM", User = "ritikakumari")]
	[EntityDataContract(EntityName = "DEGREE_PLAN", EntityType = "PHYS")]
	public class DegreePlan : IColleagueEntity
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
		/// CDD Name: DP.STUDENT.ID
		/// </summary>
		[DataMember(Order = 0, Name = "DP.STUDENT.ID")]
		public string DpStudentId { get; set; }
		
		/// <summary>
		/// CDD Name: DP.VERSION.NUMBER
		/// </summary>
		[DataMember(Order = 1, Name = "DP.VERSION.NUMBER")]
		public string DpVersionNumber { get; set; }
		
		/// <summary>
		/// CDD Name: DP.APPROVAL.DATE
		/// </summary>
		[DataMember(Order = 11, Name = "DP.APPROVAL.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> DpApprovalDate { get; set; }
		
		/// <summary>
		/// CDD Name: DP.APPROVAL.PERSON.ID
		/// </summary>
		[DataMember(Order = 12, Name = "DP.APPROVAL.PERSON.ID")]
		public List<string> DpApprovalPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: DP.APPROVAL.STATUS
		/// </summary>
		[DataMember(Order = 13, Name = "DP.APPROVAL.STATUS")]
		public List<string> DpApprovalStatus { get; set; }
		
		/// <summary>
		/// CDD Name: DP.APPROVAL.TERM.ID
		/// </summary>
		[DataMember(Order = 14, Name = "DP.APPROVAL.TERM.ID")]
		public List<string> DpApprovalTermId { get; set; }
		
		/// <summary>
		/// CDD Name: DP.APPROVAL.TIME
		/// </summary>
		[DataMember(Order = 15, Name = "DP.APPROVAL.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> DpApprovalTime { get; set; }
		
		/// <summary>
		/// CDD Name: DP.APPROVAL.COURSE.ID
		/// </summary>
		[DataMember(Order = 16, Name = "DP.APPROVAL.COURSE.ID")]
		public List<string> DpApprovalCourseId { get; set; }
		
		/// <summary>
		/// CDD Name: DP.REVIEW.REQUESTED
		/// </summary>
		[DataMember(Order = 17, Name = "DP.REVIEW.REQUESTED")]
		public string DpReviewRequested { get; set; }
		
		/// <summary>
		/// CDD Name: DP.LAST.REVIEWED.BY
		/// </summary>
		[DataMember(Order = 18, Name = "DP.LAST.REVIEWED.BY")]
		public string DpLastReviewedBy { get; set; }
		
		/// <summary>
		/// CDD Name: DP.LAST.REVIEWED.DATE
		/// </summary>
		[DataMember(Order = 19, Name = "DP.LAST.REVIEWED.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? DpLastReviewedDate { get; set; }
		
		/// <summary>
		/// CDD Name: DP.REVIEW.REQUESTED.DATE
		/// </summary>
		[DataMember(Order = 20, Name = "DP.REVIEW.REQUESTED.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? DpReviewRequestedDate { get; set; }
		
		/// <summary>
		/// CDD Name: DP.REVIEW.REQUESTED.TIME
		/// </summary>
		[DataMember(Order = 21, Name = "DP.REVIEW.REQUESTED.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? DpReviewRequestedTime { get; set; }
		
		/// <summary>
		/// CDD Name: DP.ARCHIVE.NOTIFICATION.DATE
		/// </summary>
		[DataMember(Order = 25, Name = "DP.ARCHIVE.NOTIFICATION.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? DpArchiveNotificationDate { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<DegreePlanDpApprovals> DpApprovalsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: DP.APPROVALS
			
			DpApprovalsEntityAssociation = new List<DegreePlanDpApprovals>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(DpApprovalDate != null)
			{
				int numDpApprovals = DpApprovalDate.Count;
				if (DpApprovalPersonId !=null && DpApprovalPersonId.Count > numDpApprovals) numDpApprovals = DpApprovalPersonId.Count;
				if (DpApprovalStatus !=null && DpApprovalStatus.Count > numDpApprovals) numDpApprovals = DpApprovalStatus.Count;
				if (DpApprovalTermId !=null && DpApprovalTermId.Count > numDpApprovals) numDpApprovals = DpApprovalTermId.Count;
				if (DpApprovalTime !=null && DpApprovalTime.Count > numDpApprovals) numDpApprovals = DpApprovalTime.Count;
				if (DpApprovalCourseId !=null && DpApprovalCourseId.Count > numDpApprovals) numDpApprovals = DpApprovalCourseId.Count;

				for (int i = 0; i < numDpApprovals; i++)
				{

					DateTime? value0 = null;
					if (DpApprovalDate != null && i < DpApprovalDate.Count)
					{
						value0 = DpApprovalDate[i];
					}


					string value1 = "";
					if (DpApprovalPersonId != null && i < DpApprovalPersonId.Count)
					{
						value1 = DpApprovalPersonId[i];
					}


					string value2 = "";
					if (DpApprovalStatus != null && i < DpApprovalStatus.Count)
					{
						value2 = DpApprovalStatus[i];
					}


					string value3 = "";
					if (DpApprovalTermId != null && i < DpApprovalTermId.Count)
					{
						value3 = DpApprovalTermId[i];
					}


					DateTime? value4 = null;
					if (DpApprovalTime != null && i < DpApprovalTime.Count)
					{
						value4 = DpApprovalTime[i];
					}


					string value5 = "";
					if (DpApprovalCourseId != null && i < DpApprovalCourseId.Count)
					{
						value5 = DpApprovalCourseId[i];
					}

					DpApprovalsEntityAssociation.Add(new DegreePlanDpApprovals( value0, value1, value2, value3, value4, value5));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class DegreePlanDpApprovals
	{
		public DateTime? DpApprovalDateAssocMember;	
		public string DpApprovalPersonIdAssocMember;	
		public string DpApprovalStatusAssocMember;	
		public string DpApprovalTermIdAssocMember;	
		public DateTime? DpApprovalTimeAssocMember;	
		public string DpApprovalCourseIdAssocMember;	
		public DegreePlanDpApprovals() {}
		public DegreePlanDpApprovals(
			DateTime? inDpApprovalDate,
			string inDpApprovalPersonId,
			string inDpApprovalStatus,
			string inDpApprovalTermId,
			DateTime? inDpApprovalTime,
			string inDpApprovalCourseId)
		{
			DpApprovalDateAssocMember = inDpApprovalDate;
			DpApprovalPersonIdAssocMember = inDpApprovalPersonId;
			DpApprovalStatusAssocMember = inDpApprovalStatus;
			DpApprovalTermIdAssocMember = inDpApprovalTermId;
			DpApprovalTimeAssocMember = inDpApprovalTime;
			DpApprovalCourseIdAssocMember = inDpApprovalCourseId;
		}
	}
}