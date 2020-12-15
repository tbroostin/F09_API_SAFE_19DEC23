//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/24/2020 5:23:50 PM by user dvcoll-srm
//
//     Type: ENTITY
//     Entity: EDM.CODE.HOOKS
//     Application: CORE
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
	[DataContract(Name = "EdmCodeHooks")]
	[ColleagueDataContract(GeneratedDateTime = "8/24/2020 5:23:50 PM", User = "dvcoll-srm")]
	[EntityDataContract(EntityName = "EDM.CODE.HOOKS", EntityType = "PHYS")]
	public class EdmCodeHooks : IColleagueEntity
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
		/// CDD Name: EDMC.TYPE
		/// </summary>
		[DataMember(Order = 0, Name = "EDMC.TYPE")]
		public string EdmcType { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.RESOURCE.NAME
		/// </summary>
		[DataMember(Order = 1, Name = "EDMC.RESOURCE.NAME")]
		public List<string> EdmcResourceName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.FILE.NAME
		/// </summary>
		[DataMember(Order = 2, Name = "EDMC.FILE.NAME")]
		public List<string> EdmcFileName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.FIELD.NAME
		/// </summary>
		[DataMember(Order = 3, Name = "EDMC.FIELD.NAME")]
		public List<string> EdmcFieldName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.ASSOC.NAME
		/// </summary>
		[DataMember(Order = 4, Name = "EDMC.ASSOC.NAME")]
		public List<string> EdmcAssocName { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.HOOK.CODE
		/// </summary>
		[DataMember(Order = 5, Name = "EDMC.HOOK.CODE")]
		public string EdmcHookCode { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.DESCRIPTION
		/// </summary>
		[DataMember(Order = 6, Name = "EDMC.DESCRIPTION")]
		public string EdmcDescription { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.NAMED.QUERY
		/// </summary>
		[DataMember(Order = 7, Name = "EDMC.NAMED.QUERY")]
		public List<string> EdmcNamedQuery { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.JSON.LABEL
		/// </summary>
		[DataMember(Order = 8, Name = "EDMC.JSON.LABEL")]
		public string EdmcJsonLabel { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.JSON.PATH
		/// </summary>
		[DataMember(Order = 9, Name = "EDMC.JSON.PATH")]
		public string EdmcJsonPath { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.JSON.PROPERTY.TYPE
		/// </summary>
		[DataMember(Order = 10, Name = "EDMC.JSON.PROPERTY.TYPE")]
		public string EdmcJsonPropertyType { get; set; }
		
		/// <summary>
		/// CDD Name: EDMC.RESOURCE.VERSION
		/// </summary>
		[DataMember(Order = 11, Name = "EDMC.RESOURCE.VERSION")]
		public List<string> EdmcResourceVersion { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<EdmCodeHooksEdmcResource> EdmcResourceEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: EDMC.RESOURCE
			
			EdmcResourceEntityAssociation = new List<EdmCodeHooksEdmcResource>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(EdmcResourceName != null)
			{
				int numEdmcResource = EdmcResourceName.Count;
				if (EdmcResourceVersion !=null && EdmcResourceVersion.Count > numEdmcResource) numEdmcResource = EdmcResourceVersion.Count;

				for (int i = 0; i < numEdmcResource; i++)
				{

					string value0 = "";
					if (EdmcResourceName != null && i < EdmcResourceName.Count)
					{
						value0 = EdmcResourceName[i];
					}


					string value1 = "";
					if (EdmcResourceVersion != null && i < EdmcResourceVersion.Count)
					{
						value1 = EdmcResourceVersion[i];
					}

					EdmcResourceEntityAssociation.Add(new EdmCodeHooksEdmcResource( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class EdmCodeHooksEdmcResource
	{
		public string EdmcResourceNameAssocMember;	
		public string EdmcResourceVersionAssocMember;	
		public EdmCodeHooksEdmcResource() {}
		public EdmCodeHooksEdmcResource(
			string inEdmcResourceName,
			string inEdmcResourceVersion)
		{
			EdmcResourceNameAssocMember = inEdmcResourceName;
			EdmcResourceVersionAssocMember = inEdmcResourceVersion;
		}
	}
}