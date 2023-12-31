//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 10/30/2017 10:50:57 PM by user dvcoll-schandraseka
//
//     Type: ENTITY
//     Entity: TAX.FORM.T4A.REPOS
//     Application: CF
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

namespace Ellucian.Colleague.Data.ColleagueFinance.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "TaxFormT4aRepos")]
	[ColleagueDataContract(GeneratedDateTime = "10/30/2017 10:50:57 PM", User = "dvcoll-schandraseka")]
	[EntityDataContract(EntityName = "TAX.FORM.T4A.REPOS", EntityType = "PHYS")]
	public class TaxFormT4aRepos : IColleagueEntity
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
		/// CDD Name: TFTR.ID
		/// </summary>
		[DataMember(Order = 2, Name = "TFTR.ID")]
		public string TftrId { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.SIN
		/// </summary>
		[DataMember(Order = 3, Name = "TFTR.SIN")]
		public string TftrSin { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.FIRST.NAME
		/// </summary>
		[DataMember(Order = 4, Name = "TFTR.FIRST.NAME")]
		public string TftrFirstName { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.MIDDLE.INITIAL
		/// </summary>
		[DataMember(Order = 5, Name = "TFTR.MIDDLE.INITIAL")]
		public string TftrMiddleInitial { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.SURNAME
		/// </summary>
		[DataMember(Order = 6, Name = "TFTR.SURNAME")]
		public string TftrSurname { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.ADDRESS
		/// </summary>
		[DataMember(Order = 7, Name = "TFTR.ADDRESS")]
		public string TftrAddress { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CITY
		/// </summary>
		[DataMember(Order = 8, Name = "TFTR.CITY")]
		public string TftrCity { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.PROVINCE
		/// </summary>
		[DataMember(Order = 9, Name = "TFTR.PROVINCE")]
		public string TftrProvince { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.POSTAL.CODE
		/// </summary>
		[DataMember(Order = 10, Name = "TFTR.POSTAL.CODE")]
		public string TftrPostalCode { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.COUNTRY
		/// </summary>
		[DataMember(Order = 11, Name = "TFTR.COUNTRY")]
		public string TftrCountry { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.DEFERRED.FLAG
		/// </summary>
		[DataMember(Order = 13, Name = "TFTR.DEFERRED.FLAG")]
		public string TftrDeferredFlag { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.ADDRESS2
		/// </summary>
		[DataMember(Order = 16, Name = "TFTR.ADDRESS2")]
		public string TftrAddress2 { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.ADDRESS2
		/// </summary>
		[DataMember(Order = 42, Name = "TFTR.CERTIFY.ADDRESS2")]
		public List<string> TftrCertifyAddress2 { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.PENSION.NO
		/// </summary>
		[DataMember(Order = 43, Name = "TFTR.CERTIFY.PENSION.NO")]
		public List<string> TftrCertifyPensionNo { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.DEFERRED.FLAG
		/// </summary>
		[DataMember(Order = 44, Name = "TFTR.CERTIFY.DEFERRED.FLAG")]
		public List<string> TftrCertifyDeferredFlag { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.COUNTRY
		/// </summary>
		[DataMember(Order = 45, Name = "TFTR.CERTIFY.COUNTRY")]
		public List<string> TftrCertifyCountry { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.SIN
		/// </summary>
		[DataMember(Order = 48, Name = "TFTR.CERTIFY.SIN")]
		public List<string> TftrCertifySin { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.ADDRESS
		/// </summary>
		[DataMember(Order = 49, Name = "TFTR.CERTIFY.ADDRESS")]
		public List<string> TftrCertifyAddress { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.FIRST.NAME
		/// </summary>
		[DataMember(Order = 50, Name = "TFTR.CERTIFY.FIRST.NAME")]
		public List<string> TftrCertifyFirstName { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.MIDDLE.INITIAL
		/// </summary>
		[DataMember(Order = 51, Name = "TFTR.CERTIFY.MIDDLE.INITIAL")]
		public List<string> TftrCertifyMiddleInitial { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.SURNAME
		/// </summary>
		[DataMember(Order = 52, Name = "TFTR.CERTIFY.SURNAME")]
		public List<string> TftrCertifySurname { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.CITY
		/// </summary>
		[DataMember(Order = 53, Name = "TFTR.CERTIFY.CITY")]
		public List<string> TftrCertifyCity { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.PROVINCE
		/// </summary>
		[DataMember(Order = 54, Name = "TFTR.CERTIFY.PROVINCE")]
		public List<string> TftrCertifyProvince { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.POSTAL.CODE
		/// </summary>
		[DataMember(Order = 55, Name = "TFTR.CERTIFY.POSTAL.CODE")]
		public List<string> TftrCertifyPostalCode { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.RECIPIENT.BN
		/// </summary>
		[DataMember(Order = 56, Name = "TFTR.RECIPIENT.BN")]
		public string TftrRecipientBn { get; set; }
		
		/// <summary>
		/// CDD Name: TFTR.CERTIFY.RECIPIENT.BN
		/// </summary>
		[DataMember(Order = 57, Name = "TFTR.CERTIFY.RECIPIENT.BN")]
		public List<string> TftrCertifyRecipientBn { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<TaxFormT4aReposTftrCert> TftrCertEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: TFTR.CERT
			
			TftrCertEntityAssociation = new List<TaxFormT4aReposTftrCert>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(TftrCertifyAddress2 != null)
			{
				int numTftrCert = TftrCertifyAddress2.Count;
				if (TftrCertifyPensionNo !=null && TftrCertifyPensionNo.Count > numTftrCert) numTftrCert = TftrCertifyPensionNo.Count;
				if (TftrCertifyDeferredFlag !=null && TftrCertifyDeferredFlag.Count > numTftrCert) numTftrCert = TftrCertifyDeferredFlag.Count;
				if (TftrCertifyCountry !=null && TftrCertifyCountry.Count > numTftrCert) numTftrCert = TftrCertifyCountry.Count;
				if (TftrCertifySin !=null && TftrCertifySin.Count > numTftrCert) numTftrCert = TftrCertifySin.Count;
				if (TftrCertifyAddress !=null && TftrCertifyAddress.Count > numTftrCert) numTftrCert = TftrCertifyAddress.Count;
				if (TftrCertifyFirstName !=null && TftrCertifyFirstName.Count > numTftrCert) numTftrCert = TftrCertifyFirstName.Count;
				if (TftrCertifyMiddleInitial !=null && TftrCertifyMiddleInitial.Count > numTftrCert) numTftrCert = TftrCertifyMiddleInitial.Count;
				if (TftrCertifySurname !=null && TftrCertifySurname.Count > numTftrCert) numTftrCert = TftrCertifySurname.Count;
				if (TftrCertifyCity !=null && TftrCertifyCity.Count > numTftrCert) numTftrCert = TftrCertifyCity.Count;
				if (TftrCertifyProvince !=null && TftrCertifyProvince.Count > numTftrCert) numTftrCert = TftrCertifyProvince.Count;
				if (TftrCertifyPostalCode !=null && TftrCertifyPostalCode.Count > numTftrCert) numTftrCert = TftrCertifyPostalCode.Count;
				if (TftrCertifyRecipientBn !=null && TftrCertifyRecipientBn.Count > numTftrCert) numTftrCert = TftrCertifyRecipientBn.Count;

				for (int i = 0; i < numTftrCert; i++)
				{

					string value0 = "";
					if (TftrCertifyAddress2 != null && i < TftrCertifyAddress2.Count)
					{
						value0 = TftrCertifyAddress2[i];
					}


					string value1 = "";
					if (TftrCertifyPensionNo != null && i < TftrCertifyPensionNo.Count)
					{
						value1 = TftrCertifyPensionNo[i];
					}


					string value2 = "";
					if (TftrCertifyDeferredFlag != null && i < TftrCertifyDeferredFlag.Count)
					{
						value2 = TftrCertifyDeferredFlag[i];
					}


					string value3 = "";
					if (TftrCertifyCountry != null && i < TftrCertifyCountry.Count)
					{
						value3 = TftrCertifyCountry[i];
					}


					string value4 = "";
					if (TftrCertifySin != null && i < TftrCertifySin.Count)
					{
						value4 = TftrCertifySin[i];
					}


					string value5 = "";
					if (TftrCertifyAddress != null && i < TftrCertifyAddress.Count)
					{
						value5 = TftrCertifyAddress[i];
					}


					string value6 = "";
					if (TftrCertifyFirstName != null && i < TftrCertifyFirstName.Count)
					{
						value6 = TftrCertifyFirstName[i];
					}


					string value7 = "";
					if (TftrCertifyMiddleInitial != null && i < TftrCertifyMiddleInitial.Count)
					{
						value7 = TftrCertifyMiddleInitial[i];
					}


					string value8 = "";
					if (TftrCertifySurname != null && i < TftrCertifySurname.Count)
					{
						value8 = TftrCertifySurname[i];
					}


					string value9 = "";
					if (TftrCertifyCity != null && i < TftrCertifyCity.Count)
					{
						value9 = TftrCertifyCity[i];
					}


					string value10 = "";
					if (TftrCertifyProvince != null && i < TftrCertifyProvince.Count)
					{
						value10 = TftrCertifyProvince[i];
					}


					string value11 = "";
					if (TftrCertifyPostalCode != null && i < TftrCertifyPostalCode.Count)
					{
						value11 = TftrCertifyPostalCode[i];
					}


					string value12 = "";
					if (TftrCertifyRecipientBn != null && i < TftrCertifyRecipientBn.Count)
					{
						value12 = TftrCertifyRecipientBn[i];
					}

					TftrCertEntityAssociation.Add(new TaxFormT4aReposTftrCert( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class TaxFormT4aReposTftrCert
	{
		public string TftrCertifyAddress2AssocMember;	
		public string TftrCertifyPensionNoAssocMember;	
		public string TftrCertifyDeferredFlagAssocMember;	
		public string TftrCertifyCountryAssocMember;	
		public string TftrCertifySinAssocMember;	
		public string TftrCertifyAddressAssocMember;	
		public string TftrCertifyFirstNameAssocMember;	
		public string TftrCertifyMiddleInitialAssocMember;	
		public string TftrCertifySurnameAssocMember;	
		public string TftrCertifyCityAssocMember;	
		public string TftrCertifyProvinceAssocMember;	
		public string TftrCertifyPostalCodeAssocMember;	
		public string TftrCertifyRecipientBnAssocMember;	
		public TaxFormT4aReposTftrCert() {}
		public TaxFormT4aReposTftrCert(
			string inTftrCertifyAddress2,
			string inTftrCertifyPensionNo,
			string inTftrCertifyDeferredFlag,
			string inTftrCertifyCountry,
			string inTftrCertifySin,
			string inTftrCertifyAddress,
			string inTftrCertifyFirstName,
			string inTftrCertifyMiddleInitial,
			string inTftrCertifySurname,
			string inTftrCertifyCity,
			string inTftrCertifyProvince,
			string inTftrCertifyPostalCode,
			string inTftrCertifyRecipientBn)
		{
			TftrCertifyAddress2AssocMember = inTftrCertifyAddress2;
			TftrCertifyPensionNoAssocMember = inTftrCertifyPensionNo;
			TftrCertifyDeferredFlagAssocMember = inTftrCertifyDeferredFlag;
			TftrCertifyCountryAssocMember = inTftrCertifyCountry;
			TftrCertifySinAssocMember = inTftrCertifySin;
			TftrCertifyAddressAssocMember = inTftrCertifyAddress;
			TftrCertifyFirstNameAssocMember = inTftrCertifyFirstName;
			TftrCertifyMiddleInitialAssocMember = inTftrCertifyMiddleInitial;
			TftrCertifySurnameAssocMember = inTftrCertifySurname;
			TftrCertifyCityAssocMember = inTftrCertifyCity;
			TftrCertifyProvinceAssocMember = inTftrCertifyProvince;
			TftrCertifyPostalCodeAssocMember = inTftrCertifyPostalCode;
			TftrCertifyRecipientBnAssocMember = inTftrCertifyRecipientBn;
		}
	}
}