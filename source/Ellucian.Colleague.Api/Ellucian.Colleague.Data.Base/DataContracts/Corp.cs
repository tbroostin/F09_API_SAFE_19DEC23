//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:27:42 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: CORP
//     Application: CORE
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Corp")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:27:42 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "CORP", EntityType = "LOGI")]
	public class Corp : IColleagueEntity
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
		/// CDD Name: CORP.NAME
		/// </summary>
		[DataMember(Order = 14, Name = "PREFERRED.NAME")]
		public List<string> CorpName { get; set; }
		
		/// <summary>
		/// CDD Name: CORP.PARENTS
		/// </summary>
		[DataMember(Order = 47, Name = "PARENTS")]
		public List<string> CorpParents { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}