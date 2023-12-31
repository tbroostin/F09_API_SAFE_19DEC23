//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 8/14/2020 8:20:33 PM by user mrityunjay
//
//     Type: ENTITY
//     Entity: PARM.1099NEC
//     Application: CF
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

namespace Ellucian.Colleague.Data.Base.DataContracts
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract(Name = "Parm1099nec")]
	[ColleagueDataContract(GeneratedDateTime = "8/14/2020 8:20:33 PM", User = "mrityunjay")]
	[EntityDataContract(EntityName = "PARM.1099NEC", EntityType = "PERM")]
	public class Parm1099nec : IColleagueEntity
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
		/// CDD Name: P1099NEC.ACCESS.EMAIL
		/// </summary>
		[DataMember(Order = 0, Name = "P1099NEC.ACCESS.EMAIL")]
		public string P1099necAccessEmail { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.CON.CHG.EMAIL
		/// </summary>
		[DataMember(Order = 1, Name = "P1099NEC.CON.CHG.EMAIL")]
		public string P1099necConChgEmail { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.CONSENT.TEXT
		/// </summary>
		[DataMember(Order = 2, Name = "P1099NEC.CONSENT.TEXT")]
		public string P1099necConsentText { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.FORMS.PER.PAGE
		/// </summary>
		[DataMember(Order = 3, Name = "P1099NEC.FORMS.PER.PAGE")]
		public int? P1099necFormsPerPage { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.INCLUDE.INST.INFO
		/// </summary>
		[DataMember(Order = 4, Name = "P1099NEC.INCLUDE.INST.INFO")]
		public string P1099necIncludeInstInfo { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.PAGE.WIDTH
		/// </summary>
		[DataMember(Order = 5, Name = "P1099NEC.PAGE.WIDTH")]
		public int? P1099necPageWidth { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.PHONE.EXTENSION
		/// </summary>
		[DataMember(Order = 6, Name = "P1099NEC.PHONE.EXTENSION")]
		public string P1099necPhoneExtension { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.PHONE.NUMBER
		/// </summary>
		[DataMember(Order = 7, Name = "P1099NEC.PHONE.NUMBER")]
		public string P1099necPhoneNumber { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.ELECT.FILE.NAME
		/// </summary>
		[DataMember(Order = 8, Name = "P1099NEC.T.ELECT.FILE.NAME")]
		public string P1099necTElectFileName { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.REPLACE.ALPHA
		/// </summary>
		[DataMember(Order = 9, Name = "P1099NEC.T.REPLACE.ALPHA")]
		public string P1099necTReplaceAlpha { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.TAPE.CODE
		/// </summary>
		[DataMember(Order = 10, Name = "P1099NEC.TAPE.CODE")]
		public string P1099necTapeCode { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.TAPE.SUB.NAME
		/// </summary>
		[DataMember(Order = 11, Name = "P1099NEC.TAPE.SUB.NAME")]
		public string P1099necTapeSubName { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.WHLD.CONSENT.TEXT
		/// </summary>
		[DataMember(Order = 12, Name = "P1099NEC.WHLD.CONSENT.TEXT")]
		public string P1099necWhldConsentText { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.YEAR
		/// </summary>
		[DataMember(Order = 13, Name = "P1099NEC.YEAR")]
		public string P1099necYear { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.BOX.NO
		/// </summary>
		[DataMember(Order = 20, Name = "P1099NEC.BOX.NO")]
		public List<string> P1099necBoxNo { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.BOX.DESC
		/// </summary>
		[DataMember(Order = 21, Name = "P1099NEC.BOX.DESC")]
		public List<string> P1099necBoxDesc { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.BOX.MIN.BAL
		/// </summary>
		[DataMember(Order = 22, Name = "P1099NEC.BOX.MIN.BAL")]
		public List<string> P1099necBoxMinBal { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.BOX.TOT.FLAG
		/// </summary>
		[DataMember(Order = 23, Name = "P1099NEC.BOX.TOT.FLAG")]
		public List<string> P1099necBoxTotFlag { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.BOX.WITH.FLAG
		/// </summary>
		[DataMember(Order = 24, Name = "P1099NEC.BOX.WITH.FLAG")]
		public List<string> P1099necBoxWithFlag { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.MIN.BAL.CODE
		/// </summary>
		[DataMember(Order = 25, Name = "P1099NEC.MIN.BAL.CODE")]
		public List<string> P1099necMinBalCode { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.MIN.BAL.AMT
		/// </summary>
		[DataMember(Order = 26, Name = "P1099NEC.MIN.BAL.AMT")]
		[DisplayFormat(DataFormatString = "{0:N2}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public List<Decimal?> P1099necMinBalAmt { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.MIN.BAL.DESC
		/// </summary>
		[DataMember(Order = 27, Name = "P1099NEC.MIN.BAL.DESC")]
		public List<string> P1099necMinBalDesc { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.TRANSMITTER.ID
		/// </summary>
		[DataMember(Order = 28, Name = "P1099NEC.T.TRANSMITTER.ID")]
		public List<string> P1099necTTransmitterId { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.CO.ADDR
		/// </summary>
		[DataMember(Order = 29, Name = "P1099NEC.T.CO.ADDR")]
		public List<string> P1099necTCoAddr { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.CO.CITY
		/// </summary>
		[DataMember(Order = 30, Name = "P1099NEC.T.CO.CITY")]
		public List<string> P1099necTCoCity { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.CO.NAME
		/// </summary>
		[DataMember(Order = 31, Name = "P1099NEC.T.CO.NAME")]
		public List<string> P1099necTCoName { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.CO.NAME2
		/// </summary>
		[DataMember(Order = 32, Name = "P1099NEC.T.CO.NAME2")]
		public List<string> P1099necTCoName2 { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.CONTACT
		/// </summary>
		[DataMember(Order = 33, Name = "P1099NEC.T.CONTACT")]
		public List<string> P1099necTContact { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.CONTACT.EMAIL
		/// </summary>
		[DataMember(Order = 34, Name = "P1099NEC.T.CONTACT.EMAIL")]
		public List<string> P1099necTContactEmail { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.CONTACT.EXTENSION
		/// </summary>
		[DataMember(Order = 35, Name = "P1099NEC.T.CONTACT.EXTENSION")]
		public List<string> P1099necTContactExtension { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.CONTACT.PHONE
		/// </summary>
		[DataMember(Order = 36, Name = "P1099NEC.T.CONTACT.PHONE")]
		public List<string> P1099necTContactPhone { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.NAME
		/// </summary>
		[DataMember(Order = 37, Name = "P1099NEC.T.NAME")]
		public List<string> P1099necTName { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.NAME2
		/// </summary>
		[DataMember(Order = 38, Name = "P1099NEC.T.NAME2")]
		public List<string> P1099necTName2 { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.STATE
		/// </summary>
		[DataMember(Order = 39, Name = "P1099NEC.T.STATE")]
		public List<string> P1099necTState { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.TRANSMITTER.TIN
		/// </summary>
		[DataMember(Order = 40, Name = "P1099NEC.T.TRANSMITTER.TIN")]
		public List<string> P1099necTTransmitterTin { get; set; }
		
		/// <summary>
		/// CDD Name: P1099NEC.T.ZIP
		/// </summary>
		[DataMember(Order = 41, Name = "P1099NEC.T.ZIP")]
		public List<string> P1099necTZip { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<Parm1099necP1099necBox> P1099necBoxEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<Parm1099necP1099necMinBal> P1099necMinBalEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<Parm1099necP1099necTransDm> P1099necTransDmEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: P1099NEC.BOX
			
			P1099necBoxEntityAssociation = new List<Parm1099necP1099necBox>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(P1099necBoxNo != null)
			{
				int numP1099necBox = P1099necBoxNo.Count;
				if (P1099necBoxDesc !=null && P1099necBoxDesc.Count > numP1099necBox) numP1099necBox = P1099necBoxDesc.Count;
				if (P1099necBoxMinBal !=null && P1099necBoxMinBal.Count > numP1099necBox) numP1099necBox = P1099necBoxMinBal.Count;
				if (P1099necBoxTotFlag !=null && P1099necBoxTotFlag.Count > numP1099necBox) numP1099necBox = P1099necBoxTotFlag.Count;
				if (P1099necBoxWithFlag !=null && P1099necBoxWithFlag.Count > numP1099necBox) numP1099necBox = P1099necBoxWithFlag.Count;

				for (int i = 0; i < numP1099necBox; i++)
				{

					string value0 = "";
					if (P1099necBoxNo != null && i < P1099necBoxNo.Count)
					{
						value0 = P1099necBoxNo[i];
					}


					string value1 = "";
					if (P1099necBoxDesc != null && i < P1099necBoxDesc.Count)
					{
						value1 = P1099necBoxDesc[i];
					}


					string value2 = "";
					if (P1099necBoxMinBal != null && i < P1099necBoxMinBal.Count)
					{
						value2 = P1099necBoxMinBal[i];
					}


					string value3 = "";
					if (P1099necBoxTotFlag != null && i < P1099necBoxTotFlag.Count)
					{
						value3 = P1099necBoxTotFlag[i];
					}


					string value4 = "";
					if (P1099necBoxWithFlag != null && i < P1099necBoxWithFlag.Count)
					{
						value4 = P1099necBoxWithFlag[i];
					}

					P1099necBoxEntityAssociation.Add(new Parm1099necP1099necBox( value0, value1, value2, value3, value4));
				}
			}
			// EntityAssociation Name: P1099NEC.MIN.BAL
			
			P1099necMinBalEntityAssociation = new List<Parm1099necP1099necMinBal>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(P1099necMinBalCode != null)
			{
				int numP1099necMinBal = P1099necMinBalCode.Count;
				if (P1099necMinBalAmt !=null && P1099necMinBalAmt.Count > numP1099necMinBal) numP1099necMinBal = P1099necMinBalAmt.Count;
				if (P1099necMinBalDesc !=null && P1099necMinBalDesc.Count > numP1099necMinBal) numP1099necMinBal = P1099necMinBalDesc.Count;

				for (int i = 0; i < numP1099necMinBal; i++)
				{

					string value0 = "";
					if (P1099necMinBalCode != null && i < P1099necMinBalCode.Count)
					{
						value0 = P1099necMinBalCode[i];
					}


					Decimal? value1 = null;
					if (P1099necMinBalAmt != null && i < P1099necMinBalAmt.Count)
					{
						value1 = P1099necMinBalAmt[i];
					}


					string value2 = "";
					if (P1099necMinBalDesc != null && i < P1099necMinBalDesc.Count)
					{
						value2 = P1099necMinBalDesc[i];
					}

					P1099necMinBalEntityAssociation.Add(new Parm1099necP1099necMinBal( value0, value1, value2));
				}
			}
			// EntityAssociation Name: P1099NEC.TRANS.DM
			
			P1099necTransDmEntityAssociation = new List<Parm1099necP1099necTransDm>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(P1099necTTransmitterId != null)
			{
				int numP1099necTransDm = P1099necTTransmitterId.Count;
				if (P1099necTCoAddr !=null && P1099necTCoAddr.Count > numP1099necTransDm) numP1099necTransDm = P1099necTCoAddr.Count;
				if (P1099necTCoCity !=null && P1099necTCoCity.Count > numP1099necTransDm) numP1099necTransDm = P1099necTCoCity.Count;
				if (P1099necTCoName !=null && P1099necTCoName.Count > numP1099necTransDm) numP1099necTransDm = P1099necTCoName.Count;
				if (P1099necTCoName2 !=null && P1099necTCoName2.Count > numP1099necTransDm) numP1099necTransDm = P1099necTCoName2.Count;
				if (P1099necTContact !=null && P1099necTContact.Count > numP1099necTransDm) numP1099necTransDm = P1099necTContact.Count;
				if (P1099necTContactEmail !=null && P1099necTContactEmail.Count > numP1099necTransDm) numP1099necTransDm = P1099necTContactEmail.Count;
				if (P1099necTContactExtension !=null && P1099necTContactExtension.Count > numP1099necTransDm) numP1099necTransDm = P1099necTContactExtension.Count;
				if (P1099necTContactPhone !=null && P1099necTContactPhone.Count > numP1099necTransDm) numP1099necTransDm = P1099necTContactPhone.Count;
				if (P1099necTName !=null && P1099necTName.Count > numP1099necTransDm) numP1099necTransDm = P1099necTName.Count;
				if (P1099necTName2 !=null && P1099necTName2.Count > numP1099necTransDm) numP1099necTransDm = P1099necTName2.Count;
				if (P1099necTState !=null && P1099necTState.Count > numP1099necTransDm) numP1099necTransDm = P1099necTState.Count;
				if (P1099necTTransmitterTin !=null && P1099necTTransmitterTin.Count > numP1099necTransDm) numP1099necTransDm = P1099necTTransmitterTin.Count;
				if (P1099necTZip !=null && P1099necTZip.Count > numP1099necTransDm) numP1099necTransDm = P1099necTZip.Count;

				for (int i = 0; i < numP1099necTransDm; i++)
				{

					string value0 = "";
					if (P1099necTTransmitterId != null && i < P1099necTTransmitterId.Count)
					{
						value0 = P1099necTTransmitterId[i];
					}


					string value1 = "";
					if (P1099necTCoAddr != null && i < P1099necTCoAddr.Count)
					{
						value1 = P1099necTCoAddr[i];
					}


					string value2 = "";
					if (P1099necTCoCity != null && i < P1099necTCoCity.Count)
					{
						value2 = P1099necTCoCity[i];
					}


					string value3 = "";
					if (P1099necTCoName != null && i < P1099necTCoName.Count)
					{
						value3 = P1099necTCoName[i];
					}


					string value4 = "";
					if (P1099necTCoName2 != null && i < P1099necTCoName2.Count)
					{
						value4 = P1099necTCoName2[i];
					}


					string value5 = "";
					if (P1099necTContact != null && i < P1099necTContact.Count)
					{
						value5 = P1099necTContact[i];
					}


					string value6 = "";
					if (P1099necTContactEmail != null && i < P1099necTContactEmail.Count)
					{
						value6 = P1099necTContactEmail[i];
					}


					string value7 = "";
					if (P1099necTContactExtension != null && i < P1099necTContactExtension.Count)
					{
						value7 = P1099necTContactExtension[i];
					}


					string value8 = "";
					if (P1099necTContactPhone != null && i < P1099necTContactPhone.Count)
					{
						value8 = P1099necTContactPhone[i];
					}


					string value9 = "";
					if (P1099necTName != null && i < P1099necTName.Count)
					{
						value9 = P1099necTName[i];
					}


					string value10 = "";
					if (P1099necTName2 != null && i < P1099necTName2.Count)
					{
						value10 = P1099necTName2[i];
					}


					string value11 = "";
					if (P1099necTState != null && i < P1099necTState.Count)
					{
						value11 = P1099necTState[i];
					}


					string value12 = "";
					if (P1099necTTransmitterTin != null && i < P1099necTTransmitterTin.Count)
					{
						value12 = P1099necTTransmitterTin[i];
					}


					string value13 = "";
					if (P1099necTZip != null && i < P1099necTZip.Count)
					{
						value13 = P1099necTZip[i];
					}

					P1099necTransDmEntityAssociation.Add(new Parm1099necP1099necTransDm( value0, value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class Parm1099necP1099necBox
	{
		public string P1099necBoxNoAssocMember;	
		public string P1099necBoxDescAssocMember;	
		public string P1099necBoxMinBalAssocMember;	
		public string P1099necBoxTotFlagAssocMember;	
		public string P1099necBoxWithFlagAssocMember;	
		public Parm1099necP1099necBox() {}
		public Parm1099necP1099necBox(
			string inP1099necBoxNo,
			string inP1099necBoxDesc,
			string inP1099necBoxMinBal,
			string inP1099necBoxTotFlag,
			string inP1099necBoxWithFlag)
		{
			P1099necBoxNoAssocMember = inP1099necBoxNo;
			P1099necBoxDescAssocMember = inP1099necBoxDesc;
			P1099necBoxMinBalAssocMember = inP1099necBoxMinBal;
			P1099necBoxTotFlagAssocMember = inP1099necBoxTotFlag;
			P1099necBoxWithFlagAssocMember = inP1099necBoxWithFlag;
		}
	}
	
	[Serializable]
	public class Parm1099necP1099necMinBal
	{
		public string P1099necMinBalCodeAssocMember;	
		public Decimal? P1099necMinBalAmtAssocMember;	
		public string P1099necMinBalDescAssocMember;	
		public Parm1099necP1099necMinBal() {}
		public Parm1099necP1099necMinBal(
			string inP1099necMinBalCode,
			Decimal? inP1099necMinBalAmt,
			string inP1099necMinBalDesc)
		{
			P1099necMinBalCodeAssocMember = inP1099necMinBalCode;
			P1099necMinBalAmtAssocMember = inP1099necMinBalAmt;
			P1099necMinBalDescAssocMember = inP1099necMinBalDesc;
		}
	}
	
	[Serializable]
	public class Parm1099necP1099necTransDm
	{
		public string P1099necTTransmitterIdAssocMember;	
		public string P1099necTCoAddrAssocMember;	
		public string P1099necTCoCityAssocMember;	
		public string P1099necTCoNameAssocMember;	
		public string P1099necTCoName2AssocMember;	
		public string P1099necTContactAssocMember;	
		public string P1099necTContactEmailAssocMember;	
		public string P1099necTContactExtensionAssocMember;	
		public string P1099necTContactPhoneAssocMember;	
		public string P1099necTNameAssocMember;	
		public string P1099necTName2AssocMember;	
		public string P1099necTStateAssocMember;	
		public string P1099necTTransmitterTinAssocMember;	
		public string P1099necTZipAssocMember;	
		public Parm1099necP1099necTransDm() {}
		public Parm1099necP1099necTransDm(
			string inP1099necTTransmitterId,
			string inP1099necTCoAddr,
			string inP1099necTCoCity,
			string inP1099necTCoName,
			string inP1099necTCoName2,
			string inP1099necTContact,
			string inP1099necTContactEmail,
			string inP1099necTContactExtension,
			string inP1099necTContactPhone,
			string inP1099necTName,
			string inP1099necTName2,
			string inP1099necTState,
			string inP1099necTTransmitterTin,
			string inP1099necTZip)
		{
			P1099necTTransmitterIdAssocMember = inP1099necTTransmitterId;
			P1099necTCoAddrAssocMember = inP1099necTCoAddr;
			P1099necTCoCityAssocMember = inP1099necTCoCity;
			P1099necTCoNameAssocMember = inP1099necTCoName;
			P1099necTCoName2AssocMember = inP1099necTCoName2;
			P1099necTContactAssocMember = inP1099necTContact;
			P1099necTContactEmailAssocMember = inP1099necTContactEmail;
			P1099necTContactExtensionAssocMember = inP1099necTContactExtension;
			P1099necTContactPhoneAssocMember = inP1099necTContactPhone;
			P1099necTNameAssocMember = inP1099necTName;
			P1099necTName2AssocMember = inP1099necTName2;
			P1099necTStateAssocMember = inP1099necTState;
			P1099necTTransmitterTinAssocMember = inP1099necTTransmitterTin;
			P1099necTZipAssocMember = inP1099necTZip;
		}
	}
}