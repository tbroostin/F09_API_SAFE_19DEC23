//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/4/2017 2:32:58 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: SWVER
//     Application: HR
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

namespace Ellucian.Colleague.Data.HumanResources.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Swver")]
	[ColleagueDataContract(GeneratedDateTime = "10/4/2017 2:32:58 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "SWVER", EntityType = "PHYS")]
	public class Swver : IColleagueGuidEntity
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
		/// Record GUID
		/// </summary>
		[DataMember(Name = "RecordGuid")]
		public string RecordGuid { get; set; }

		/// <summary>
		/// Record Model Name
		/// </summary>
		[DataMember(Name = "RecordModelName")]
		public string RecordModelName { get; set; }	
		
		/// <summary>
		/// CDD Name: SWV.DESC
		/// </summary>
		[DataMember(Order = 0, Name = "SWV.DESC")]
		public string SwvDesc { get; set; }
		
		/// <summary>
		/// CDD Name: SWV.SWTABLES.ID
		/// </summary>
		[DataMember(Order = 2, Name = "SWV.SWTABLES.ID")]
		public string SwvSwtablesId { get; set; }
		
		/// <summary>
		/// CDD Name: SWV.WAGE.GRADE.STEP
		/// </summary>
		[DataMember(Order = 24, Name = "SWV.WAGE.GRADE.STEP")]
		public List<string> SwvWageGradeStep { get; set; }
		
		/// <summary>
		/// CDD Name: SWV.WAGE.AMOUNT
		/// </summary>
		[DataMember(Order = 25, Name = "SWV.WAGE.AMOUNT")]
		public List<string> SwvWageAmount { get; set; }
		
		/// <summary>
		/// CDD Name: SWV.START.DATE
		/// </summary>
		[DataMember(Order = 35, Name = "SWV.START.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SwvStartDate { get; set; }
		
		/// <summary>
		/// CDD Name: SWV.END.DATE
		/// </summary>
		[DataMember(Order = 36, Name = "SWV.END.DATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? SwvEndDate { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<SwverSwvWage> SwvWageEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: SWV.WAGE
			
			SwvWageEntityAssociation = new List<SwverSwvWage>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(SwvWageGradeStep != null)
			{
				int numSwvWage = SwvWageGradeStep.Count;
				if (SwvWageAmount !=null && SwvWageAmount.Count > numSwvWage) numSwvWage = SwvWageAmount.Count;

				for (int i = 0; i < numSwvWage; i++)
				{

					string value0 = "";
					if (SwvWageGradeStep != null && i < SwvWageGradeStep.Count)
					{
						value0 = SwvWageGradeStep[i];
					}


					string value1 = "";
					if (SwvWageAmount != null && i < SwvWageAmount.Count)
					{
						value1 = SwvWageAmount[i];
					}

					SwvWageEntityAssociation.Add(new SwverSwvWage( value0, value1));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class SwverSwvWage
	{
		public string SwvWageGradeStepAssocMember;	
		public string SwvWageAmountAssocMember;	
		public SwverSwvWage() {}
		public SwverSwvWage(
			string inSwvWageGradeStep,
			string inSwvWageAmount)
		{
			SwvWageGradeStepAssocMember = inSwvWageGradeStep;
			SwvWageAmountAssocMember = inSwvWageAmount;
		}
	}
}