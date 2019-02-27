//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/12/2018 9:44:02 AM by user bsf1
//
//     Type: ENTITY
//     Entity: HIST
//     Application: UT
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Hist")]
	[ColleagueDataContract(GeneratedDateTime = "10/12/2018 9:44:02 AM", User = "bsf1")]
	[EntityDataContract(EntityName = "HIST", EntityType = "PHYS")]
	public class Hist : IColleagueEntity
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
		/// CDD Name: HIST.FIELD.NAME
		/// </summary>
		[DataMember(Order = 0, Name = "HIST.FIELD.NAME")]
		public string HistFieldName { get; set; }
		
		/// <summary>
		/// CDD Name: HIST.OLD.VALUES
		/// </summary>
		[DataMember(Order = 1, Name = "HIST.OLD.VALUES")]
		public string HistOldValues { get; set; }
		
		/// <summary>
		/// CDD Name: HIST.NEW.VALUES
		/// </summary>
		[DataMember(Order = 2, Name = "HIST.NEW.VALUES")]
		public string HistNewValues { get; set; }
		
		/// <summary>
		/// CDD Name: HIST.LOG
		/// </summary>
		[DataMember(Order = 3, Name = "HIST.LOG")]
		public string HistLog { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}