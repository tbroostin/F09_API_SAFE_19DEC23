//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/10/2021 3:26:19 PM by user gtt_dvcoll_wstst01
//
//     Type: ENTITY
//     Entity: CNST.RPT.PARMS
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "CnstRptParms")]
	[ColleagueDataContract(GeneratedDateTime = "5/10/2021 3:26:19 PM", User = "gtt_dvcoll_wstst01")]
	[EntityDataContract(EntityName = "CNST.RPT.PARMS", EntityType = "PERM")]
	public class CnstRptParms : IColleagueEntity
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
		/// CDD Name: CNST.T2202A.PDF.TAX.YEAR
		/// </summary>
		[DataMember(Order = 98, Name = "CNST.T2202A.PDF.TAX.YEAR")]
		public List<string> CnstT2202aPdfTaxYear { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.T2202A.PDF.RPT.PGM
		/// </summary>
		[DataMember(Order = 99, Name = "CNST.T2202A.PDF.RPT.PGM")]
		public List<string> CnstT2202aPdfRptPgm { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.T2202A.PDF.RPT.MAPPING
		/// </summary>
		[DataMember(Order = 100, Name = "CNST.T2202A.PDF.RPT.MAPPING")]
		public List<string> CnstT2202aPdfRptMapping { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.T2202A.PDF.WEB.FLAG
		/// </summary>
		[DataMember(Order = 101, Name = "CNST.T2202A.PDF.WEB.FLAG")]
		public List<string> CnstT2202aPdfWebFlag { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.T2202A.PDF.WEB.PGM
		/// </summary>
		[DataMember(Order = 102, Name = "CNST.T2202A.PDF.WEB.PGM")]
		public List<string> CnstT2202aPdfWebPgm { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.CONSENT.TEXT
		/// </summary>
		[DataMember(Order = 103, Name = "CNST.CONSENT.TEXT")]
		public string CnstConsentText { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.WHLD.CONSENT.TEXT
		/// </summary>
		[DataMember(Order = 104, Name = "CNST.WHLD.CONSENT.TEXT")]
		public string CnstWhldConsentText { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.T2202.SCHOOL.TYPE
		/// </summary>
		[DataMember(Order = 115, Name = "CNST.T2202.SCHOOL.TYPE")]
		public string CnstT2202SchoolType { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.T2202.FLYING.CLUB
		/// </summary>
		[DataMember(Order = 116, Name = "CNST.T2202.FLYING.CLUB")]
		public string CnstT2202FlyingClub { get; set; }
		
		/// <summary>
		/// CDD Name: CNST.HIDE.CONSENT.FLAG
		/// </summary>
		[DataMember(Order = 133, Name = "CNST.HIDE.CONSENT.FLAG")]
		public string CnstHideConsentFlag { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<CnstRptParmsCnstT2202aPdfParms> CnstT2202aPdfParmsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: CNST.T2202A.PDF.PARMS
			
			CnstT2202aPdfParmsEntityAssociation = new List<CnstRptParmsCnstT2202aPdfParms>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(CnstT2202aPdfTaxYear != null)
			{
				int numCnstT2202aPdfParms = CnstT2202aPdfTaxYear.Count;
				if (CnstT2202aPdfRptPgm !=null && CnstT2202aPdfRptPgm.Count > numCnstT2202aPdfParms) numCnstT2202aPdfParms = CnstT2202aPdfRptPgm.Count;
				if (CnstT2202aPdfRptMapping !=null && CnstT2202aPdfRptMapping.Count > numCnstT2202aPdfParms) numCnstT2202aPdfParms = CnstT2202aPdfRptMapping.Count;
				if (CnstT2202aPdfWebFlag !=null && CnstT2202aPdfWebFlag.Count > numCnstT2202aPdfParms) numCnstT2202aPdfParms = CnstT2202aPdfWebFlag.Count;
				if (CnstT2202aPdfWebPgm !=null && CnstT2202aPdfWebPgm.Count > numCnstT2202aPdfParms) numCnstT2202aPdfParms = CnstT2202aPdfWebPgm.Count;

				for (int i = 0; i < numCnstT2202aPdfParms; i++)
				{

					string value0 = "";
					if (CnstT2202aPdfTaxYear != null && i < CnstT2202aPdfTaxYear.Count)
					{
						value0 = CnstT2202aPdfTaxYear[i];
					}


					string value1 = "";
					if (CnstT2202aPdfRptPgm != null && i < CnstT2202aPdfRptPgm.Count)
					{
						value1 = CnstT2202aPdfRptPgm[i];
					}


					string value2 = "";
					if (CnstT2202aPdfRptMapping != null && i < CnstT2202aPdfRptMapping.Count)
					{
						value2 = CnstT2202aPdfRptMapping[i];
					}


					string value3 = "";
					if (CnstT2202aPdfWebFlag != null && i < CnstT2202aPdfWebFlag.Count)
					{
						value3 = CnstT2202aPdfWebFlag[i];
					}


					string value4 = "";
					if (CnstT2202aPdfWebPgm != null && i < CnstT2202aPdfWebPgm.Count)
					{
						value4 = CnstT2202aPdfWebPgm[i];
					}

					CnstT2202aPdfParmsEntityAssociation.Add(new CnstRptParmsCnstT2202aPdfParms( value0, value1, value2, value3, value4));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class CnstRptParmsCnstT2202aPdfParms
	{
		public string CnstT2202aPdfTaxYearAssocMember;	
		public string CnstT2202aPdfRptPgmAssocMember;	
		public string CnstT2202aPdfRptMappingAssocMember;	
		public string CnstT2202aPdfWebFlagAssocMember;	
		public string CnstT2202aPdfWebPgmAssocMember;	
		public CnstRptParmsCnstT2202aPdfParms() {}
		public CnstRptParmsCnstT2202aPdfParms(
			string inCnstT2202aPdfTaxYear,
			string inCnstT2202aPdfRptPgm,
			string inCnstT2202aPdfRptMapping,
			string inCnstT2202aPdfWebFlag,
			string inCnstT2202aPdfWebPgm)
		{
			CnstT2202aPdfTaxYearAssocMember = inCnstT2202aPdfTaxYear;
			CnstT2202aPdfRptPgmAssocMember = inCnstT2202aPdfRptPgm;
			CnstT2202aPdfRptMappingAssocMember = inCnstT2202aPdfRptMapping;
			CnstT2202aPdfWebFlagAssocMember = inCnstT2202aPdfWebFlag;
			CnstT2202aPdfWebPgmAssocMember = inCnstT2202aPdfWebPgm;
		}
	}
}