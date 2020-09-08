//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 1/15/2020 11:15:19 AM by user cindystair
//
//     Type: ENTITY
//     Entity: OFFICE.COLLECTION.MAP
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
	[DataContract(Name = "OfficeCollectionMap")]
	[ColleagueDataContract(GeneratedDateTime = "1/15/2020 11:15:19 AM", User = "cindystair")]
	[EntityDataContract(EntityName = "OFFICE.COLLECTION.MAP", EntityType = "PERM")]
	public class OfficeCollectionMap : IColleagueEntity
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
		/// CDD Name: OFCO.DEFAULT.COLLECTION
		/// </summary>
		[DataMember(Order = 0, Name = "OFCO.DEFAULT.COLLECTION")]
		public string OfcoDefaultCollection { get; set; }
		
		/// <summary>
		/// CDD Name: OFCO.OFFICE.CODES
		/// </summary>
		[DataMember(Order = 1, Name = "OFCO.OFFICE.CODES")]
		public List<string> OfcoOfficeCodes { get; set; }
		
		/// <summary>
		/// CDD Name: OFCO.COLLECTION.IDS
		/// </summary>
		[DataMember(Order = 2, Name = "OFCO.COLLECTION.IDS")]
		public List<string> OfcoCollectionIds { get; set; }
		
		/// <summary>
		/// CDD Name: OFFICE.COLLECTION.MAP.CHGOPR
		/// </summary>
		[DataMember(Order = 3, Name = "OFFICE.COLLECTION.MAP.CHGOPR")]
		public string OfficeCollectionMapChgopr { get; set; }
		
		/// <summary>
		/// CDD Name: OFFICE.COLLECTION.MAP.CHGDAT
		/// </summary>
		[DataMember(Order = 4, Name = "OFFICE.COLLECTION.MAP.CHGDAT")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? OfficeCollectionMapChgdat { get; set; }
		
		/// <summary>
		/// CDD Name: OFCO.DFLT.OFFICE.COLLECTION
		/// </summary>
		[DataMember(Order = 5, Name = "OFCO.DFLT.OFFICE.COLLECTION")]
		public string OfcoDfltOfficeCollection { get; set; }
		
		/// <summary>
		/// CDD Name: OFCO.OFFICE.EMAIL
		/// </summary>
		[DataMember(Order = 6, Name = "OFCO.OFFICE.EMAIL")]
		public List<string> OfcoOfficeEmail { get; set; }
		
		/// <summary>
		/// CDD Name: OFCO.DEFAULT.EMAIL
		/// </summary>
		[DataMember(Order = 7, Name = "OFCO.DEFAULT.EMAIL")]
		public string OfcoDefaultEmail { get; set; }
		
		/// <summary>
		/// CDD Name: OFCO.DFLT.OFFICE.EMAIL
		/// </summary>
		[DataMember(Order = 8, Name = "OFCO.DFLT.OFFICE.EMAIL")]
		public string OfcoDfltOfficeEmail { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<OfficeCollectionMapOfcomap> OfcomapEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: OFCOMAP
			
			OfcomapEntityAssociation = new List<OfficeCollectionMapOfcomap>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(OfcoOfficeCodes != null)
			{
				int numOfcomap = OfcoOfficeCodes.Count;
				if (OfcoCollectionIds !=null && OfcoCollectionIds.Count > numOfcomap) numOfcomap = OfcoCollectionIds.Count;
				if (OfcoOfficeEmail !=null && OfcoOfficeEmail.Count > numOfcomap) numOfcomap = OfcoOfficeEmail.Count;

				for (int i = 0; i < numOfcomap; i++)
				{

					string value0 = "";
					if (OfcoOfficeCodes != null && i < OfcoOfficeCodes.Count)
					{
						value0 = OfcoOfficeCodes[i];
					}


					string value1 = "";
					if (OfcoCollectionIds != null && i < OfcoCollectionIds.Count)
					{
						value1 = OfcoCollectionIds[i];
					}


					string value2 = "";
					if (OfcoOfficeEmail != null && i < OfcoOfficeEmail.Count)
					{
						value2 = OfcoOfficeEmail[i];
					}

					OfcomapEntityAssociation.Add(new OfficeCollectionMapOfcomap( value0, value1, value2));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class OfficeCollectionMapOfcomap
	{
		public string OfcoOfficeCodesAssocMember;	
		public string OfcoCollectionIdsAssocMember;	
		public string OfcoOfficeEmailAssocMember;	
		public OfficeCollectionMapOfcomap() {}
		public OfficeCollectionMapOfcomap(
			string inOfcoOfficeCodes,
			string inOfcoCollectionIds,
			string inOfcoOfficeEmail)
		{
			OfcoOfficeCodesAssocMember = inOfcoOfficeCodes;
			OfcoCollectionIdsAssocMember = inOfcoCollectionIds;
			OfcoOfficeEmailAssocMember = inOfcoOfficeEmail;
		}
	}
}