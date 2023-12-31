//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/13/2020 2:40:09 PM by user jsullivan
//
//     Type: ENTITY
//     Entity: BUDGET.DEV.DEFAULTS
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "BudgetDevDefaults")]
	[ColleagueDataContract(GeneratedDateTime = "8/13/2020 2:40:09 PM", User = "jsullivan")]
	[EntityDataContract(EntityName = "BUDGET.DEV.DEFAULTS", EntityType = "PERM")]
	public class BudgetDevDefaults : IColleagueEntity
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
		/// CDD Name: BUD.DEV.BUDGET
		/// </summary>
		[DataMember(Order = 0, Name = "BUD.DEV.BUDGET")]
		public string BudDevBudget { get; set; }
		
		/// <summary>
		/// CDD Name: BUD.DEV.SHOW.NOTES
		/// </summary>
		[DataMember(Order = 1, Name = "BUD.DEV.SHOW.NOTES")]
		public string BudDevShowNotes { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}