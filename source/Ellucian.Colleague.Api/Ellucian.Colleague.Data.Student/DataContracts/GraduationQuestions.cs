//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 3:12:14 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: GRADUATION.QUESTIONS
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
	[DataContract(Name = "GraduationQuestions")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 3:12:14 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "GRADUATION.QUESTIONS", EntityType = "PHYS")]
	public class GraduationQuestions : IColleagueEntity
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
		/// CDD Name: GRADQ.HIDE
		/// </summary>
		[DataMember(Order = 0, Name = "GRADQ.HIDE")]
		public string GradqHide { get; set; }
		
		/// <summary>
		/// CDD Name: GRADQ.IS.REQUIRED
		/// </summary>
		[DataMember(Order = 1, Name = "GRADQ.IS.REQUIRED")]
		public string GradqIsRequired { get; set; }
		
		/// <summary>
		/// CDD Name: GRADQ.TEXT
		/// </summary>
		[DataMember(Order = 3, Name = "GRADQ.TEXT")]
		public string GradqText { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}