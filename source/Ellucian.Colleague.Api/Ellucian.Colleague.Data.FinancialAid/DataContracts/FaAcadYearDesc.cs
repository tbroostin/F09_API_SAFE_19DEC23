//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:53:31 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: FA.ACAD.YEAR.DESC
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

namespace Ellucian.Colleague.Data.FinancialAid.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "FaAcadYearDesc")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:53:31 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "FA.ACAD.YEAR.DESC", EntityType = "PHYS")]
	public class FaAcadYearDesc : IColleagueEntity
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
		/// CDD Name: FAYD.YEAR
		/// </summary>
		[DataMember(Order = 0, Name = "FAYD.YEAR")]
		public string FaydYear { get; set; }
		
		/// <summary>
		/// CDD Name: FAYD.DESCRIPTION
		/// </summary>
		[DataMember(Order = 1, Name = "FAYD.DESCRIPTION")]
		public string FaydDescription { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}