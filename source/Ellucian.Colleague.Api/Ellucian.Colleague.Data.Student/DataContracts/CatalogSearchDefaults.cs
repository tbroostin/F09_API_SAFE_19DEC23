//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 2/12/2020 4:24:18 PM by user sobel
//
//     Type: ENTITY
//     Entity: CATALOG.SEARCH.DEFAULTS
//     Application: ST
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

namespace Ellucian.Colleague.Data.Student.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CatalogSearchDefaults")]
	[ColleagueDataContract(GeneratedDateTime = "2/12/2020 4:24:18 PM", User = "sobel")]
	[EntityDataContract(EntityName = "CATALOG.SEARCH.DEFAULTS", EntityType = "PERM")]
	public class CatalogSearchDefaults : IColleagueEntity
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
		/// CDD Name: CLSD.SEARCH.ELEMENT
		/// </summary>
		[DataMember(Order = 0, Name = "CLSD.SEARCH.ELEMENT")]
		public List<string> ClsdSearchElement { get; set; }
		
		/// <summary>
		/// CDD Name: CLSD.HIDE
		/// </summary>
		[DataMember(Order = 1, Name = "CLSD.HIDE")]
		public List<string> ClsdHide { get; set; }
		
		/// <summary>
		/// CDD Name: CLSD.CE.SEARCH.ELEMENT
		/// </summary>
		[DataMember(Order = 2, Name = "CLSD.CE.SEARCH.ELEMENT")]
		public List<string> ClsdCeSearchElement { get; set; }
		
		/// <summary>
		/// CDD Name: CLSD.CE.HIDE
		/// </summary>
		[DataMember(Order = 3, Name = "CLSD.CE.HIDE")]
		public List<string> ClsdCeHide { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CatalogSearchDefaultsClsdSearchElements> ClsdSearchElementsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CatalogSearchDefaultsClsdCeSearchElements> ClsdCeSearchElementsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: CLSD.SEARCH.ELEMENTS
			
			ClsdSearchElementsEntityAssociation = new List<CatalogSearchDefaultsClsdSearchElements>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(ClsdSearchElement != null)
			{
				int numClsdSearchElements = ClsdSearchElement.Count;
				if (ClsdHide !=null && ClsdHide.Count > numClsdSearchElements) numClsdSearchElements = ClsdHide.Count;

				for (int i = 0; i < numClsdSearchElements; i++)
				{

					string value0 = "";
					if (ClsdSearchElement != null && i < ClsdSearchElement.Count)
					{
						value0 = ClsdSearchElement[i];
					}


					string value1 = "";
					if (ClsdHide != null && i < ClsdHide.Count)
					{
						value1 = ClsdHide[i];
					}

					ClsdSearchElementsEntityAssociation.Add(new CatalogSearchDefaultsClsdSearchElements( value0, value1));
				}
			}
			// EntityAssociation Name: CLSD.CE.SEARCH.ELEMENTS
			
			ClsdCeSearchElementsEntityAssociation = new List<CatalogSearchDefaultsClsdCeSearchElements>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(ClsdCeSearchElement != null)
			{
				int numClsdCeSearchElements = ClsdCeSearchElement.Count;
				if (ClsdCeHide !=null && ClsdCeHide.Count > numClsdCeSearchElements) numClsdCeSearchElements = ClsdCeHide.Count;

				for (int i = 0; i < numClsdCeSearchElements; i++)
				{

					string value0 = "";
					if (ClsdCeSearchElement != null && i < ClsdCeSearchElement.Count)
					{
						value0 = ClsdCeSearchElement[i];
					}


					string value1 = "";
					if (ClsdCeHide != null && i < ClsdCeHide.Count)
					{
						value1 = ClsdCeHide[i];
					}

					ClsdCeSearchElementsEntityAssociation.Add(new CatalogSearchDefaultsClsdCeSearchElements( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class CatalogSearchDefaultsClsdSearchElements
	{
		public string ClsdSearchElementAssocMember;	
		public string ClsdHideAssocMember;	
		public CatalogSearchDefaultsClsdSearchElements() {}
		public CatalogSearchDefaultsClsdSearchElements(
			string inClsdSearchElement,
			string inClsdHide)
		{
			ClsdSearchElementAssocMember = inClsdSearchElement;
			ClsdHideAssocMember = inClsdHide;
		}
	}
	
	[Serializable]
	public class CatalogSearchDefaultsClsdCeSearchElements
	{
		public string ClsdCeSearchElementAssocMember;	
		public string ClsdCeHideAssocMember;	
		public CatalogSearchDefaultsClsdCeSearchElements() {}
		public CatalogSearchDefaultsClsdCeSearchElements(
			string inClsdCeSearchElement,
			string inClsdCeHide)
		{
			ClsdCeSearchElementAssocMember = inClsdCeSearchElement;
			ClsdCeHideAssocMember = inClsdCeHide;
		}
	}
}