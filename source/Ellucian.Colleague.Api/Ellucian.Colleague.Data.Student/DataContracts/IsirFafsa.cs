//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 7/30/2020 8:59:05 AM by user sbhole1
//
//     Type: ENTITY
//     Entity: ISIR.FAFSA
//     Application: ST
//     Environment: dvcoll_wstst01
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
	[DataContract(Name = "IsirFafsa")]
	[ColleagueDataContract(GeneratedDateTime = "7/30/2020 8:59:05 AM", User = "sbhole1")]
	[EntityDataContract(EntityName = "ISIR.FAFSA", EntityType = "PHYS")]
	public class IsirFafsa : IColleagueGuidEntity
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
		/// CDD Name: IFAF.S.MARITAL.ST
		/// </summary>
		[DataMember(Order = 6, Name = "IFAF.S.MARITAL.ST")]
		public string IfafSMaritalSt { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.S.LEGAL.RES
		/// </summary>
		[DataMember(Order = 8, Name = "IFAF.S.LEGAL.RES")]
		public string IfafSLegalRes { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.INTEREST.CWS
		/// </summary>
		[DataMember(Order = 18, Name = "IFAF.INTEREST.CWS")]
		public string IfafInterestCws { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.FATHER.GRADE.LVL
		/// </summary>
		[DataMember(Order = 19, Name = "IFAF.FATHER.GRADE.LVL")]
		public string IfafFatherGradeLvl { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.MOTHER.GRADE.LVL
		/// </summary>
		[DataMember(Order = 20, Name = "IFAF.MOTHER.GRADE.LVL")]
		public string IfafMotherGradeLvl { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.S.TAX.RETURN.FILED
		/// </summary>
		[DataMember(Order = 22, Name = "IFAF.S.TAX.RETURN.FILED")]
		public string IfafSTaxReturnFiled { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.S.AGI
		/// </summary>
		[DataMember(Order = 25, Name = "IFAF.S.AGI")]
		public int? IfafSAgi { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.S.STUDENT.INC
		/// </summary>
		[DataMember(Order = 28, Name = "IFAF.S.STUDENT.INC")]
		public int? IfafSStudentInc { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.SPOUSE.INC
		/// </summary>
		[DataMember(Order = 29, Name = "IFAF.SPOUSE.INC")]
		public int? IfafSpouseInc { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.BORN.B4.DT
		/// </summary>
		[DataMember(Order = 38, Name = "IFAF.BORN.B4.DT")]
		public string IfafBornB4Dt { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.GRAD.PROF
		/// </summary>
		[DataMember(Order = 39, Name = "IFAF.GRAD.PROF")]
		public string IfafGradProf { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.MARRIED
		/// </summary>
		[DataMember(Order = 40, Name = "IFAF.MARRIED")]
		public string IfafMarried { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.DEPEND.CHILDREN
		/// </summary>
		[DataMember(Order = 41, Name = "IFAF.DEPEND.CHILDREN")]
		public string IfafDependChildren { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.OTHER.DEPEND
		/// </summary>
		[DataMember(Order = 42, Name = "IFAF.OTHER.DEPEND")]
		public string IfafOtherDepend { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.ORPHAN.WARD
		/// </summary>
		[DataMember(Order = 43, Name = "IFAF.ORPHAN.WARD")]
		public string IfafOrphanWard { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.VETERAN
		/// </summary>
		[DataMember(Order = 44, Name = "IFAF.VETERAN")]
		public string IfafVeteran { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.P.NBR.FAMILY
		/// </summary>
		[DataMember(Order = 55, Name = "IFAF.P.NBR.FAMILY")]
		public int? IfafPNbrFamily { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.P.NO.COLL
		/// </summary>
		[DataMember(Order = 56, Name = "IFAF.P.NO.COLL")]
		public int? IfafPNoColl { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.P.TAX.RETURN.FILED
		/// </summary>
		[DataMember(Order = 61, Name = "IFAF.P.TAX.RETURN.FILED")]
		public string IfafPTaxReturnFiled { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.P.AGI
		/// </summary>
		[DataMember(Order = 64, Name = "IFAF.P.AGI")]
		public int? IfafPAgi { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.P.FATHER.INC
		/// </summary>
		[DataMember(Order = 67, Name = "IFAF.P.FATHER.INC")]
		public int? IfafPFatherInc { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.P.MOTHER.INC
		/// </summary>
		[DataMember(Order = 68, Name = "IFAF.P.MOTHER.INC")]
		public int? IfafPMotherInc { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.S.NO.FAMILY
		/// </summary>
		[DataMember(Order = 75, Name = "IFAF.S.NO.FAMILY")]
		public int? IfafSNoFamily { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.S.NO.COLL
		/// </summary>
		[DataMember(Order = 76, Name = "IFAF.S.NO.COLL")]
		public int? IfafSNoColl { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.STUDENT.ID
		/// </summary>
		[DataMember(Order = 77, Name = "IFAF.STUDENT.ID")]
		public string IfafStudentId { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.DATE.SIGN
		/// </summary>
		[DataMember(Order = 79, Name = "IFAF.DATE.SIGN")]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[ColleagueDataMember(UseEnvisionInternalFormat = true)]
		public DateTime? IfafDateSign { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.CORRECTION.ID
		/// </summary>
		[DataMember(Order = 100, Name = "IFAF.CORRECTION.ID")]
		public string IfafCorrectionId { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.DEPEND.OVERRIDE
		/// </summary>
		[DataMember(Order = 101, Name = "IFAF.DEPEND.OVERRIDE")]
		public string IfafDependOverride { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.1
		/// </summary>
		[DataMember(Order = 110, Name = "IFAF.HOUSING.1")]
		public string IfafHousing1 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.2
		/// </summary>
		[DataMember(Order = 111, Name = "IFAF.HOUSING.2")]
		public string IfafHousing2 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.3
		/// </summary>
		[DataMember(Order = 112, Name = "IFAF.HOUSING.3")]
		public string IfafHousing3 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.4
		/// </summary>
		[DataMember(Order = 113, Name = "IFAF.HOUSING.4")]
		public string IfafHousing4 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.5
		/// </summary>
		[DataMember(Order = 114, Name = "IFAF.HOUSING.5")]
		public string IfafHousing5 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.6
		/// </summary>
		[DataMember(Order = 115, Name = "IFAF.HOUSING.6")]
		public string IfafHousing6 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.1
		/// </summary>
		[DataMember(Order = 116, Name = "IFAF.TITLEIV.1")]
		public string IfafTitleiv1 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.2
		/// </summary>
		[DataMember(Order = 117, Name = "IFAF.TITLEIV.2")]
		public string IfafTitleiv2 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.3
		/// </summary>
		[DataMember(Order = 118, Name = "IFAF.TITLEIV.3")]
		public string IfafTitleiv3 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.4
		/// </summary>
		[DataMember(Order = 119, Name = "IFAF.TITLEIV.4")]
		public string IfafTitleiv4 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.5
		/// </summary>
		[DataMember(Order = 120, Name = "IFAF.TITLEIV.5")]
		public string IfafTitleiv5 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.6
		/// </summary>
		[DataMember(Order = 121, Name = "IFAF.TITLEIV.6")]
		public string IfafTitleiv6 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.CORRECTED.FROM.ID
		/// </summary>
		[DataMember(Order = 123, Name = "IFAF.CORRECTED.FROM.ID")]
		public string IfafCorrectedFromId { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.ISIR.TYPE
		/// </summary>
		[DataMember(Order = 126, Name = "IFAF.ISIR.TYPE")]
		public string IfafIsirType { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.FAA.ADJ
		/// </summary>
		[DataMember(Order = 155, Name = "IFAF.FAA.ADJ")]
		public string IfafFaaAdj { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.IMPORT.YEAR
		/// </summary>
		[DataMember(Order = 171, Name = "IFAF.IMPORT.YEAR")]
		public string IfafImportYear { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.ACTIVE.DUTY
		/// </summary>
		[DataMember(Order = 173, Name = "IFAF.ACTIVE.DUTY")]
		public string IfafActiveDuty { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.7
		/// </summary>
		[DataMember(Order = 185, Name = "IFAF.HOUSING.7")]
		public string IfafHousing7 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.8
		/// </summary>
		[DataMember(Order = 186, Name = "IFAF.HOUSING.8")]
		public string IfafHousing8 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.9
		/// </summary>
		[DataMember(Order = 187, Name = "IFAF.HOUSING.9")]
		public string IfafHousing9 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOUSING.10
		/// </summary>
		[DataMember(Order = 188, Name = "IFAF.HOUSING.10")]
		public string IfafHousing10 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.7
		/// </summary>
		[DataMember(Order = 189, Name = "IFAF.TITLEIV.7")]
		public string IfafTitleiv7 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.8
		/// </summary>
		[DataMember(Order = 190, Name = "IFAF.TITLEIV.8")]
		public string IfafTitleiv8 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.9
		/// </summary>
		[DataMember(Order = 191, Name = "IFAF.TITLEIV.9")]
		public string IfafTitleiv9 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.TITLEIV.10
		/// </summary>
		[DataMember(Order = 192, Name = "IFAF.TITLEIV.10")]
		public string IfafTitleiv10 { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.EMANCIPATED.MINOR
		/// </summary>
		[DataMember(Order = 213, Name = "IFAF.EMANCIPATED.MINOR")]
		public string IfafEmancipatedMinor { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.LEGAL.GUARDIANSHIP
		/// </summary>
		[DataMember(Order = 214, Name = "IFAF.LEGAL.GUARDIANSHIP")]
		public string IfafLegalGuardianship { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOMELESS.BY.SCHOOL
		/// </summary>
		[DataMember(Order = 215, Name = "IFAF.HOMELESS.BY.SCHOOL")]
		public string IfafHomelessBySchool { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOMELESS.BY.HUD
		/// </summary>
		[DataMember(Order = 216, Name = "IFAF.HOMELESS.BY.HUD")]
		public string IfafHomelessByHud { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.HOMELESS.AT.RISK
		/// </summary>
		[DataMember(Order = 217, Name = "IFAF.HOMELESS.AT.RISK")]
		public string IfafHomelessAtRisk { get; set; }
		
		/// <summary>
		/// CDD Name: IFAF.S.INT.WORK.STUDY
		/// </summary>
		[DataMember(Order = 240, Name = "IFAF.S.INT.WORK.STUDY")]
		public string IfafSIntWorkStudy { get; set; }
		
	
		// build up all the Associated objects and add them to the properties
		public void buildAssociations()
		{	
			   
		}
	}
	
	// EntityAssociation classes
}