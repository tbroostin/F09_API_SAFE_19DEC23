//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:53:06 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: DATE.AWARD
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
	[DataContract(Name = "DateAward")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:53:06 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "DATE.AWARD", EntityType = "PHYS")]
	public class DateAward : IColleagueEntity
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
		/// CDD Name: DAW.DATE.AWARD.DISB.IDS
		/// </summary>
		[DataMember(Order = 18, Name = "DAW.DATE.AWARD.DISB.IDS")]
		public List<string> DawDateAwardDisbIds { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}