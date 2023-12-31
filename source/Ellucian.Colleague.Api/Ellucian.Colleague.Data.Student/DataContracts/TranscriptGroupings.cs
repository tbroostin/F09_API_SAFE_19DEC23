//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 3/28/2019 5:00:18 PM by user pbhaumik2
//
//     Type: ENTITY
//     Entity: TRANSCRIPT.GROUPINGS
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
	[DataContract(Name = "TranscriptGroupings")]
	[ColleagueDataContract(GeneratedDateTime = "3/28/2019 5:00:18 PM", User = "pbhaumik2")]
	[EntityDataContract(EntityName = "TRANSCRIPT.GROUPINGS", EntityType = "PHYS")]
	public class TranscriptGroupings : IColleagueEntity
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
		/// CDD Name: TRGP.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "TRGP.DESC")]
		public string TrgpDesc { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.ACAD.LEVELS
		/// </summary>
		[DataMember(Order = 1, Name = "TRGP.ACAD.LEVELS")]
		public List<string> TrgpAcadLevels { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.DEPTS
		/// </summary>
		[DataMember(Order = 2, Name = "TRGP.DEPTS")]
		public List<string> TrgpDepts { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.DIVISIONS
		/// </summary>
		[DataMember(Order = 3, Name = "TRGP.DIVISIONS")]
		public List<string> TrgpDivisions { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.SCHOOLS
		/// </summary>
		[DataMember(Order = 4, Name = "TRGP.SCHOOLS")]
		public List<string> TrgpSchools { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.LOCATIONS
		/// </summary>
		[DataMember(Order = 5, Name = "TRGP.LOCATIONS")]
		public List<string> TrgpLocations { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.COURSE.LEVELS
		/// </summary>
		[DataMember(Order = 6, Name = "TRGP.COURSE.LEVELS")]
		public List<string> TrgpCourseLevels { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.CRED.TYPES
		/// </summary>
		[DataMember(Order = 7, Name = "TRGP.CRED.TYPES")]
		public List<string> TrgpCredTypes { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.INCL.NO.GRADES.FLAG
		/// </summary>
		[DataMember(Order = 15, Name = "TRGP.INCL.NO.GRADES.FLAG")]
		public string TrgpInclNoGradesFlag { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.SUBJECTS
		/// </summary>
		[DataMember(Order = 22, Name = "TRGP.SUBJECTS")]
		public List<string> TrgpSubjects { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.ACAD.CRED.MARKS
		/// </summary>
		[DataMember(Order = 27, Name = "TRGP.ACAD.CRED.MARKS")]
		public List<string> TrgpAcadCredMarks { get; set; }
		
		/// <summary>
		/// CDD Name: TRGP.ADDNL.SELECT.CRITERIA
		/// </summary>
		[DataMember(Order = 40, Name = "TRGP.ADDNL.SELECT.CRITERIA")]
		public string TrgpAddnlSelectCriteria { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}