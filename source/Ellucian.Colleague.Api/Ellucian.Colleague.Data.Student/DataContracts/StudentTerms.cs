//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/7/2022 5:53:11 PM by user pbhaumik2
//
//     Type: ENTITY
//     Entity: STUDENT.TERMS
//     Application: ST
//     Environment: devcoll
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
	[DataContract(Name = "StudentTerms")]
	[ColleagueDataContract(GeneratedDateTime = "7/7/2022 5:53:11 PM", User = "pbhaumik2")]
	[EntityDataContract(EntityName = "STUDENT.TERMS", EntityType = "PHYS")]
	public class StudentTerms : IColleagueGuidEntity
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
		/// CDD Name: STTR.STUDENT.LOAD
		/// </summary>
		[DataMember(Order = 0, Name = "STTR.STUDENT.LOAD")]
		public string SttrStudentLoad { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.REG.DATE
		/// </summary>
		[DataMember(Order = 1, Name = "STTR.REG.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SttrRegDate { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.PREREG.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "STTR.PREREG.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SttrPreregDate { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.COURSE.SEC
		/// </summary>
		[DataMember(Order = 4, Name = "STTR.COURSE.SEC")]
		public List<string> SttrCourseSec { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.SCHEDULE
		/// </summary>
		[DataMember(Order = 5, Name = "STTR.SCHEDULE")]
		public List<string> SttrSchedule { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.RANDOM.ID
		/// </summary>
		[DataMember(Order = 7, Name = "STTR.RANDOM.ID")]
		public long? SttrRandomId { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.STUDENT.ACAD.CRED
		/// </summary>
		[DataMember(Order = 8, Name = "STTR.STUDENT.ACAD.CRED")]
		public List<string> SttrStudentAcadCred { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.STATUS
		/// </summary>
		[DataMember(Order = 14, Name = "STTR.STATUS")]
		public List<string> SttrStatus { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.STATUS.DATE
		/// </summary>
		[DataMember(Order = 15, Name = "STTR.STATUS.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> SttrStatusDate { get; set; }
		
		/// <summary>
		/// CDD Name: STTR.MID.RANDOM.ID
		/// </summary>
		[DataMember(Order = 93, Name = "STTR.MID.RANDOM.ID")]
		public long? SttrMidRandomId { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<StudentTermsSttrStatuses> SttrStatusesEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: STTR.STATUSES
			
			SttrStatusesEntityAssociation = new List<StudentTermsSttrStatuses>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SttrStatus != null)
			{
				int numSttrStatuses = SttrStatus.Count;
				if (SttrStatusDate !=null && SttrStatusDate.Count > numSttrStatuses) numSttrStatuses = SttrStatusDate.Count;

				for (int i = 0; i < numSttrStatuses; i++)
				{

					string value0 = "";
					if (SttrStatus != null && i < SttrStatus.Count)
					{
						value0 = SttrStatus[i];
					}


					DateTime? value1 = null;
					if (SttrStatusDate != null && i < SttrStatusDate.Count)
					{
						value1 = SttrStatusDate[i];
					}

					SttrStatusesEntityAssociation.Add(new StudentTermsSttrStatuses( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class StudentTermsSttrStatuses
	{
		public string SttrStatusAssocMember;	
		public DateTime? SttrStatusDateAssocMember;	
		public StudentTermsSttrStatuses() {}
		public StudentTermsSttrStatuses(
			string inSttrStatus,
			DateTime? inSttrStatusDate)
		{
			SttrStatusAssocMember = inSttrStatus;
			SttrStatusDateAssocMember = inSttrStatusDate;
		}
	}
}