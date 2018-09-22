//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/10/2017 7:47:56 AM by user bsf1
//
//     Type: ENTITY
//     Entity: ACAD.CREDENTIALS
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "AcadCredentials")]
	[ColleagueDataContract(GeneratedDateTime = "10/10/2017 7:47:56 AM", User = "bsf1")]
	[EntityDataContract(EntityName = "ACAD.CREDENTIALS", EntityType = "PHYS")]
	public class AcadCredentials : IColleagueGuidEntity
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
		/// CDD Name: ACAD.PERSON.ID
		/// </summary>
		[DataMember(Order = 0, Name = "ACAD.PERSON.ID")]
		public string AcadPersonId { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.INSTITUTIONS.ID
		/// </summary>
		[DataMember(Order = 1, Name = "ACAD.INSTITUTIONS.ID")]
		public string AcadInstitutionsId { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.START.DATE
		/// </summary>
		[DataMember(Order = 2, Name = "ACAD.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? AcadStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.END.DATE
		/// </summary>
		[DataMember(Order = 3, Name = "ACAD.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? AcadEndDate { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.DEGREE
		/// </summary>
		[DataMember(Order = 4, Name = "ACAD.DEGREE")]
		public string AcadDegree { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.DEGREE.DATE
		/// </summary>
		[DataMember(Order = 5, Name = "ACAD.DEGREE.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? AcadDegreeDate { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.CCD
		/// </summary>
		[DataMember(Order = 6, Name = "ACAD.CCD")]
		public List<string> AcadCcd { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.CCD.DATE
		/// </summary>
		[DataMember(Order = 7, Name = "ACAD.CCD.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<DateTime?> AcadCcdDate { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.MAJORS
		/// </summary>
		[DataMember(Order = 8, Name = "ACAD.MAJORS")]
		public List<string> AcadMajors { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.MINORS
		/// </summary>
		[DataMember(Order = 9, Name = "ACAD.MINORS")]
		public List<string> AcadMinors { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.SPECIALIZATION
		/// </summary>
		[DataMember(Order = 10, Name = "ACAD.SPECIALIZATION")]
		public List<string> AcadSpecialization { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.HONORS
		/// </summary>
		[DataMember(Order = 11, Name = "ACAD.HONORS")]
		public List<string> AcadHonors { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.AWARDS
		/// </summary>
		[DataMember(Order = 12, Name = "ACAD.AWARDS")]
		public List<string> AcadAwards { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.NO.YEARS
		/// </summary>
		[DataMember(Order = 16, Name = "ACAD.NO.YEARS")]
		public int? AcadNoYears { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.COMMENCEMENT.DATE
		/// </summary>
		[DataMember(Order = 17, Name = "ACAD.COMMENCEMENT.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? AcadCommencementDate { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.GPA
		/// </summary>
		[DataMember(Order = 19, Name = "ACAD.GPA")]
		[DisplayFormat(DataFormatString = "{0:N3}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? AcadGpa { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.RANK.PERCENT
		/// </summary>
		[DataMember(Order = 22, Name = "ACAD.RANK.PERCENT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? AcadRankPercent { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.RANK.NUMERATOR
		/// </summary>
		[DataMember(Order = 23, Name = "ACAD.RANK.NUMERATOR")]
		public int? AcadRankNumerator { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.RANK.DENOMINATOR
		/// </summary>
		[DataMember(Order = 24, Name = "ACAD.RANK.DENOMINATOR")]
		public int? AcadRankDenominator { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.THESIS
		/// </summary>
		[DataMember(Order = 25, Name = "ACAD.THESIS")]
		public string AcadThesis { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.COMMENTS
		/// </summary>
		[DataMember(Order = 26, Name = "ACAD.COMMENTS")]
		public string AcadComments { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.ACAD.PROGRAM
		/// </summary>
		[DataMember(Order = 47, Name = "ACAD.ACAD.PROGRAM")]
		public string AcadAcadProgram { get; set; }
		
		/// <summary>
		/// CDD Name: ACAD.TERM
		/// </summary>
		[DataMember(Order = 58, Name = "ACAD.TERM")]
		public string AcadTerm { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}