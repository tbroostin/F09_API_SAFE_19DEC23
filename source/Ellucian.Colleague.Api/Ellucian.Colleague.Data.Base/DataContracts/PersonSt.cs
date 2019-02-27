//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/14/2018 4:27:09 PM by user sbhole1
//
//     Type: ENTITY
//     Entity: PERSON.ST
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "PersonSt")]
	[ColleagueDataContract(GeneratedDateTime = "8/14/2018 4:27:09 PM", User = "sbhole1")]
	[EntityDataContract(EntityName = "PERSON.ST", EntityType = "PHYS")]
	public class PersonSt : IColleagueGuidEntity
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
		/// CDD Name: PST.CAMPUS.ORGS.MEMBER
		/// </summary>
		[DataMember(Order = 1, Name = "PST.CAMPUS.ORGS.MEMBER")]
		public List<string> PstCampusOrgsMember { get; set; }
		
		/// <summary>
		/// CDD Name: PST.EDUC.GOALS
		/// </summary>
		[DataMember(Order = 4, Name = "PST.EDUC.GOALS")]
		public List<string> PstEducGoals { get; set; }
		
		/// <summary>
		/// CDD Name: PST.EDUC.GOALS.CHGDATES
		/// </summary>
		[DataMember(Order = 5, Name = "PST.EDUC.GOALS.CHGDATES")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> PstEducGoalsChgdates { get; set; }
		
		/// <summary>
		/// CDD Name: PST.STUDENT.ACAD.CRED
		/// </summary>
		[DataMember(Order = 7, Name = "PST.STUDENT.ACAD.CRED")]
		public List<string> PstStudentAcadCred { get; set; }
		
		/// <summary>
		/// CDD Name: PST.ADMISSIONS.TESTS
		/// </summary>
		[DataMember(Order = 10, Name = "PST.ADMISSIONS.TESTS")]
		public List<string> PstAdmissionsTests { get; set; }
		
		/// <summary>
		/// CDD Name: PST.PLACEMENT.TESTS
		/// </summary>
		[DataMember(Order = 11, Name = "PST.PLACEMENT.TESTS")]
		public List<string> PstPlacementTests { get; set; }
		
		/// <summary>
		/// CDD Name: PST.OTHER.TESTS
		/// </summary>
		[DataMember(Order = 12, Name = "PST.OTHER.TESTS")]
		public List<string> PstOtherTests { get; set; }
		
		/// <summary>
		/// CDD Name: PST.STUDENT.COURSE.SEC
		/// </summary>
		[DataMember(Order = 14, Name = "PST.STUDENT.COURSE.SEC")]
		public List<string> PstStudentCourseSec { get; set; }
		
		/// <summary>
		/// CDD Name: PST.RESTRICTIONS
		/// </summary>
		[DataMember(Order = 19, Name = "PST.RESTRICTIONS")]
		public List<string> PstRestrictions { get; set; }
		
		/// <summary>
		/// CDD Name: PST.ADVISEMENT
		/// </summary>
		[DataMember(Order = 20, Name = "PST.ADVISEMENT")]
		public List<string> PstAdvisement { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<PersonStEducGoals> EducGoalsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: EDUC.GOALS
			
			EducGoalsEntityAssociation = new List<PersonStEducGoals>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(PstEducGoals != null)
			{
				int numEducGoals = PstEducGoals.Count;
				if (PstEducGoalsChgdates !=null && PstEducGoalsChgdates.Count > numEducGoals) numEducGoals = PstEducGoalsChgdates.Count;

				for (int i = 0; i < numEducGoals; i++)
				{

					string value0 = "";
					if (PstEducGoals != null && i < PstEducGoals.Count)
					{
						value0 = PstEducGoals[i];
					}


					DateTime? value1 = null;
					if (PstEducGoalsChgdates != null && i < PstEducGoalsChgdates.Count)
					{
						value1 = PstEducGoalsChgdates[i];
					}

					EducGoalsEntityAssociation.Add(new PersonStEducGoals( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class PersonStEducGoals
	{
		public string PstEducGoalsAssocMember;	
		public DateTime? PstEducGoalsChgdatesAssocMember;	
		public PersonStEducGoals() {}
		public PersonStEducGoals(
			string inPstEducGoals,
			DateTime? inPstEducGoalsChgdates)
		{
			PstEducGoalsAssocMember = inPstEducGoals;
			PstEducGoalsChgdatesAssocMember = inPstEducGoalsChgdates;
		}
	}
}