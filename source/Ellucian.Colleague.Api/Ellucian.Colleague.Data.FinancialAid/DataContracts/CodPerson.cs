//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 11/20/2020 12:38:09 PM by user tylerchristiansen
//
//     Type: ENTITY
//     Entity: COD.PERSON
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

namespace Ellucian.Colleague.Data.FinancialAid.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CodPerson")]
	[ColleagueDataContract(GeneratedDateTime = "11/20/2020 12:38:09 PM", User = "tylerchristiansen")]
	[EntityDataContract(EntityName = "COD.PERSON", EntityType = "PHYS")]
	public class CodPerson : IColleagueEntity
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
		/// CDD Name: CODP.COLLEAGUE.ID
		/// </summary>
		[DataMember(Order = 4, Name = "CODP.COLLEAGUE.ID")]
		public string CodpColleagueId { get; set; }
		
		/// <summary>
		/// CDD Name: CODP.COD.MPN.IDS
		/// </summary>
		[DataMember(Order = 19, Name = "CODP.COD.MPN.IDS")]
		public List<string> CodpCodMpnIds { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}