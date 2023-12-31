//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 3:18:34 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: STUDENT.NON.COURSES
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "StudentNonCourses")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 3:18:34 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "STUDENT.NON.COURSES", EntityType = "PHYS")]
	public class StudentNonCourses : IColleagueGuidEntity
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
		/// CDD Name: STNC.PERSON.ID
		/// </summary>
		[DataMember(Order = 0, Name = "STNC.PERSON.ID")]
		public string StncPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.PCT
		/// </summary>
		[DataMember(Order = 4, Name = "STNC.PCT")]
		public int? StncPct { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SOURCE
		/// </summary>
		[DataMember(Order = 5, Name = "STNC.SOURCE")]
		public string StncSource { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.TEST.FORM.NAME
		/// </summary>
		[DataMember(Order = 7, Name = "STNC.TEST.FORM.NAME")]
		public string StncTestFormName { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.TEST.FORM.NO
		/// </summary>
		[DataMember(Order = 8, Name = "STNC.TEST.FORM.NO")]
		public string StncTestFormNo { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SUBCOMPONENTS
		/// </summary>
		[DataMember(Order = 9, Name = "STNC.SUBCOMPONENTS")]
		public List<string> StncSubcomponents { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SUBCOMPONENT.SCORES
		/// </summary>
		[DataMember(Order = 10, Name = "STNC.SUBCOMPONENT.SCORES")]
		public List<int?> StncSubcomponentScores { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.CATEGORY
		/// </summary>
		[DataMember(Order = 11, Name = "STNC.CATEGORY")]
		public string StncCategory { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.TITLE
		/// </summary>
		[DataMember(Order = 12, Name = "STNC.TITLE")]
		public string StncTitle { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SPECIAL.FACTORS
		/// </summary>
		[DataMember(Order = 13, Name = "STNC.SPECIAL.FACTORS")]
		public List<string> StncSpecialFactors { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.START.DATE
		/// </summary>
		[DataMember(Order = 22, Name = "STNC.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? StncStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.STATUS
		/// </summary>
		[DataMember(Order = 26, Name = "STNC.STATUS")]
		public string StncStatus { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.STATUS.DATE
		/// </summary>
		[DataMember(Order = 27, Name = "STNC.STATUS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? StncStatusDate { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.NON.COURSE
		/// </summary>
		[DataMember(Order = 28, Name = "STNC.NON.COURSE")]
		public string StncNonCourse { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SUBCOMPONENT.PCTS
		/// </summary>
		[DataMember(Order = 29, Name = "STNC.SUBCOMPONENT.PCTS")]
		public List<int?> StncSubcomponentPcts { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SUB.STUDENT.NCRS.IDS
		/// </summary>
		[DataMember(Order = 30, Name = "STNC.SUB.STUDENT.NCRS.IDS")]
		public List<string> StncSubStudentNcrsIds { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.PCT2
		/// </summary>
		[DataMember(Order = 44, Name = "STNC.PCT2")]
		public int? StncPct2 { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SUBCOMPONENT.PCTS2
		/// </summary>
		[DataMember(Order = 45, Name = "STNC.SUBCOMPONENT.PCTS2")]
		public List<int?> StncSubcomponentPcts2 { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SCORE.DEC
		/// </summary>
		[DataMember(Order = 46, Name = "STNC.SCORE.DEC")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? StncScoreDec { get; set; }
		
		/// <summary>
		/// CDD Name: STNC.SUBCOMP.SCORES.DEC
		/// </summary>
		[DataMember(Order = 47, Name = "STNC.SUBCOMP.SCORES.DEC")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> StncSubcompScoresDec { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<StudentNonCoursesNonCourseSubs> NonCourseSubsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: NON.COURSE.SUBS
			
			NonCourseSubsEntityAssociation = new List<StudentNonCoursesNonCourseSubs>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(StncSubcomponents != null)
			{
				int numNonCourseSubs = StncSubcomponents.Count;
				if (StncSubcomponentScores !=null && StncSubcomponentScores.Count > numNonCourseSubs) numNonCourseSubs = StncSubcomponentScores.Count;
				if (StncSubcomponentPcts !=null && StncSubcomponentPcts.Count > numNonCourseSubs) numNonCourseSubs = StncSubcomponentPcts.Count;
				if (StncSubcomponentPcts2 !=null && StncSubcomponentPcts2.Count > numNonCourseSubs) numNonCourseSubs = StncSubcomponentPcts2.Count;
				if (StncSubcompScoresDec !=null && StncSubcompScoresDec.Count > numNonCourseSubs) numNonCourseSubs = StncSubcompScoresDec.Count;

				for (int i = 0; i < numNonCourseSubs; i++)
				{

					string value0 = "";
					if (StncSubcomponents != null && i < StncSubcomponents.Count)
					{
						value0 = StncSubcomponents[i];
					}


					int? value1 = null;
					if (StncSubcomponentScores != null && i < StncSubcomponentScores.Count)
					{
						value1 = StncSubcomponentScores[i];
					}


					int? value2 = null;
					if (StncSubcomponentPcts != null && i < StncSubcomponentPcts.Count)
					{
						value2 = StncSubcomponentPcts[i];
					}


					int? value3 = null;
					if (StncSubcomponentPcts2 != null && i < StncSubcomponentPcts2.Count)
					{
						value3 = StncSubcomponentPcts2[i];
					}


					Decimal? value4 = null;
					if (StncSubcompScoresDec != null && i < StncSubcompScoresDec.Count)
					{
						value4 = StncSubcompScoresDec[i];
					}

					NonCourseSubsEntityAssociation.Add(new StudentNonCoursesNonCourseSubs( value0, value1, value2, value3, value4));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class StudentNonCoursesNonCourseSubs
	{
		public string StncSubcomponentsAssocMember;	
		public int? StncSubcomponentScoresAssocMember;	
		public int? StncSubcomponentPctsAssocMember;	
		public int? StncSubcomponentPcts2AssocMember;	
		public Decimal? StncSubcompScoresDecAssocMember;	
		public StudentNonCoursesNonCourseSubs() {}
		public StudentNonCoursesNonCourseSubs(
			string inStncSubcomponents,
			int? inStncSubcomponentScores,
			int? inStncSubcomponentPcts,
			int? inStncSubcomponentPcts2,
			Decimal? inStncSubcompScoresDec)
		{
			StncSubcomponentsAssocMember = inStncSubcomponents;
			StncSubcomponentScoresAssocMember = inStncSubcomponentScores;
			StncSubcomponentPctsAssocMember = inStncSubcomponentPcts;
			StncSubcomponentPcts2AssocMember = inStncSubcomponentPcts2;
			StncSubcompScoresDecAssocMember = inStncSubcompScoresDec;
		}
	}
}