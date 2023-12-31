//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/30/2017 10:47:01 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: INSTALLED.APPLS
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
	[DataContract(Name = "InstalledAppls")]
	[ColleagueDataContract(GeneratedDateTime = "10/30/2017 10:47:01 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "INSTALLED.APPLS", EntityType = "PERM")]
	public class InstalledAppls : IColleagueEntity
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
		/// CDD Name: IA.MODULE.NAMES
		/// </summary>
		[DataMember(Order = 7, Name = "IA.MODULE.NAMES")]
		public List<string> IaModuleNames { get; set; }
		
		/// <summary>
		/// CDD Name: IA.MODULE.STAMP
		/// </summary>
		[DataMember(Order = 8, Name = "IA.MODULE.STAMP")]
		public List<string> IaModuleStamp { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<InstalledApplsIaModules> IaModulesEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: IA.MODULES
			
			IaModulesEntityAssociation = new List<InstalledApplsIaModules>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(IaModuleNames != null)
			{
				int numIaModules = IaModuleNames.Count;
				if (IaModuleStamp !=null && IaModuleStamp.Count > numIaModules) numIaModules = IaModuleStamp.Count;

				for (int i = 0; i < numIaModules; i++)
				{

					string value0 = "";
					if (IaModuleNames != null && i < IaModuleNames.Count)
					{
						value0 = IaModuleNames[i];
					}


					string value1 = "";
					if (IaModuleStamp != null && i < IaModuleStamp.Count)
					{
						value1 = IaModuleStamp[i];
					}

					IaModulesEntityAssociation.Add(new InstalledApplsIaModules( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class InstalledApplsIaModules
	{
		public string IaModuleNamesAssocMember;	
		public string IaModuleStampAssocMember;	
		public InstalledApplsIaModules() {}
		public InstalledApplsIaModules(
			string inIaModuleNames,
			string inIaModuleStamp)
		{
			IaModuleNamesAssocMember = inIaModuleNames;
			IaModuleStampAssocMember = inIaModuleStamp;
		}
	}
}