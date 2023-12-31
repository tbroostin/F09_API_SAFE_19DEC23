//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 12/14/2020 3:41:20 PM by user balvano3
//
//     Type: ENTITY
//     Entity: LDM.DEFAULTS
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
	[DataContract(Name = "LdmDefaults")]
	[ColleagueDataContract(GeneratedDateTime = "12/14/2020 3:41:20 PM", User = "balvano3")]
	[EntityDataContract(EntityName = "LDM.DEFAULTS", EntityType = "PERM")]
	public class LdmDefaults : IColleagueEntity
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
		/// CDD Name: LDMD.PERSON.DUPL.CRITERIA
		/// </summary>
		[DataMember(Order = 0, Name = "LDMD.PERSON.DUPL.CRITERIA")]
		public string LdmdPersonDuplCriteria { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.ADDR.DUPL.CRITERIA
		/// </summary>
		[DataMember(Order = 1, Name = "LDMD.ADDR.DUPL.CRITERIA")]
		public string LdmdAddrDuplCriteria { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.ADDR.TYPE.MAPPING
		/// </summary>
		[DataMember(Order = 2, Name = "LDMD.ADDR.TYPE.MAPPING")]
		public string LdmdAddrTypeMapping { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.EMAIL.TYPE.MAPPING
		/// </summary>
		[DataMember(Order = 3, Name = "LDMD.EMAIL.TYPE.MAPPING")]
		public string LdmdEmailTypeMapping { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.LDM.PHONE.TYPE
		/// </summary>
		[DataMember(Order = 4, Name = "LDMD.LDM.PHONE.TYPE")]
		public List<string> LdmdLdmPhoneType { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.COLL.PHONE.TYPES
		/// </summary>
		[DataMember(Order = 5, Name = "LDMD.COLL.PHONE.TYPES")]
		public List<string> LdmdCollPhoneTypes { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.COLL.FIELD.NAME
		/// </summary>
		[DataMember(Order = 6, Name = "LDMD.COLL.FIELD.NAME")]
		public List<string> LdmdCollFieldName { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.COLL.DEFAULT.VALUE
		/// </summary>
		[DataMember(Order = 7, Name = "LDMD.COLL.DEFAULT.VALUE")]
		public List<string> LdmdCollDefaultValue { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.SUBJ.DEPT.MAPPING
		/// </summary>
		[DataMember(Order = 8, Name = "LDMD.SUBJ.DEPT.MAPPING")]
		public string LdmdSubjDeptMapping { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.COLL.FILE.NAME
		/// </summary>
		[DataMember(Order = 9, Name = "LDMD.COLL.FILE.NAME")]
		public List<string> LdmdCollFileName { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.COLL.FIELD.NUMBER
		/// </summary>
		[DataMember(Order = 10, Name = "LDMD.COLL.FIELD.NUMBER")]
		public List<int?> LdmdCollFieldNumber { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.COURSE.ACT.STATUS
		/// </summary>
		[DataMember(Order = 11, Name = "LDMD.COURSE.ACT.STATUS")]
		public string LdmdCourseActStatus { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.COURSE.INACT.STATUS
		/// </summary>
		[DataMember(Order = 12, Name = "LDMD.COURSE.INACT.STATUS")]
		public string LdmdCourseInactStatus { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.SECTION.ACT.STATUS
		/// </summary>
		[DataMember(Order = 13, Name = "LDMD.SECTION.ACT.STATUS")]
		public string LdmdSectionActStatus { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.SECTION.INACT.STATUS
		/// </summary>
		[DataMember(Order = 14, Name = "LDMD.SECTION.INACT.STATUS")]
		public string LdmdSectionInactStatus { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.REQD.COLL.FIELDS
		/// </summary>
		[DataMember(Order = 15, Name = "LDMD.REQD.COLL.FIELDS")]
		public List<string> LdmdReqdCollFields { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.CHECK.FACULTY.LOAD
		/// </summary>
		[DataMember(Order = 19, Name = "LDMD.CHECK.FACULTY.LOAD")]
		public string LdmdCheckFacultyLoad { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.REG.USERS.ID
		/// </summary>
		[DataMember(Order = 22, Name = "LDMD.REG.USERS.ID")]
		public string LdmdRegUsersId { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DEFAULT.PRIVACY.CODE
		/// </summary>
		[DataMember(Order = 23, Name = "LDMD.DEFAULT.PRIVACY.CODE")]
		public string LdmdDefaultPrivacyCode { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.GUARDIAN.REL.TYPES
		/// </summary>
		[DataMember(Order = 24, Name = "LDMD.GUARDIAN.REL.TYPES")]
		public List<string> LdmdGuardianRelTypes { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DEFAULT.AR.TYPE
		/// </summary>
		[DataMember(Order = 25, Name = "LDMD.DEFAULT.AR.TYPE")]
		public string LdmdDefaultArType { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.CHARGE.TYPES
		/// </summary>
		[DataMember(Order = 26, Name = "LDMD.CHARGE.TYPES")]
		public List<string> LdmdChargeTypes { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DEFAULT.AR.CODES
		/// </summary>
		[DataMember(Order = 27, Name = "LDMD.DEFAULT.AR.CODES")]
		public List<string> LdmdDefaultArCodes { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.EXCLUDE.BENEFITS
		/// </summary>
		[DataMember(Order = 28, Name = "LDMD.EXCLUDE.BENEFITS")]
		public List<string> LdmdExcludeBenefits { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.POSITION.LEAVE.REASONS
		/// </summary>
		[DataMember(Order = 29, Name = "LDMD.POSITION.LEAVE.REASONS")]
		public List<string> LdmdPositionLeaveReasons { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PAY.DEDUCT.INTERVAL
		/// </summary>
		[DataMember(Order = 30, Name = "LDMD.PAY.DEDUCT.INTERVAL")]
		public List<long?> LdmdPayDeductInterval { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PAY.DEDUCT.PERIOD
		/// </summary>
		[DataMember(Order = 31, Name = "LDMD.PAY.DEDUCT.PERIOD")]
		public List<string> LdmdPayDeductPeriod { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.COMMITMENT.TYPES
		/// </summary>
		[DataMember(Order = 32, Name = "LDMD.COMMITMENT.TYPES")]
		public List<string> LdmdCommitmentTypes { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.BENDED.CODE
		/// </summary>
		[DataMember(Order = 33, Name = "LDMD.BENDED.CODE")]
		public List<string> LdmdBendedCode { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PAY.DEDUCT.MONTHLY
		/// </summary>
		[DataMember(Order = 34, Name = "LDMD.PAY.DEDUCT.MONTHLY")]
		public List<long?> LdmdPayDeductMonthly { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PAY.DEDUCT.MO.PERIOD
		/// </summary>
		[DataMember(Order = 35, Name = "LDMD.PAY.DEDUCT.MO.PERIOD")]
		public List<string> LdmdPayDeductMoPeriod { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.LEAVE.STATUS.CODES
		/// </summary>
		[DataMember(Order = 36, Name = "LDMD.LEAVE.STATUS.CODES")]
		public List<string> LdmdLeaveStatusCodes { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DEFAULT.DISTR
		/// </summary>
		[DataMember(Order = 37, Name = "LDMD.DEFAULT.DISTR")]
		public string LdmdDefaultDistr { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.CASHIER
		/// </summary>
		[DataMember(Order = 38, Name = "LDMD.CASHIER")]
		public string LdmdCashier { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DEFAULT.SPONSOR
		/// </summary>
		[DataMember(Order = 39, Name = "LDMD.DEFAULT.SPONSOR")]
		public string LdmdDefaultSponsor { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.SPONSOR.AR.TYPE
		/// </summary>
		[DataMember(Order = 40, Name = "LDMD.SPONSOR.AR.TYPE")]
		public string LdmdSponsorArType { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.GNGE.EXCLUDE.ENTITIES
		/// </summary>
		[DataMember(Order = 41, Name = "LDMD.GNGE.EXCLUDE.ENTITIES")]
		public List<string> LdmdGngeExcludeEntities { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PAYMENT.METHOD
		/// </summary>
		[DataMember(Order = 42, Name = "LDMD.PAYMENT.METHOD")]
		public string LdmdPaymentMethod { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DFLT.RES.LIFE.AR.TYPE
		/// </summary>
		[DataMember(Order = 43, Name = "LDMD.DFLT.RES.LIFE.AR.TYPE")]
		public string LdmdDfltResLifeArType { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.GNGE.MAN.GUID.ENTITIES
		/// </summary>
		[DataMember(Order = 44, Name = "LDMD.GNGE.MAN.GUID.ENTITIES")]
		public List<string> LdmdGngeManGuidEntities { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DFLT.ADM.OFFICE.CODES
		/// </summary>
		[DataMember(Order = 45, Name = "LDMD.DFLT.ADM.OFFICE.CODES")]
		public List<string> LdmdDfltAdmOfficeCodes { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DEFAULT.APPL.STATUS
		/// </summary>
		[DataMember(Order = 46, Name = "LDMD.DEFAULT.APPL.STATUS")]
		public string LdmdDefaultApplStatus { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DEFAULT.APPL.STAT.STAFF
		/// </summary>
		[DataMember(Order = 47, Name = "LDMD.DEFAULT.APPL.STAT.STAFF")]
		public string LdmdDefaultApplStatStaff { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.SPONSOR.AR.CODE
		/// </summary>
		[DataMember(Order = 48, Name = "LDMD.SPONSOR.AR.CODE")]
		public string LdmdSponsorArCode { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.CHECK.POSTING.PERFORMED
		/// </summary>
		[DataMember(Order = 50, Name = "LDMD.CHECK.POSTING.PERFORMED")]
		public string LdmdCheckPostingPerformed { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PRIN.INVESTIGATOR.ROLE
		/// </summary>
		[DataMember(Order = 51, Name = "LDMD.PRIN.INVESTIGATOR.ROLE")]
		public string LdmdPrinInvestigatorRole { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.ACAD.LEVELS
		/// </summary>
		[DataMember(Order = 52, Name = "LDMD.ACAD.LEVELS")]
		public List<string> LdmdAcadLevels { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.ACAD.PROGRAMS
		/// </summary>
		[DataMember(Order = 53, Name = "LDMD.ACAD.PROGRAMS")]
		public List<string> LdmdAcadPrograms { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.STU.PROGRAM.STATUSES
		/// </summary>
		[DataMember(Order = 54, Name = "LDMD.STU.PROGRAM.STATUSES")]
		public List<string> LdmdStuProgramStatuses { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PRSPCT.APPL.STATUS
		/// </summary>
		[DataMember(Order = 55, Name = "LDMD.PRSPCT.APPL.STATUS")]
		public string LdmdPrspctApplStatus { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PRSPCT.APPL.STAT.STAFF
		/// </summary>
		[DataMember(Order = 56, Name = "LDMD.PRSPCT.APPL.STAT.STAFF")]
		public string LdmdPrspctApplStatStaff { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.MAPPING.CONTROL
		/// </summary>
		[DataMember(Order = 57, Name = "LDMD.MAPPING.CONTROL")]
		public string LdmdMappingControl { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.PROSPECT.DUPL.CRITERIA
		/// </summary>
		[DataMember(Order = 58, Name = "LDMD.PROSPECT.DUPL.CRITERIA")]
		public string LdmdProspectDuplCriteria { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.INCLUDE.ENRL.HEADCOUNTS
		/// </summary>
		[DataMember(Order = 59, Name = "LDMD.INCLUDE.ENRL.HEADCOUNTS")]
		public List<string> LdmdIncludeEnrlHeadcounts { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.RELATION.DUPL.CRITERIA
		/// </summary>
		[DataMember(Order = 60, Name = "LDMD.RELATION.DUPL.CRITERIA")]
		public string LdmdRelationDuplCriteria { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.VENCONTACT.DUP.CRITERIA
		/// </summary>
		[DataMember(Order = 61, Name = "LDMD.VENCONTACT.DUP.CRITERIA")]
		public string LdmdVencontactDupCriteria { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.DEFAULT.AP.TYPE
		/// </summary>
		[DataMember(Order = 62, Name = "LDMD.DEFAULT.AP.TYPE")]
		public string LdmdDefaultApType { get; set; }
		
		/// <summary>
		/// CDD Name: LDMD.ALLOW.MOVE.TO.STU
		/// </summary>
		[DataMember(Order = 63, Name = "LDMD.ALLOW.MOVE.TO.STU")]
		public string LdmdAllowMoveToStu { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<LdmDefaultsLdmdPhoneTypeMapping> LdmdPhoneTypeMappingEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<LdmDefaultsLdmdCollDefaults> LdmdCollDefaultsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<LdmDefaultsLdmdArDefaults> LdmdArDefaultsEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<LdmDefaultsLdmdPayPeriod> LdmdPayPeriodEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<LdmDefaultsLdmdCommitment> LdmdCommitmentEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<LdmDefaultsLdmdPayPeriodMo> LdmdPayPeriodMoEntityAssociation { get; set; }
		
		/// <summary>
		/// Entity assocation member
		/// </summary>
		[DataMember]
		public List<LdmDefaultsLdmdDefaultPrograms> LdmdDefaultProgramsEntityAssociation { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			// EntityAssociation Name: LDMD.PHONE.TYPE.MAPPING
			
			LdmdPhoneTypeMappingEntityAssociation = new List<LdmDefaultsLdmdPhoneTypeMapping>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(LdmdLdmPhoneType != null)
			{
				int numLdmdPhoneTypeMapping = LdmdLdmPhoneType.Count;
				if (LdmdCollPhoneTypes !=null && LdmdCollPhoneTypes.Count > numLdmdPhoneTypeMapping) numLdmdPhoneTypeMapping = LdmdCollPhoneTypes.Count;

				for (int i = 0; i < numLdmdPhoneTypeMapping; i++)
				{

					string value0 = "";
					if (LdmdLdmPhoneType != null && i < LdmdLdmPhoneType.Count)
					{
						value0 = LdmdLdmPhoneType[i];
					}


					string value1 = "";
					if (LdmdCollPhoneTypes != null && i < LdmdCollPhoneTypes.Count)
					{
						value1 = LdmdCollPhoneTypes[i];
					}

					LdmdPhoneTypeMappingEntityAssociation.Add(new LdmDefaultsLdmdPhoneTypeMapping( value0, value1));
				}
			}
			// EntityAssociation Name: LDMD.COLL.DEFAULTS
			
			LdmdCollDefaultsEntityAssociation = new List<LdmDefaultsLdmdCollDefaults>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(LdmdCollFieldName != null)
			{
				int numLdmdCollDefaults = LdmdCollFieldName.Count;
				if (LdmdCollDefaultValue !=null && LdmdCollDefaultValue.Count > numLdmdCollDefaults) numLdmdCollDefaults = LdmdCollDefaultValue.Count;
				if (LdmdCollFileName !=null && LdmdCollFileName.Count > numLdmdCollDefaults) numLdmdCollDefaults = LdmdCollFileName.Count;
				if (LdmdCollFieldNumber !=null && LdmdCollFieldNumber.Count > numLdmdCollDefaults) numLdmdCollDefaults = LdmdCollFieldNumber.Count;

				for (int i = 0; i < numLdmdCollDefaults; i++)
				{

					string value0 = "";
					if (LdmdCollFieldName != null && i < LdmdCollFieldName.Count)
					{
						value0 = LdmdCollFieldName[i];
					}


					string value1 = "";
					if (LdmdCollDefaultValue != null && i < LdmdCollDefaultValue.Count)
					{
						value1 = LdmdCollDefaultValue[i];
					}


					string value2 = "";
					if (LdmdCollFileName != null && i < LdmdCollFileName.Count)
					{
						value2 = LdmdCollFileName[i];
					}


					int? value3 = null;
					if (LdmdCollFieldNumber != null && i < LdmdCollFieldNumber.Count)
					{
						value3 = LdmdCollFieldNumber[i];
					}

					LdmdCollDefaultsEntityAssociation.Add(new LdmDefaultsLdmdCollDefaults( value0, value1, value2, value3));
				}
			}
			// EntityAssociation Name: LDMD.AR.DEFAULTS
			
			LdmdArDefaultsEntityAssociation = new List<LdmDefaultsLdmdArDefaults>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(LdmdChargeTypes != null)
			{
				int numLdmdArDefaults = LdmdChargeTypes.Count;
				if (LdmdDefaultArCodes !=null && LdmdDefaultArCodes.Count > numLdmdArDefaults) numLdmdArDefaults = LdmdDefaultArCodes.Count;

				for (int i = 0; i < numLdmdArDefaults; i++)
				{

					string value0 = "";
					if (LdmdChargeTypes != null && i < LdmdChargeTypes.Count)
					{
						value0 = LdmdChargeTypes[i];
					}


					string value1 = "";
					if (LdmdDefaultArCodes != null && i < LdmdDefaultArCodes.Count)
					{
						value1 = LdmdDefaultArCodes[i];
					}

					LdmdArDefaultsEntityAssociation.Add(new LdmDefaultsLdmdArDefaults( value0, value1));
				}
			}
			// EntityAssociation Name: LDMD.PAY.PERIOD
			
			LdmdPayPeriodEntityAssociation = new List<LdmDefaultsLdmdPayPeriod>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(LdmdPayDeductInterval != null)
			{
				int numLdmdPayPeriod = LdmdPayDeductInterval.Count;
				if (LdmdPayDeductPeriod !=null && LdmdPayDeductPeriod.Count > numLdmdPayPeriod) numLdmdPayPeriod = LdmdPayDeductPeriod.Count;

				for (int i = 0; i < numLdmdPayPeriod; i++)
				{

					long? value0 = null;
					if (LdmdPayDeductInterval != null && i < LdmdPayDeductInterval.Count)
					{
						value0 = LdmdPayDeductInterval[i];
					}


					string value1 = "";
					if (LdmdPayDeductPeriod != null && i < LdmdPayDeductPeriod.Count)
					{
						value1 = LdmdPayDeductPeriod[i];
					}

					LdmdPayPeriodEntityAssociation.Add(new LdmDefaultsLdmdPayPeriod( value0, value1));
				}
			}
			// EntityAssociation Name: LDMD.COMMITMENT
			
			LdmdCommitmentEntityAssociation = new List<LdmDefaultsLdmdCommitment>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(LdmdCommitmentTypes != null)
			{
				int numLdmdCommitment = LdmdCommitmentTypes.Count;
				if (LdmdBendedCode !=null && LdmdBendedCode.Count > numLdmdCommitment) numLdmdCommitment = LdmdBendedCode.Count;

				for (int i = 0; i < numLdmdCommitment; i++)
				{

					string value0 = "";
					if (LdmdCommitmentTypes != null && i < LdmdCommitmentTypes.Count)
					{
						value0 = LdmdCommitmentTypes[i];
					}


					string value1 = "";
					if (LdmdBendedCode != null && i < LdmdBendedCode.Count)
					{
						value1 = LdmdBendedCode[i];
					}

					LdmdCommitmentEntityAssociation.Add(new LdmDefaultsLdmdCommitment( value0, value1));
				}
			}
			// EntityAssociation Name: LDMD.PAY.PERIOD.MO
			
			LdmdPayPeriodMoEntityAssociation = new List<LdmDefaultsLdmdPayPeriodMo>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(LdmdPayDeductMonthly != null)
			{
				int numLdmdPayPeriodMo = LdmdPayDeductMonthly.Count;
				if (LdmdPayDeductMoPeriod !=null && LdmdPayDeductMoPeriod.Count > numLdmdPayPeriodMo) numLdmdPayPeriodMo = LdmdPayDeductMoPeriod.Count;

				for (int i = 0; i < numLdmdPayPeriodMo; i++)
				{

					long? value0 = null;
					if (LdmdPayDeductMonthly != null && i < LdmdPayDeductMonthly.Count)
					{
						value0 = LdmdPayDeductMonthly[i];
					}


					string value1 = "";
					if (LdmdPayDeductMoPeriod != null && i < LdmdPayDeductMoPeriod.Count)
					{
						value1 = LdmdPayDeductMoPeriod[i];
					}

					LdmdPayPeriodMoEntityAssociation.Add(new LdmDefaultsLdmdPayPeriodMo( value0, value1));
				}
			}
			// EntityAssociation Name: LDMD.DEFAULT.PROGRAMS
			
			LdmdDefaultProgramsEntityAssociation = new List<LdmDefaultsLdmdDefaultPrograms>();
			// Set max length to the count of controller when initializing.
			//Update max length if any of the association members has higher length than controller length
			if(LdmdAcadLevels != null)
			{
				int numLdmdDefaultPrograms = LdmdAcadLevels.Count;
				if (LdmdAcadPrograms !=null && LdmdAcadPrograms.Count > numLdmdDefaultPrograms) numLdmdDefaultPrograms = LdmdAcadPrograms.Count;
				if (LdmdStuProgramStatuses !=null && LdmdStuProgramStatuses.Count > numLdmdDefaultPrograms) numLdmdDefaultPrograms = LdmdStuProgramStatuses.Count;

				for (int i = 0; i < numLdmdDefaultPrograms; i++)
				{

					string value0 = "";
					if (LdmdAcadLevels != null && i < LdmdAcadLevels.Count)
					{
						value0 = LdmdAcadLevels[i];
					}


					string value1 = "";
					if (LdmdAcadPrograms != null && i < LdmdAcadPrograms.Count)
					{
						value1 = LdmdAcadPrograms[i];
					}


					string value2 = "";
					if (LdmdStuProgramStatuses != null && i < LdmdStuProgramStatuses.Count)
					{
						value2 = LdmdStuProgramStatuses[i];
					}

					LdmdDefaultProgramsEntityAssociation.Add(new LdmDefaultsLdmdDefaultPrograms( value0, value1, value2));
				}
			}
			   
		}
	}
	
	// EntityAssociation classes
	
	[Serializable]
	public class LdmDefaultsLdmdPhoneTypeMapping
	{
		public string LdmdLdmPhoneTypeAssocMember;	
		public string LdmdCollPhoneTypesAssocMember;	
		public LdmDefaultsLdmdPhoneTypeMapping() {}
		public LdmDefaultsLdmdPhoneTypeMapping(
			string inLdmdLdmPhoneType,
			string inLdmdCollPhoneTypes)
		{
			LdmdLdmPhoneTypeAssocMember = inLdmdLdmPhoneType;
			LdmdCollPhoneTypesAssocMember = inLdmdCollPhoneTypes;
		}
	}
	
	[Serializable]
	public class LdmDefaultsLdmdCollDefaults
	{
		public string LdmdCollFieldNameAssocMember;	
		public string LdmdCollDefaultValueAssocMember;	
		public string LdmdCollFileNameAssocMember;	
		public int? LdmdCollFieldNumberAssocMember;	
		public LdmDefaultsLdmdCollDefaults() {}
		public LdmDefaultsLdmdCollDefaults(
			string inLdmdCollFieldName,
			string inLdmdCollDefaultValue,
			string inLdmdCollFileName,
			int? inLdmdCollFieldNumber)
		{
			LdmdCollFieldNameAssocMember = inLdmdCollFieldName;
			LdmdCollDefaultValueAssocMember = inLdmdCollDefaultValue;
			LdmdCollFileNameAssocMember = inLdmdCollFileName;
			LdmdCollFieldNumberAssocMember = inLdmdCollFieldNumber;
		}
	}
	
	[Serializable]
	public class LdmDefaultsLdmdArDefaults
	{
		public string LdmdChargeTypesAssocMember;	
		public string LdmdDefaultArCodesAssocMember;	
		public LdmDefaultsLdmdArDefaults() {}
		public LdmDefaultsLdmdArDefaults(
			string inLdmdChargeTypes,
			string inLdmdDefaultArCodes)
		{
			LdmdChargeTypesAssocMember = inLdmdChargeTypes;
			LdmdDefaultArCodesAssocMember = inLdmdDefaultArCodes;
		}
	}
	
	[Serializable]
	public class LdmDefaultsLdmdPayPeriod
	{
		public long? LdmdPayDeductIntervalAssocMember;	
		public string LdmdPayDeductPeriodAssocMember;	
		public LdmDefaultsLdmdPayPeriod() {}
		public LdmDefaultsLdmdPayPeriod(
			long? inLdmdPayDeductInterval,
			string inLdmdPayDeductPeriod)
		{
			LdmdPayDeductIntervalAssocMember = inLdmdPayDeductInterval;
			LdmdPayDeductPeriodAssocMember = inLdmdPayDeductPeriod;
		}
	}
	
	[Serializable]
	public class LdmDefaultsLdmdCommitment
	{
		public string LdmdCommitmentTypesAssocMember;	
		public string LdmdBendedCodeAssocMember;	
		public LdmDefaultsLdmdCommitment() {}
		public LdmDefaultsLdmdCommitment(
			string inLdmdCommitmentTypes,
			string inLdmdBendedCode)
		{
			LdmdCommitmentTypesAssocMember = inLdmdCommitmentTypes;
			LdmdBendedCodeAssocMember = inLdmdBendedCode;
		}
	}
	
	[Serializable]
	public class LdmDefaultsLdmdPayPeriodMo
	{
		public long? LdmdPayDeductMonthlyAssocMember;	
		public string LdmdPayDeductMoPeriodAssocMember;	
		public LdmDefaultsLdmdPayPeriodMo() {}
		public LdmDefaultsLdmdPayPeriodMo(
			long? inLdmdPayDeductMonthly,
			string inLdmdPayDeductMoPeriod)
		{
			LdmdPayDeductMonthlyAssocMember = inLdmdPayDeductMonthly;
			LdmdPayDeductMoPeriodAssocMember = inLdmdPayDeductMoPeriod;
		}
	}
	
	[Serializable]
	public class LdmDefaultsLdmdDefaultPrograms
	{
		public string LdmdAcadLevelsAssocMember;	
		public string LdmdAcadProgramsAssocMember;	
		public string LdmdStuProgramStatusesAssocMember;	
		public LdmDefaultsLdmdDefaultPrograms() {}
		public LdmDefaultsLdmdDefaultPrograms(
			string inLdmdAcadLevels,
			string inLdmdAcadPrograms,
			string inLdmdStuProgramStatuses)
		{
			LdmdAcadLevelsAssocMember = inLdmdAcadLevels;
			LdmdAcadProgramsAssocMember = inLdmdAcadPrograms;
			LdmdStuProgramStatusesAssocMember = inLdmdStuProgramStatuses;
		}
	}
}