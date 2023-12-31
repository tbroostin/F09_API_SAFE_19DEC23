//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 10:34:49 AM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: INTL.PARAMS
//     Application: UT
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
	[DataContract(Name = "IntlParams")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 10:34:49 AM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "INTL.PARAMS", EntityType = "PHYS")]
	public class IntlParams : IColleagueEntity
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
		/// CDD Name: HOST.COUNTRY
		/// </summary>
		[DataMember(Order = 0, Name = "HOST.COUNTRY")]
		public string HostCountry { get; set; }
		
		/// <summary>
		/// CDD Name: HOST.SHORT.DATE.FORMAT
		/// </summary>
		[DataMember(Order = 4, Name = "HOST.SHORT.DATE.FORMAT")]
		public string HostShortDateFormat { get; set; }
		
		/// <summary>
		/// CDD Name: HOST.DATE.DELIMITER
		/// </summary>
		[DataMember(Order = 6, Name = "HOST.DATE.DELIMITER")]
		public string HostDateDelimiter { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}