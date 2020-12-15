//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 6/22/2020 5:21:15 PM by user bsf1
//
//     Type: ENTITY
//     Entity: EDM.EXT.VERSIONS
//     Application: CORE
//     Environment: dvcoll-2019
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

namespace Ellucian.Web.Http.Configuration.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "EdmExtVersions")]
	[ColleagueDataContract(GeneratedDateTime = "6/22/2020 5:21:15 PM", User = "bsf1")]
	[EntityDataContract(EntityName = "EDM.EXT.VERSIONS", EntityType = "PHYS")]
	public class EdmExtVersions : IColleagueEntity
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
		/// CDD Name: EDMV.DESCRIPTION
		/// </summary>
		[DataMember(Order = 0, Name = "EDMV.DESCRIPTION")]
		public string EdmvDescription { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.RESOURCE.NAME
		/// </summary>
		[DataMember(Order = 1, Name = "EDMV.RESOURCE.NAME")]
		public string EdmvResourceName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.VERSION.NUMBER
		/// </summary>
		[DataMember(Order = 4, Name = "EDMV.VERSION.NUMBER")]
		public string EdmvVersionNumber { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.EXTENDED.SCHEMA.TYPE
		/// </summary>
		[DataMember(Order = 8, Name = "EDMV.EXTENDED.SCHEMA.TYPE")]
		public string EdmvExtendedSchemaType { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.EXTENSION
		/// </summary>
		[DataMember(Order = 14, Name = "EDMV.EXTENSION")]
		public string EdmvExtension { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.INQUIRY.FIELDS
		/// </summary>
		[DataMember(Order = 19, Name = "EDMV.INQUIRY.FIELDS")]
		public List<string> EdmvInquiryFields { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.VERSION.STATUS
		/// </summary>
		[DataMember(Order = 37, Name = "EDMV.VERSION.STATUS")]
		public string EdmvVersionStatus { get; set; }
		
		/// <summary>
		/// CDD Name: EDMV.NAMED.QUERIES
		/// </summary>
		[DataMember(Order = 41, Name = "EDMV.NAMED.QUERIES")]
		public List<string> EdmvNamedQueries { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}