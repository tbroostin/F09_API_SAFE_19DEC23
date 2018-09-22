//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 6/25/2018 1:28:41 PM by user sbhole1
//
//     Type: ENTITY
//     Entity: FA.SAP.STATUS.INFO
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "FaSapStatusInfo")]
	[ColleagueDataContract(GeneratedDateTime = "6/25/2018 1:28:41 PM", User = "sbhole1")]
	[EntityDataContract(EntityName = "FA.SAP.STATUS.INFO", EntityType = "PHYS")]
	public class FaSapStatusInfo : IColleagueEntity
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
		/// CDD Name: FSSI.STATUS
		/// </summary>
		[DataMember(Order = 0, Name = "FSSI.STATUS")]
		public string FssiStatus { get; set; }
		
		/// <summary>
		/// CDD Name: FSSI.DESCRIPTION
		/// </summary>
		[DataMember(Order = 1, Name = "FSSI.DESCRIPTION")]
		public string FssiDescription { get; set; }
		
		/// <summary>
		/// CDD Name: FSSI.CATEGORY
		/// </summary>
		[DataMember(Order = 2, Name = "FSSI.CATEGORY")]
		public string FssiCategory { get; set; }
		
		/// <summary>
		/// CDD Name: FSSI.EXPLANATION
		/// </summary>
		[DataMember(Order = 3, Name = "FSSI.EXPLANATION")]
		public string FssiExplanation { get; set; }
		
		/// <summary>
		/// CDD Name: FSSI.EXPLAINED
		/// </summary>
		[DataMember(Order = 4, Name = "FSSI.EXPLAINED")]
		public string FssiExplained { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}