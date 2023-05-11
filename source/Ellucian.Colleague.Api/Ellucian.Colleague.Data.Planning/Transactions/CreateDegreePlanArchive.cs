//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 9/22/2021 10:59:27 AM by user jmansfield
//
//     Type: CTX
//     Transaction ID: CREATE.DEGREE.PLAN.ARCHIVE
//     Application: ST
//     Environment: dvcoll_wstst01
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

namespace Ellucian.Colleague.Data.Planning.Transactions
{
	[DataContract]
	public class AlCourses
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.COURSE.ID", InBoundData = true)]
		public string AlCourseId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TERM.ID", InBoundData = true)]
		public string AlTermId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SECTION.ID", InBoundData = true)]
		public string AlSectionId { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[SctrqDataMember(AppServerName = "AL.CREDITS", InBoundData = true)]
		public Nullable<Decimal> AlCredits { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.NAME", InBoundData = true)]
		public string AlName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TITLE", InBoundData = true)]
		public string AlTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.APPROVAL.STATUS", InBoundData = true)]
		public string AlCrsApprovalStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.STATUS.BY", InBoundData = true)]
		public string AlCrsStatusBy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.CRS.STATUS.DATE", InBoundData = true)]
		public Nullable<DateTime> AlCrsStatusDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "AL.CRS.STATUS.TIME", InBoundData = true)]
		public Nullable<DateTime> AlCrsStatusTime { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.ADDED.BY", InBoundData = true)]
		public string AlCrsAddedBy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.CRS.ADDED.DATE", InBoundData = true)]
		public Nullable<DateTime> AlCrsAddedDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "AL.CRS.ADDED.TIME", InBoundData = true)]
		public Nullable<DateTime> AlCrsAddedTime { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[SctrqDataMember(AppServerName = "AL.CRS.CEUS", InBoundData = true)]
		public Nullable<Decimal> AlCrsCeus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.STC.STATUS", InBoundData = true)]
		public string AlCrsStcStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.IS.PLANNED", InBoundData = true)]
		public string AlCrsIsPlanned { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CRS.HAS.WITHDRAW.GRD", InBoundData = true)]
		public string AlCrsHasWithdrawGrd { get; set; }
	}

	[DataContract]
	public class AlAcadPrograms
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.ACAD.PROGRAM", InBoundData = true)]
		public string AlProgram { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CATALOG", InBoundData = true)]
		public string AlCatalog { get; set; }
	}

	[DataContract]
	public class AlComments
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.COMMENT.TEXT", InBoundData = true)]
		public string AlCommentText { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.COMMENT.ADDED.BY", InBoundData = true)]
		public string AlCommentAddedBy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.COMMENT.ADDED.DATE", InBoundData = true)]
		public Nullable<DateTime> AlCommentAddedDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "AL.COMMENT.ADDED.TIME", InBoundData = true)]
		public Nullable<DateTime> AlCommentAddedTime { get; set; }
	}

	[DataContract]
	public class CoursePlaceholders
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CPH.ID", InBoundData = true)]
		public string CoursePlaceholderId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CPH.ACAD.REQMT", InBoundData = true)]
		public string PlaceholderAcadReqmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CPH.CREDITS", InBoundData = true)]
		public string PlaceholderCredits { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CPH.GROUP.ACAD.REQMT", InBoundData = true)]
		public string PlaceholderGroupAcadReqmt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CPH.SREQ.ACAD.REQMT", InBoundData = true)]
		public string PlaceholderSubrequirement { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CPH.TERM", InBoundData = true)]
		public string PlaceholderTerm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CPH.TITLE", InBoundData = true)]
		public string PlaceholderTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CPH.ADDED.BY", InBoundData = true)]
		public string PlaceholderAddedBy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "AL.CPH.ADDED.DATE", InBoundData = true)]
		public Nullable<DateTime> PlaceholderAddedDate { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[SctrqDataMember(AppServerName = "AL.CPH.ADDED.TIME", InBoundData = true)]
		public Nullable<DateTime> PlaceholderAddedTime { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.DEGREE.PLAN.ARCHIVE", GeneratedDateTime = "9/22/2021 10:59:27 AM", User = "jmansfield")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class CreateDegreePlanArchiveRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.DEGREE.PLAN.ID", InBoundData = true)]        
		public string ADegreePlanId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DP.STUDENT.ID", InBoundData = true)]        
		public string ADpStudentId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DP.VERSION.NUMBER", InBoundData = true)]        
		public string ADpVersionNumber { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DP.LAST.REVIEWED.BY", InBoundData = true)]        
		public string ADpLastReviewedBy { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "A.DP.LAST.REVIEWED.DATE", InBoundData = true)]        
		public Nullable<DateTime> ADpLastReviewedDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.COURSE.ID", InBoundData = true)]
		public List<AlCourses> AlCourses { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.ACAD.PROGRAM", InBoundData = true)]
		public List<AlAcadPrograms> AlAcadPrograms { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.COMMENT.ADDED.BY", InBoundData = true)]
		public List<AlComments> AlComments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.CPH.ID", InBoundData = true)]
		public List<CoursePlaceholders> CoursePlaceholders { get; set; }

		public CreateDegreePlanArchiveRequest()
		{	
			AlCourses = new List<AlCourses>();
			AlAcadPrograms = new List<AlAcadPrograms>();
			AlComments = new List<AlComments>();
			CoursePlaceholders = new List<CoursePlaceholders>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "CREATE.DEGREE.PLAN.ARCHIVE", GeneratedDateTime = "9/22/2021 10:59:27 AM", User = "jmansfield")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 2)]
	public class CreateDegreePlanArchiveResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DEGREE.PLAN.ARCHIVE.ID", OutBoundData = true)]        
		public string ADegreePlanArchiveId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.ERROR.MESSAGE", OutBoundData = true)]        
		public string AErrorMessage { get; set; }

		public CreateDegreePlanArchiveResponse()
		{	
		}
	}
}
