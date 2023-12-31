//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/12/2021 1:35:20 PM by user gtt_dvcoll_wstst01
//
//     Type: ENTITY
//     Entity: PARM.T4A
//     Application: CF
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
	[DataContract(Name = "ParmT4a")]
	[ColleagueDataContract(GeneratedDateTime = "5/12/2021 1:35:20 PM", User = "gtt_dvcoll_wstst01")]
	[EntityDataContract(EntityName = "PARM.T4A", EntityType = "PERM")]
	public class ParmT4a : IColleagueEntity
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
		/// CDD Name: PT4A.YEAR
		/// </summary>
		[DataMember(Order = 0, Name = "PT4A.YEAR")]
		public string Pt4aYear { get; set; }
		
		/// <summary>
		/// CDD Name: PT4A.BOX.NO.SUB
		/// </summary>
		[DataMember(Order = 19, Name = "PT4A.BOX.NO.SUB")]
		public List<string> Pt4aBoxNoSub { get; set; }
		
		/// <summary>
		/// CDD Name: PT4A.BOX.NO.SUB.DESC
		/// </summary>
		[DataMember(Order = 20, Name = "PT4A.BOX.NO.SUB.DESC")]
		public List<string> Pt4aBoxNoSubDesc { get; set; }
		
		/// <summary>
		/// CDD Name: PT4A.BOX.NO.SUB.MIN.BAL
		/// </summary>
		[DataMember(Order = 25, Name = "PT4A.BOX.NO.SUB.MIN.BAL")]
		public List<string> Pt4aBoxNoSubMinBal { get; set; }
		
		/// <summary>
		/// CDD Name: PT4A.NAME.ADDR.HIERARCHY
		/// </summary>
		[DataMember(Order = 28, Name = "PT4A.NAME.ADDR.HIERARCHY")]
		public string Pt4aNameAddrHierarchy { get; set; }
		
		/// <summary>
		/// CDD Name: PT4A.MIN.TOT.AMT
		/// </summary>
		[DataMember(Order = 29, Name = "PT4A.MIN.TOT.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? Pt4aMinTotAmt { get; set; }
		
		/// <summary>
		/// CDD Name: PT4A.CON.TEXT
		/// </summary>
		[DataMember(Order = 30, Name = "PT4A.CON.TEXT")]
		public string Pt4aConText { get; set; }
		
		/// <summary>
		/// CDD Name: PT4A.WHLD.TEXT
		/// </summary>
		[DataMember(Order = 31, Name = "PT4A.WHLD.TEXT")]
		public string Pt4aWhldText { get; set; }
		
		/// <summary>
		/// CDD Name: PT4A.HIDE.CONSENT.FLAG
		/// </summary>
		[DataMember(Order = 38, Name = "PT4A.HIDE.CONSENT.FLAG")]
		public string Pt4aHideConsentFlag { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<ParmT4aPt4aSub> Pt4aSubEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: PT4A.SUB
			
			Pt4aSubEntityAssociation = new List<ParmT4aPt4aSub>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(Pt4aBoxNoSub != null)
			{
				int numPt4aSub = Pt4aBoxNoSub.Count;
				if (Pt4aBoxNoSubDesc !=null && Pt4aBoxNoSubDesc.Count > numPt4aSub) numPt4aSub = Pt4aBoxNoSubDesc.Count;
				if (Pt4aBoxNoSubMinBal !=null && Pt4aBoxNoSubMinBal.Count > numPt4aSub) numPt4aSub = Pt4aBoxNoSubMinBal.Count;

				for (int i = 0; i < numPt4aSub; i++)
				{

					string value0 = "";
					if (Pt4aBoxNoSub != null && i < Pt4aBoxNoSub.Count)
					{
						value0 = Pt4aBoxNoSub[i];
					}


					string value1 = "";
					if (Pt4aBoxNoSubDesc != null && i < Pt4aBoxNoSubDesc.Count)
					{
						value1 = Pt4aBoxNoSubDesc[i];
					}


					string value2 = "";
					if (Pt4aBoxNoSubMinBal != null && i < Pt4aBoxNoSubMinBal.Count)
					{
						value2 = Pt4aBoxNoSubMinBal[i];
					}

					Pt4aSubEntityAssociation.Add(new ParmT4aPt4aSub( value0, value1, value2));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class ParmT4aPt4aSub
	{
		public string Pt4aBoxNoSubAssocMember;	
		public string Pt4aBoxNoSubDescAssocMember;	
		public string Pt4aBoxNoSubMinBalAssocMember;	
		public ParmT4aPt4aSub() {}
		public ParmT4aPt4aSub(
			string inPt4aBoxNoSub,
			string inPt4aBoxNoSubDesc,
			string inPt4aBoxNoSubMinBal)
		{
			Pt4aBoxNoSubAssocMember = inPt4aBoxNoSub;
			Pt4aBoxNoSubDescAssocMember = inPt4aBoxNoSubDesc;
			Pt4aBoxNoSubMinBalAssocMember = inPt4aBoxNoSubMinBal;
		}
	}
}