//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 9/22/2021 10:59:36 AM by user jmansfield
//
//     Type: ENTITY
//     Entity: DEGREE_PLAN_ARCHIVE
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
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Data.Planning.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "DegreePlanArchive")]
	[ColleagueDataContract(GeneratedDateTime = "9/22/2021 10:59:36 AM", User = "jmansfield")]
	[EntityDataContract(EntityName = "DEGREE_PLAN_ARCHIVE", EntityType = "PHYS")]
	public class DegreePlanArchive : IColleagueEntity
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
		/// CDD Name: DEGREE_PLAN_ARCHIVE.ADDDATE
		/// </summary>
		[DataMember(Order = 0, Name = "DEGREE_PLAN_ARCHIVE.ADDDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? DegreePlanArchiveAdddate { get; set; }
		
		/// <summary>
		/// CDD Name: DEGREE_PLAN_ARCHIVE.ADDOPR
		/// </summary>
		[DataMember(Order = 1, Name = "DEGREE_PLAN_ARCHIVE.ADDOPR")]
		public string DegreePlanArchiveAddopr { get; set; }
		
		/// <summary>
		/// CDD Name: DEGREE_PLAN_ARCHIVE.ADDTIME
		/// </summary>
		[DataMember(Order = 2, Name = "DEGREE_PLAN_ARCHIVE.ADDTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? DegreePlanArchiveAddtime { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.DEGREE.PLAN.ID
		/// </summary>
		[DataMember(Order = 3, Name = "DPARCHV.DEGREE.PLAN.ID")]
		public string DparchvDegreePlanId { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.LAST.REVIEWED.DATE
		/// </summary>
		[DataMember(Order = 4, Name = "DPARCHV.LAST.REVIEWED.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? DparchvLastReviewedDate { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.LAST.REVIEWED.BY
		/// </summary>
		[DataMember(Order = 5, Name = "DPARCHV.LAST.REVIEWED.BY")]
		public string DparchvLastReviewedBy { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.STUDENT.ID
		/// </summary>
		[DataMember(Order = 6, Name = "DPARCHV.STUDENT.ID")]
		public string DparchvStudentId { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.LAST.REVIEWED.TIME
		/// </summary>
		[DataMember(Order = 7, Name = "DPARCHV.LAST.REVIEWED.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? DparchvLastReviewedTime { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.PGM.ACAD.PROGRAM.ID
		/// </summary>
		[DataMember(Order = 8, Name = "DPARCHV.PGM.ACAD.PROGRAM.ID")]
		public List<string> DparchvPgmAcadProgramId { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.PGM.CATALOG
		/// </summary>
		[DataMember(Order = 9, Name = "DPARCHV.PGM.CATALOG")]
		public List<string> DparchvPgmCatalog { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.COURSE.ID
		/// </summary>
		[DataMember(Order = 10, Name = "DPARCHV.CRS.COURSE.ID")]
		public List<string> DparchvCrsCourseId { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.TERM.ID
		/// </summary>
		[DataMember(Order = 11, Name = "DPARCHV.CRS.TERM.ID")]
		public List<string> DparchvCrsTermId { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.SECTION.ID
		/// </summary>
		[DataMember(Order = 12, Name = "DPARCHV.CRS.SECTION.ID")]
		public List<string> DparchvCrsSectionId { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.NAME
		/// </summary>
		[DataMember(Order = 13, Name = "DPARCHV.CRS.NAME")]
		public List<string> DparchvCrsName { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.TITLE
		/// </summary>
		[DataMember(Order = 14, Name = "DPARCHV.CRS.TITLE")]
		public List<string> DparchvCrsTitle { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.CREDITS
		/// </summary>
		[DataMember(Order = 15, Name = "DPARCHV.CRS.CREDITS")]
		[DisplayFormat(DataFormatString = "{0:N5}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> DparchvCrsCredits { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.APPROVAL.STATUS
		/// </summary>
		[DataMember(Order = 16, Name = "DPARCHV.CRS.APPROVAL.STATUS")]
		public List<string> DparchvCrsApprovalStatus { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.STATUS.BY
		/// </summary>
		[DataMember(Order = 17, Name = "DPARCHV.CRS.STATUS.BY")]
		public List<string> DparchvCrsStatusBy { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.STATUS.DATE
		/// </summary>
		[DataMember(Order = 18, Name = "DPARCHV.CRS.STATUS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> DparchvCrsStatusDate { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.STATUS.TIME
		/// </summary>
		[DataMember(Order = 19, Name = "DPARCHV.CRS.STATUS.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> DparchvCrsStatusTime { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.VERSION.NUMBER
		/// </summary>
		[DataMember(Order = 20, Name = "DPARCHV.VERSION.NUMBER")]
		public string DparchvVersionNumber { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.ADDED.BY
		/// </summary>
		[DataMember(Order = 21, Name = "DPARCHV.CRS.ADDED.BY")]
		public List<string> DparchvCrsAddedBy { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.ADDED.ON.DATE
		/// </summary>
		[DataMember(Order = 22, Name = "DPARCHV.CRS.ADDED.ON.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> DparchvCrsAddedOnDate { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.ADDED.ON.TIME
		/// </summary>
		[DataMember(Order = 23, Name = "DPARCHV.CRS.ADDED.ON.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> DparchvCrsAddedOnTime { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.CEUS
		/// </summary>
		[DataMember(Order = 24, Name = "DPARCHV.CRS.CEUS")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> DparchvCrsCeus { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.STC.STATUS
		/// </summary>
		[DataMember(Order = 25, Name = "DPARCHV.CRS.STC.STATUS")]
		public List<string> DparchvCrsStcStatus { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.IS.PLANNED
		/// </summary>
		[DataMember(Order = 26, Name = "DPARCHV.CRS.IS.PLANNED")]
		public List<string> DparchvCrsIsPlanned { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CRS.HAS.WITHDRAW.GRD
		/// </summary>
		[DataMember(Order = 27, Name = "DPARCHV.CRS.HAS.WITHDRAW.GRD")]
		public List<string> DparchvCrsHasWithdrawGrd { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.ID
		/// </summary>
		[DataMember(Order = 28, Name = "DPARCHV.CPH.ID")]
		public List<string> DparchvCphId { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.TITLE
		/// </summary>
		[DataMember(Order = 29, Name = "DPARCHV.CPH.TITLE")]
		public List<string> DparchvCphTitle { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.CREDITS
		/// </summary>
		[DataMember(Order = 31, Name = "DPARCHV.CPH.CREDITS")]
		public List<string> DparchvCphCredits { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.ACAD.REQMT
		/// </summary>
		[DataMember(Order = 32, Name = "DPARCHV.CPH.ACAD.REQMT")]
		public List<string> DparchvCphAcadReqmt { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.SREQ.ACAD.REQMT
		/// </summary>
		[DataMember(Order = 33, Name = "DPARCHV.CPH.SREQ.ACAD.REQMT")]
		public List<string> DparchvCphSreqAcadReqmt { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.GROUP.ACAD.REQMT
		/// </summary>
		[DataMember(Order = 34, Name = "DPARCHV.CPH.GROUP.ACAD.REQMT")]
		public List<string> DparchvCphGroupAcadReqmt { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.TERM
		/// </summary>
		[DataMember(Order = 35, Name = "DPARCHV.CPH.TERM")]
		public List<string> DparchvCphTerm { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.ADDED.BY
		/// </summary>
		[DataMember(Order = 36, Name = "DPARCHV.CPH.ADDED.BY")]
		public List<string> DparchvCphAddedBy { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.ADDED.ON.DATE
		/// </summary>
		[DataMember(Order = 37, Name = "DPARCHV.CPH.ADDED.ON.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> DparchvCphAddedOnDate { get; set; }
		
		/// <summary>
		/// CDD Name: DPARCHV.CPH.ADDED.ON.TIME
		/// </summary>
		[DataMember(Order = 38, Name = "DPARCHV.CPH.ADDED.ON.TIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> DparchvCphAddedOnTime { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<DegreePlanArchiveDparchvPrograms> DparchvProgramsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<DegreePlanArchiveDparchvCourses> DparchvCoursesEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<DegreePlanArchiveDparchvCrsPlaceholders> DparchvCrsPlaceholdersEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: DPARCHV.PROGRAMS
			
			DparchvProgramsEntityAssociation = new List<DegreePlanArchiveDparchvPrograms>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(DparchvPgmAcadProgramId != null)
			{
				int numDparchvPrograms = DparchvPgmAcadProgramId.Count;
				if (DparchvPgmCatalog !=null && DparchvPgmCatalog.Count > numDparchvPrograms) numDparchvPrograms = DparchvPgmCatalog.Count;

				for (int i = 0; i < numDparchvPrograms; i++)
				{

					string value0 = "";
					if (DparchvPgmAcadProgramId != null && i < DparchvPgmAcadProgramId.Count)
					{
						value0 = DparchvPgmAcadProgramId[i];
					}


					string value1 = "";
					if (DparchvPgmCatalog != null && i < DparchvPgmCatalog.Count)
					{
						value1 = DparchvPgmCatalog[i];
					}

					DparchvProgramsEntityAssociation.Add(new DegreePlanArchiveDparchvPrograms( value0, value1));
				}
			}
			// EntityAssociation Name: DPARCHV.COURSES
			
			DparchvCoursesEntityAssociation = new List<DegreePlanArchiveDparchvCourses>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(DparchvCrsCourseId != null)
			{
				int numDparchvCourses = DparchvCrsCourseId.Count;
				if (DparchvCrsTermId !=null && DparchvCrsTermId.Count > numDparchvCourses) numDparchvCourses = DparchvCrsTermId.Count;
				if (DparchvCrsSectionId !=null && DparchvCrsSectionId.Count > numDparchvCourses) numDparchvCourses = DparchvCrsSectionId.Count;
				if (DparchvCrsName !=null && DparchvCrsName.Count > numDparchvCourses) numDparchvCourses = DparchvCrsName.Count;
				if (DparchvCrsTitle !=null && DparchvCrsTitle.Count > numDparchvCourses) numDparchvCourses = DparchvCrsTitle.Count;
				if (DparchvCrsCredits !=null && DparchvCrsCredits.Count > numDparchvCourses) numDparchvCourses = DparchvCrsCredits.Count;
				if (DparchvCrsApprovalStatus !=null && DparchvCrsApprovalStatus.Count > numDparchvCourses) numDparchvCourses = DparchvCrsApprovalStatus.Count;
				if (DparchvCrsStatusBy !=null && DparchvCrsStatusBy.Count > numDparchvCourses) numDparchvCourses = DparchvCrsStatusBy.Count;
				if (DparchvCrsStatusDate !=null && DparchvCrsStatusDate.Count > numDparchvCourses) numDparchvCourses = DparchvCrsStatusDate.Count;
				if (DparchvCrsStatusTime !=null && DparchvCrsStatusTime.Count > numDparchvCourses) numDparchvCourses = DparchvCrsStatusTime.Count;
				if (DparchvCrsAddedBy !=null && DparchvCrsAddedBy.Count > numDparchvCourses) numDparchvCourses = DparchvCrsAddedBy.Count;
				if (DparchvCrsAddedOnDate !=null && DparchvCrsAddedOnDate.Count > numDparchvCourses) numDparchvCourses = DparchvCrsAddedOnDate.Count;
				if (DparchvCrsAddedOnTime !=null && DparchvCrsAddedOnTime.Count > numDparchvCourses) numDparchvCourses = DparchvCrsAddedOnTime.Count;
				if (DparchvCrsCeus !=null && DparchvCrsCeus.Count > numDparchvCourses) numDparchvCourses = DparchvCrsCeus.Count;
				if (DparchvCrsStcStatus !=null && DparchvCrsStcStatus.Count > numDparchvCourses) numDparchvCourses = DparchvCrsStcStatus.Count;
				if (DparchvCrsIsPlanned !=null && DparchvCrsIsPlanned.Count > numDparchvCourses) numDparchvCourses = DparchvCrsIsPlanned.Count;
				if (DparchvCrsHasWithdrawGrd !=null && DparchvCrsHasWithdrawGrd.Count > numDparchvCourses) numDparchvCourses = DparchvCrsHasWithdrawGrd.Count;

				for (int i = 0; i < numDparchvCourses; i++)
				{

					string value0 = "";
					if (DparchvCrsCourseId != null && i < DparchvCrsCourseId.Count)
					{
						value0 = DparchvCrsCourseId[i];
					}


					string value1 = "";
					if (DparchvCrsTermId != null && i < DparchvCrsTermId.Count)
					{
						value1 = DparchvCrsTermId[i];
					}


					string value2 = "";
					if (DparchvCrsSectionId != null && i < DparchvCrsSectionId.Count)
					{
						value2 = DparchvCrsSectionId[i];
					}


					string value3 = "";
					if (DparchvCrsName != null && i < DparchvCrsName.Count)
					{
						value3 = DparchvCrsName[i];
					}


					string value4 = "";
					if (DparchvCrsTitle != null && i < DparchvCrsTitle.Count)
					{
						value4 = DparchvCrsTitle[i];
					}


					Decimal? value5 = null;
					if (DparchvCrsCredits != null && i < DparchvCrsCredits.Count)
					{
						value5 = DparchvCrsCredits[i];
					}


					string value6 = "";
					if (DparchvCrsApprovalStatus != null && i < DparchvCrsApprovalStatus.Count)
					{
						value6 = DparchvCrsApprovalStatus[i];
					}


					string value7 = "";
					if (DparchvCrsStatusBy != null && i < DparchvCrsStatusBy.Count)
					{
						value7 = DparchvCrsStatusBy[i];
					}


					DateTime? value8 = null;
					if (DparchvCrsStatusDate != null && i < DparchvCrsStatusDate.Count)
					{
						value8 = DparchvCrsStatusDate[i];
					}


					DateTime? value9 = null;
					if (DparchvCrsStatusTime != null && i < DparchvCrsStatusTime.Count)
					{
						value9 = DparchvCrsStatusTime[i];
					}


					string value10 = "";
					if (DparchvCrsAddedBy != null && i < DparchvCrsAddedBy.Count)
					{
						value10 = DparchvCrsAddedBy[i];
					}


					DateTime? value11 = null;
					if (DparchvCrsAddedOnDate != null && i < DparchvCrsAddedOnDate.Count)
					{
						value11 = DparchvCrsAddedOnDate[i];
					}


					DateTime? value12 = null;
					if (DparchvCrsAddedOnTime != null && i < DparchvCrsAddedOnTime.Count)
					{
						value12 = DparchvCrsAddedOnTime[i];
					}


					Decimal? value13 = null;
					if (DparchvCrsCeus != null && i < DparchvCrsCeus.Count)
					{
						value13 = DparchvCrsCeus[i];
					}


					string value14 = "";
					if (DparchvCrsStcStatus != null && i < DparchvCrsStcStatus.Count)
					{
						value14 = DparchvCrsStcStatus[i];
					}


					string value15 = "";
					if (DparchvCrsIsPlanned != null && i < DparchvCrsIsPlanned.Count)
					{
						value15 = DparchvCrsIsPlanned[i];
					}


					string value16 = "";
					if (DparchvCrsHasWithdrawGrd != null && i < DparchvCrsHasWithdrawGrd.Count)
					{
						value16 = DparchvCrsHasWithdrawGrd[i];
					}

					DparchvCoursesEntityAssociation.Add(new DegreePlanArchiveDparchvCourses( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15, value16));
				}
			}
			// EntityAssociation Name: DPARCHV.CRS.PLACEHOLDERS
			
			DparchvCrsPlaceholdersEntityAssociation = new List<DegreePlanArchiveDparchvCrsPlaceholders>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(DparchvCphId != null)
			{
				int numDparchvCrsPlaceholders = DparchvCphId.Count;
				if (DparchvCphTitle !=null && DparchvCphTitle.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphTitle.Count;
				if (DparchvCphCredits !=null && DparchvCphCredits.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphCredits.Count;
				if (DparchvCphAcadReqmt !=null && DparchvCphAcadReqmt.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphAcadReqmt.Count;
				if (DparchvCphSreqAcadReqmt !=null && DparchvCphSreqAcadReqmt.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphSreqAcadReqmt.Count;
				if (DparchvCphGroupAcadReqmt !=null && DparchvCphGroupAcadReqmt.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphGroupAcadReqmt.Count;
				if (DparchvCphTerm !=null && DparchvCphTerm.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphTerm.Count;
				if (DparchvCphAddedBy !=null && DparchvCphAddedBy.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphAddedBy.Count;
				if (DparchvCphAddedOnDate !=null && DparchvCphAddedOnDate.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphAddedOnDate.Count;
				if (DparchvCphAddedOnTime !=null && DparchvCphAddedOnTime.Count > numDparchvCrsPlaceholders) numDparchvCrsPlaceholders = DparchvCphAddedOnTime.Count;

				for (int i = 0; i < numDparchvCrsPlaceholders; i++)
				{

					string value0 = "";
					if (DparchvCphId != null && i < DparchvCphId.Count)
					{
						value0 = DparchvCphId[i];
					}


					string value1 = "";
					if (DparchvCphTitle != null && i < DparchvCphTitle.Count)
					{
						value1 = DparchvCphTitle[i];
					}


					string value2 = "";
					if (DparchvCphCredits != null && i < DparchvCphCredits.Count)
					{
						value2 = DparchvCphCredits[i];
					}


					string value3 = "";
					if (DparchvCphAcadReqmt != null && i < DparchvCphAcadReqmt.Count)
					{
						value3 = DparchvCphAcadReqmt[i];
					}


					string value4 = "";
					if (DparchvCphSreqAcadReqmt != null && i < DparchvCphSreqAcadReqmt.Count)
					{
						value4 = DparchvCphSreqAcadReqmt[i];
					}


					string value5 = "";
					if (DparchvCphGroupAcadReqmt != null && i < DparchvCphGroupAcadReqmt.Count)
					{
						value5 = DparchvCphGroupAcadReqmt[i];
					}


					string value6 = "";
					if (DparchvCphTerm != null && i < DparchvCphTerm.Count)
					{
						value6 = DparchvCphTerm[i];
					}


					string value7 = "";
					if (DparchvCphAddedBy != null && i < DparchvCphAddedBy.Count)
					{
						value7 = DparchvCphAddedBy[i];
					}


					DateTime? value8 = null;
					if (DparchvCphAddedOnDate != null && i < DparchvCphAddedOnDate.Count)
					{
						value8 = DparchvCphAddedOnDate[i];
					}


					DateTime? value9 = null;
					if (DparchvCphAddedOnTime != null && i < DparchvCphAddedOnTime.Count)
					{
						value9 = DparchvCphAddedOnTime[i];
					}

					DparchvCrsPlaceholdersEntityAssociation.Add(new DegreePlanArchiveDparchvCrsPlaceholders( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class DegreePlanArchiveDparchvPrograms
	{
		public string DparchvPgmAcadProgramIdAssocMember;	
		public string DparchvPgmCatalogAssocMember;	
		public DegreePlanArchiveDparchvPrograms() {}
		public DegreePlanArchiveDparchvPrograms(
			string inDparchvPgmAcadProgramId,
			string inDparchvPgmCatalog)
		{
			DparchvPgmAcadProgramIdAssocMember = inDparchvPgmAcadProgramId;
			DparchvPgmCatalogAssocMember = inDparchvPgmCatalog;
		}
	}
	
	[Serializable]
	public class DegreePlanArchiveDparchvCourses
	{
		public string DparchvCrsCourseIdAssocMember;	
		public string DparchvCrsTermIdAssocMember;	
		public string DparchvCrsSectionIdAssocMember;	
		public string DparchvCrsNameAssocMember;	
		public string DparchvCrsTitleAssocMember;	
		public Decimal? DparchvCrsCreditsAssocMember;	
		public string DparchvCrsApprovalStatusAssocMember;	
		public string DparchvCrsStatusByAssocMember;	
		public DateTime? DparchvCrsStatusDateAssocMember;	
		public DateTime? DparchvCrsStatusTimeAssocMember;	
		public string DparchvCrsAddedByAssocMember;	
		public DateTime? DparchvCrsAddedOnDateAssocMember;	
		public DateTime? DparchvCrsAddedOnTimeAssocMember;	
		public Decimal? DparchvCrsCeusAssocMember;	
		public string DparchvCrsStcStatusAssocMember;	
		public string DparchvCrsIsPlannedAssocMember;	
		public string DparchvCrsHasWithdrawGrdAssocMember;	
		public DegreePlanArchiveDparchvCourses() {}
		public DegreePlanArchiveDparchvCourses(
			string inDparchvCrsCourseId,
			string inDparchvCrsTermId,
			string inDparchvCrsSectionId,
			string inDparchvCrsName,
			string inDparchvCrsTitle,
			Decimal? inDparchvCrsCredits,
			string inDparchvCrsApprovalStatus,
			string inDparchvCrsStatusBy,
			DateTime? inDparchvCrsStatusDate,
			DateTime? inDparchvCrsStatusTime,
			string inDparchvCrsAddedBy,
			DateTime? inDparchvCrsAddedOnDate,
			DateTime? inDparchvCrsAddedOnTime,
			Decimal? inDparchvCrsCeus,
			string inDparchvCrsStcStatus,
			string inDparchvCrsIsPlanned,
			string inDparchvCrsHasWithdrawGrd)
		{
			DparchvCrsCourseIdAssocMember = inDparchvCrsCourseId;
			DparchvCrsTermIdAssocMember = inDparchvCrsTermId;
			DparchvCrsSectionIdAssocMember = inDparchvCrsSectionId;
			DparchvCrsNameAssocMember = inDparchvCrsName;
			DparchvCrsTitleAssocMember = inDparchvCrsTitle;
			DparchvCrsCreditsAssocMember = inDparchvCrsCredits;
			DparchvCrsApprovalStatusAssocMember = inDparchvCrsApprovalStatus;
			DparchvCrsStatusByAssocMember = inDparchvCrsStatusBy;
			DparchvCrsStatusDateAssocMember = inDparchvCrsStatusDate;
			DparchvCrsStatusTimeAssocMember = inDparchvCrsStatusTime;
			DparchvCrsAddedByAssocMember = inDparchvCrsAddedBy;
			DparchvCrsAddedOnDateAssocMember = inDparchvCrsAddedOnDate;
			DparchvCrsAddedOnTimeAssocMember = inDparchvCrsAddedOnTime;
			DparchvCrsCeusAssocMember = inDparchvCrsCeus;
			DparchvCrsStcStatusAssocMember = inDparchvCrsStcStatus;
			DparchvCrsIsPlannedAssocMember = inDparchvCrsIsPlanned;
			DparchvCrsHasWithdrawGrdAssocMember = inDparchvCrsHasWithdrawGrd;
		}
	}
	
	[Serializable]
	public class DegreePlanArchiveDparchvCrsPlaceholders
	{
		public string DparchvCphIdAssocMember;	
		public string DparchvCphTitleAssocMember;	
		public string DparchvCphCreditsAssocMember;	
		public string DparchvCphAcadReqmtAssocMember;	
		public string DparchvCphSreqAcadReqmtAssocMember;	
		public string DparchvCphGroupAcadReqmtAssocMember;	
		public string DparchvCphTermAssocMember;	
		public string DparchvCphAddedByAssocMember;	
		public DateTime? DparchvCphAddedOnDateAssocMember;	
		public DateTime? DparchvCphAddedOnTimeAssocMember;	
		public DegreePlanArchiveDparchvCrsPlaceholders() {}
		public DegreePlanArchiveDparchvCrsPlaceholders(
			string inDparchvCphId,
			string inDparchvCphTitle,
			string inDparchvCphCredits,
			string inDparchvCphAcadReqmt,
			string inDparchvCphSreqAcadReqmt,
			string inDparchvCphGroupAcadReqmt,
			string inDparchvCphTerm,
			string inDparchvCphAddedBy,
			DateTime? inDparchvCphAddedOnDate,
			DateTime? inDparchvCphAddedOnTime)
		{
			DparchvCphIdAssocMember = inDparchvCphId;
			DparchvCphTitleAssocMember = inDparchvCphTitle;
			DparchvCphCreditsAssocMember = inDparchvCphCredits;
			DparchvCphAcadReqmtAssocMember = inDparchvCphAcadReqmt;
			DparchvCphSreqAcadReqmtAssocMember = inDparchvCphSreqAcadReqmt;
			DparchvCphGroupAcadReqmtAssocMember = inDparchvCphGroupAcadReqmt;
			DparchvCphTermAssocMember = inDparchvCphTerm;
			DparchvCphAddedByAssocMember = inDparchvCphAddedBy;
			DparchvCphAddedOnDateAssocMember = inDparchvCphAddedOnDate;
			DparchvCphAddedOnTimeAssocMember = inDparchvCphAddedOnTime;
		}
	}
}