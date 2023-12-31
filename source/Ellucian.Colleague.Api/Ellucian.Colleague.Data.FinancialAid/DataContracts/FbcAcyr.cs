//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/14/2018 12:49:28 PM by user otorres
//
//     Type: ENTITY
//     Entity: FBC.ACYR
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

namespace Ellucian.Colleague.Data.FinancialAid.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "FbcAcyr")]
	[ColleagueDataContract(GeneratedDateTime = "12/14/2018 12:49:28 PM", User = "otorres")]
	[EntityDataContract(EntityName = "FBC.ACYR", EntityType = "PHYS")]
	public class FbcAcyr : IColleagueEntity
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
		/// CDD Name: FBC.DESC
		/// </summary>
		[DataMember(Order = 5, Name = "FBC.DESC")]
		public string FbcDesc { get; set; }
		
		/// <summary>
		/// CDD Name: FBC.SHOPSHEET.GROUP
		/// </summary>
		[DataMember(Order = 13, Name = "FBC.SHOPSHEET.GROUP")]
		public string FbcShopsheetGroup { get; set; }
		
		/// <summary>
		/// CDD Name: FBC.COST.INDICATOR
		/// </summary>
		[DataMember(Order = 14, Name = "FBC.COST.INDICATOR")]
		public string FbcCostIndicator { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}