//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/21/2020 4:05:49 PM by user mrityunjay
//
//     Type: ENTITY
//     Entity: TAX.FORM.1095C.WHIST
//     Application: HR
//     Environment: DvColl_WSTST01
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
	[DataContract(Name = "TaxForm1095cWhist")]
	[ColleagueDataContract(GeneratedDateTime = "10/21/2020 4:05:49 PM", User = "mrityunjay")]
	[EntityDataContract(EntityName = "TAX.FORM.1095C.WHIST", EntityType = "PHYS")]
	public class TaxForm1095cWhist : IColleagueEntity
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
		/// CDD Name: TFCWH.HRPER.ID
		/// </summary>
		[DataMember(Order = 0, Name = "TFCWH.HRPER.ID")]
		public string TfcwhHrperId { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.CORRECTED.IND
		/// </summary>
		[DataMember(Order = 3, Name = "TFCWH.CORRECTED.IND")]
		public string TfcwhCorrectedInd { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.TAX.YEAR
		/// </summary>
		[DataMember(Order = 9, Name = "TFCWH.TAX.YEAR")]
		public string TfcwhTaxYear { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.FIRST.NAME
		/// </summary>
		[DataMember(Order = 10, Name = "TFCWH.FIRST.NAME")]
		public string TfcwhFirstName { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.MIDDLE.NAME
		/// </summary>
		[DataMember(Order = 11, Name = "TFCWH.MIDDLE.NAME")]
		public string TfcwhMiddleName { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LAST.NAME
		/// </summary>
		[DataMember(Order = 12, Name = "TFCWH.LAST.NAME")]
		public string TfcwhLastName { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ADDRESS.LINE1.TEXT
		/// </summary>
		[DataMember(Order = 17, Name = "TFCWH.ADDRESS.LINE1.TEXT")]
		public string TfcwhAddressLine1Text { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ADDRESS.LINE2.TEXT
		/// </summary>
		[DataMember(Order = 18, Name = "TFCWH.ADDRESS.LINE2.TEXT")]
		public string TfcwhAddressLine2Text { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.CITY.NAME
		/// </summary>
		[DataMember(Order = 19, Name = "TFCWH.CITY.NAME")]
		public string TfcwhCityName { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.STATE.PROV.CODE
		/// </summary>
		[DataMember(Order = 20, Name = "TFCWH.STATE.PROV.CODE")]
		public string TfcwhStateProvCode { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.POSTAL.CODE
		/// </summary>
		[DataMember(Order = 21, Name = "TFCWH.POSTAL.CODE")]
		public string TfcwhPostalCode { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.EXTENSION
		/// </summary>
		[DataMember(Order = 22, Name = "TFCWH.ZIP.EXTENSION")]
		public string TfcwhZipExtension { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.COUNTRY.NAME
		/// </summary>
		[DataMember(Order = 24, Name = "TFCWH.COUNTRY.NAME")]
		public string TfcwhCountryName { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.12MNTH
		/// </summary>
		[DataMember(Order = 26, Name = "TFCWH.OFFER.CODE.12MNTH")]
		public string TfcwhOfferCode12mnth { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.JAN
		/// </summary>
		[DataMember(Order = 27, Name = "TFCWH.OFFER.CODE.JAN")]
		public string TfcwhOfferCodeJan { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.FEB
		/// </summary>
		[DataMember(Order = 28, Name = "TFCWH.OFFER.CODE.FEB")]
		public string TfcwhOfferCodeFeb { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.MAR
		/// </summary>
		[DataMember(Order = 29, Name = "TFCWH.OFFER.CODE.MAR")]
		public string TfcwhOfferCodeMar { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.APR
		/// </summary>
		[DataMember(Order = 30, Name = "TFCWH.OFFER.CODE.APR")]
		public string TfcwhOfferCodeApr { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.MAY
		/// </summary>
		[DataMember(Order = 31, Name = "TFCWH.OFFER.CODE.MAY")]
		public string TfcwhOfferCodeMay { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.JUN
		/// </summary>
		[DataMember(Order = 32, Name = "TFCWH.OFFER.CODE.JUN")]
		public string TfcwhOfferCodeJun { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.JUL
		/// </summary>
		[DataMember(Order = 33, Name = "TFCWH.OFFER.CODE.JUL")]
		public string TfcwhOfferCodeJul { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.AUG
		/// </summary>
		[DataMember(Order = 34, Name = "TFCWH.OFFER.CODE.AUG")]
		public string TfcwhOfferCodeAug { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.SEP
		/// </summary>
		[DataMember(Order = 35, Name = "TFCWH.OFFER.CODE.SEP")]
		public string TfcwhOfferCodeSep { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.OCT
		/// </summary>
		[DataMember(Order = 36, Name = "TFCWH.OFFER.CODE.OCT")]
		public string TfcwhOfferCodeOct { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.NOV
		/// </summary>
		[DataMember(Order = 37, Name = "TFCWH.OFFER.CODE.NOV")]
		public string TfcwhOfferCodeNov { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.OFFER.CODE.DEC
		/// </summary>
		[DataMember(Order = 38, Name = "TFCWH.OFFER.CODE.DEC")]
		public string TfcwhOfferCodeDec { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.12MNTH
		/// </summary>
		[DataMember(Order = 39, Name = "TFCWH.LOWEST.COST.AMT.12MNTH")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmt12mnth { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.JAN
		/// </summary>
		[DataMember(Order = 40, Name = "TFCWH.LOWEST.COST.AMT.JAN")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtJan { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.FEB
		/// </summary>
		[DataMember(Order = 41, Name = "TFCWH.LOWEST.COST.AMT.FEB")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtFeb { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.MAR
		/// </summary>
		[DataMember(Order = 42, Name = "TFCWH.LOWEST.COST.AMT.MAR")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtMar { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.APR
		/// </summary>
		[DataMember(Order = 43, Name = "TFCWH.LOWEST.COST.AMT.APR")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtApr { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.MAY
		/// </summary>
		[DataMember(Order = 44, Name = "TFCWH.LOWEST.COST.AMT.MAY")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtMay { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.JUN
		/// </summary>
		[DataMember(Order = 45, Name = "TFCWH.LOWEST.COST.AMT.JUN")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtJun { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.JUL
		/// </summary>
		[DataMember(Order = 46, Name = "TFCWH.LOWEST.COST.AMT.JUL")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtJul { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.AUG
		/// </summary>
		[DataMember(Order = 47, Name = "TFCWH.LOWEST.COST.AMT.AUG")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtAug { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.SEP
		/// </summary>
		[DataMember(Order = 48, Name = "TFCWH.LOWEST.COST.AMT.SEP")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtSep { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.OCT
		/// </summary>
		[DataMember(Order = 49, Name = "TFCWH.LOWEST.COST.AMT.OCT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtOct { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.NOV
		/// </summary>
		[DataMember(Order = 50, Name = "TFCWH.LOWEST.COST.AMT.NOV")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtNov { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.LOWEST.COST.AMT.DEC
		/// </summary>
		[DataMember(Order = 51, Name = "TFCWH.LOWEST.COST.AMT.DEC")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public Decimal? TfcwhLowestCostAmtDec { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CD.12MNTH
		/// </summary>
		[DataMember(Order = 52, Name = "TFCWH.SAFE.HARBOR.CD.12MNTH")]
		public string TfcwhSafeHarborCd12mnth { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.JAN
		/// </summary>
		[DataMember(Order = 53, Name = "TFCWH.SAFE.HARBOR.CODE.JAN")]
		public string TfcwhSafeHarborCodeJan { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.FEB
		/// </summary>
		[DataMember(Order = 54, Name = "TFCWH.SAFE.HARBOR.CODE.FEB")]
		public string TfcwhSafeHarborCodeFeb { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.MAR
		/// </summary>
		[DataMember(Order = 55, Name = "TFCWH.SAFE.HARBOR.CODE.MAR")]
		public string TfcwhSafeHarborCodeMar { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.APR
		/// </summary>
		[DataMember(Order = 56, Name = "TFCWH.SAFE.HARBOR.CODE.APR")]
		public string TfcwhSafeHarborCodeApr { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.MAY
		/// </summary>
		[DataMember(Order = 57, Name = "TFCWH.SAFE.HARBOR.CODE.MAY")]
		public string TfcwhSafeHarborCodeMay { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.JUN
		/// </summary>
		[DataMember(Order = 58, Name = "TFCWH.SAFE.HARBOR.CODE.JUN")]
		public string TfcwhSafeHarborCodeJun { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.JUL
		/// </summary>
		[DataMember(Order = 59, Name = "TFCWH.SAFE.HARBOR.CODE.JUL")]
		public string TfcwhSafeHarborCodeJul { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.AUG
		/// </summary>
		[DataMember(Order = 60, Name = "TFCWH.SAFE.HARBOR.CODE.AUG")]
		public string TfcwhSafeHarborCodeAug { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.SEP
		/// </summary>
		[DataMember(Order = 61, Name = "TFCWH.SAFE.HARBOR.CODE.SEP")]
		public string TfcwhSafeHarborCodeSep { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.OCT
		/// </summary>
		[DataMember(Order = 62, Name = "TFCWH.SAFE.HARBOR.CODE.OCT")]
		public string TfcwhSafeHarborCodeOct { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.NOV
		/// </summary>
		[DataMember(Order = 63, Name = "TFCWH.SAFE.HARBOR.CODE.NOV")]
		public string TfcwhSafeHarborCodeNov { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.SAFE.HARBOR.CODE.DEC
		/// </summary>
		[DataMember(Order = 64, Name = "TFCWH.SAFE.HARBOR.CODE.DEC")]
		public string TfcwhSafeHarborCodeDec { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.COVERED.INDIV.IND
		/// </summary>
		[DataMember(Order = 65, Name = "TFCWH.COVERED.INDIV.IND")]
		public string TfcwhCoveredIndivInd { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.STATUS
		/// </summary>
		[DataMember(Order = 66, Name = "TFCWH.STATUS")]
		public string TfcwhStatus { get; set; }
		
		/// <summary>
		/// CDD Name: TAX.FORM.1095C.WHIST.ADDDATE
		/// </summary>
		[DataMember(Order = 68, Name = "TAX.FORM.1095C.WHIST.ADDDATE")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? TaxForm1095cWhistAdddate { get; set; }
		
		/// <summary>
		/// CDD Name: TAX.FORM.1095C.WHIST.ADDTIME
		/// </summary>
		[DataMember(Order = 69, Name = "TAX.FORM.1095C.WHIST.ADDTIME")]
		[DisplayFormat(DataFormatString = "{0:T}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? TaxForm1095cWhistAddtime { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.VOID.IND
		/// </summary>
		[DataMember(Order = 78, Name = "TFCWH.VOID.IND")]
		public string TfcwhVoidInd { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.AGE
		/// </summary>
		[DataMember(Order = 80, Name = "TFCWH.AGE")]
		public int? TfcwhAge { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.12MNTH
		/// </summary>
		[DataMember(Order = 81, Name = "TFCWH.ZIP.12MNTH")]
		public string TfcwhZip12mnth { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.JAN
		/// </summary>
		[DataMember(Order = 82, Name = "TFCWH.ZIP.JAN")]
		public string TfcwhZipJan { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.FEB
		/// </summary>
		[DataMember(Order = 83, Name = "TFCWH.ZIP.FEB")]
		public string TfcwhZipFeb { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.MAR
		/// </summary>
		[DataMember(Order = 84, Name = "TFCWH.ZIP.MAR")]
		public string TfcwhZipMar { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.APR
		/// </summary>
		[DataMember(Order = 85, Name = "TFCWH.ZIP.APR")]
		public string TfcwhZipApr { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.MAY
		/// </summary>
		[DataMember(Order = 86, Name = "TFCWH.ZIP.MAY")]
		public string TfcwhZipMay { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.JUN
		/// </summary>
		[DataMember(Order = 87, Name = "TFCWH.ZIP.JUN")]
		public string TfcwhZipJun { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.JUL
		/// </summary>
		[DataMember(Order = 88, Name = "TFCWH.ZIP.JUL")]
		public string TfcwhZipJul { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.AUG
		/// </summary>
		[DataMember(Order = 89, Name = "TFCWH.ZIP.AUG")]
		public string TfcwhZipAug { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.SEP
		/// </summary>
		[DataMember(Order = 90, Name = "TFCWH.ZIP.SEP")]
		public string TfcwhZipSep { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.OCT
		/// </summary>
		[DataMember(Order = 91, Name = "TFCWH.ZIP.OCT")]
		public string TfcwhZipOct { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.NOV
		/// </summary>
		[DataMember(Order = 92, Name = "TFCWH.ZIP.NOV")]
		public string TfcwhZipNov { get; set; }
		
		/// <summary>
		/// CDD Name: TFCWH.ZIP.DEC
		/// </summary>
		[DataMember(Order = 93, Name = "TFCWH.ZIP.DEC")]
		public string TfcwhZipDec { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}