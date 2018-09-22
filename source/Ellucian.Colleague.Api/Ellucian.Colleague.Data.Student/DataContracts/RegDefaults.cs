//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 4/23/2018 9:54:06 AM by user cindystair
//
//     Type: ENTITY
//     Entity: REG.DEFAULTS
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
	[DataContract(Name = "RegDefaults")]
	[ColleagueDataContract(GeneratedDateTime = "4/23/2018 9:54:06 AM", User = "cindystair")]
	[EntityDataContract(EntityName = "REG.DEFAULTS", EntityType = "PERM")]
	public class RegDefaults : IColleagueEntity
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
		/// CDD Name: RGD.PHONE.DROP.GRADE.SCHEME
		/// </summary>
		[DataMember(Order = 26, Name = "RGD.PHONE.DROP.GRADE.SCHEME")]
		public List<string> RgdPhoneDropGradeScheme { get; set; }
		
		/// <summary>
		/// CDD Name: RGD.PHONE.DROP.GRADE
		/// </summary>
		[DataMember(Order = 27, Name = "RGD.PHONE.DROP.GRADE")]
		public List<string> RgdPhoneDropGrade { get; set; }
		
		/// <summary>
		/// CDD Name: RGD.REQUIRE.ADD.AUTH.FLAG
		/// </summary>
		[DataMember(Order = 37, Name = "RGD.REQUIRE.ADD.AUTH.FLAG")]
		public string RgdRequireAddAuthFlag { get; set; }
		
		/// <summary>
		/// CDD Name: RGD.ADD.AUTH.START.OFFSET
		/// </summary>
		[DataMember(Order = 38, Name = "RGD.ADD.AUTH.START.OFFSET")]
		public int? RgdAddAuthStartOffset { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<RegDefaultsRgdPhoneDrops> RgdPhoneDropsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: RGD.PHONE.DROPS
			
			RgdPhoneDropsEntityAssociation = new List<RegDefaultsRgdPhoneDrops>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(RgdPhoneDropGradeScheme != null)
			{
				int numRgdPhoneDrops = RgdPhoneDropGradeScheme.Count;
				if (RgdPhoneDropGrade !=null && RgdPhoneDropGrade.Count > numRgdPhoneDrops) numRgdPhoneDrops = RgdPhoneDropGrade.Count;

				for (int i = 0; i < numRgdPhoneDrops; i++)
				{

					string value0 = "";
					if (RgdPhoneDropGradeScheme != null && i < RgdPhoneDropGradeScheme.Count)
					{
						value0 = RgdPhoneDropGradeScheme[i];
					}


					string value1 = "";
					if (RgdPhoneDropGrade != null && i < RgdPhoneDropGrade.Count)
					{
						value1 = RgdPhoneDropGrade[i];
					}

					RgdPhoneDropsEntityAssociation.Add(new RegDefaultsRgdPhoneDrops( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class RegDefaultsRgdPhoneDrops
	{
		public string RgdPhoneDropGradeSchemeAssocMember;	
		public string RgdPhoneDropGradeAssocMember;	
		public RegDefaultsRgdPhoneDrops() {}
		public RegDefaultsRgdPhoneDrops(
			string inRgdPhoneDropGradeScheme,
			string inRgdPhoneDropGrade)
		{
			RgdPhoneDropGradeSchemeAssocMember = inRgdPhoneDropGradeScheme;
			RgdPhoneDropGradeAssocMember = inRgdPhoneDropGrade;
		}
	}
}