//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 1:36:40 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: PPWEB.DEFAULTS
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

namespace Ellucian.Colleague.Data.Finance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "PpwebDefaults")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 1:36:40 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "PPWEB.DEFAULTS", EntityType = "PERM")]
	public class PpwebDefaults : IColleagueEntity
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
		/// CDD Name: PPWEB.PARTIAL.PMT.IND
		/// </summary>
		[DataMember(Order = 1, Name = "PPWEB.PARTIAL.PMT.IND")]
		public string PpwebPartialPmtInd { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}