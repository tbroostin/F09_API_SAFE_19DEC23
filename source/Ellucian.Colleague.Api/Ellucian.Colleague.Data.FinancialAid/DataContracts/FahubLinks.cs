//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:53:51 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: FAHUB.LINKS
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
	[DataContract(Name = "FahubLinks")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:53:51 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "FAHUB.LINKS", EntityType = "PHYS")]
	public class FahubLinks : IColleagueEntity
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
		/// CDD Name: FAHUB.LINK.TITLE
		/// </summary>
		[DataMember(Order = 0, Name = "FAHUB.LINK.TITLE")]
		public string FahubLinkTitle { get; set; }
		
		/// <summary>
		/// CDD Name: FAHUB.LINK.TYPE
		/// </summary>
		[DataMember(Order = 2, Name = "FAHUB.LINK.TYPE")]
		public string FahubLinkType { get; set; }
		
		/// <summary>
		/// CDD Name: FAHUB.LINK.URL
		/// </summary>
		[DataMember(Order = 3, Name = "FAHUB.LINK.URL")]
		public string FahubLinkUrl { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}