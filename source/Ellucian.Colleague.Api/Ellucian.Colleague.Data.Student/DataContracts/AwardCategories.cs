//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 9/10/2018 1:58:44 PM by user balvano3
//
//     Type: ENTITY
//     Entity: AWARD.CATEGORIES
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
	[DataContract(Name = "AwardCategories")]
	[ColleagueDataContract(GeneratedDateTime = "9/10/2018 1:58:44 PM", User = "balvano3")]
	[EntityDataContract(EntityName = "AWARD.CATEGORIES", EntityType = "PHYS")]
	public class AwardCategories : IColleagueGuidEntity
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
		/// CDD Name: AC.DESCRIPTION
		/// </summary>
		[DataMember(Order = 0, Name = "AC.DESCRIPTION")]
		public string AcDescription { get; set; }
		
		/// <summary>
		/// CDD Name: AC.LOAN.FLAG
		/// </summary>
		[DataMember(Order = 3, Name = "AC.LOAN.FLAG")]
		public string AcLoanFlag { get; set; }
		
		/// <summary>
		/// CDD Name: AC.GRANT.FLAG
		/// </summary>
		[DataMember(Order = 7, Name = "AC.GRANT.FLAG")]
		public string AcGrantFlag { get; set; }
		
		/// <summary>
		/// CDD Name: AC.WORK.FLAG
		/// </summary>
		[DataMember(Order = 8, Name = "AC.WORK.FLAG")]
		public string AcWorkFlag { get; set; }
		
		/// <summary>
		/// CDD Name: AC.SCHOLARSHIP.FLAG
		/// </summary>
		[DataMember(Order = 9, Name = "AC.SCHOLARSHIP.FLAG")]
		public string AcScholarshipFlag { get; set; }
		
		/// <summary>
		/// CDD Name: AC.INTG.NAME
		/// </summary>
		[DataMember(Order = 14, Name = "AC.INTG.NAME")]
		public string AcIntgName { get; set; }
		
		/// <summary>
		/// CDD Name: AC.INTG.RESTRICTED
		/// </summary>
		[DataMember(Order = 15, Name = "AC.INTG.RESTRICTED")]
		public string AcIntgRestricted { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}