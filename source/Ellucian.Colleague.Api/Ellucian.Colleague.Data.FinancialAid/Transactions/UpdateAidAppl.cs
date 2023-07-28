//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 30-01-2023 6.35.51 PM by user deepak_p
//
//     Type: CTX
//     Transaction ID: UPDATE.AID.APPL
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

namespace Ellucian.Colleague.Data.FinancialAid.Transactions
{
	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.AID.APPL", GeneratedDateTime = "30-01-2023 6.35.51 PM", User = "deepak_p")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateAidApplRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.NAMES", InBoundData = true)]        
		public List<string> ExtendedNames { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EXTENDED.VALUES", InBoundData = true)]        
		public List<string> ExtendedValues { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "APPL.ID", InBoundData = true)]        
		public string ApplId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "IDEM.ID", InBoundData = true)]        
		public string IdemId { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "YEAR", InBoundData = true)]        
		public string Year { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "TYPE", InBoundData = true)]        
		public string Type { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "STUDENT.ID", InBoundData = true)]        
		public string StudentId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.MARITAL.STATUS", InBoundData = true)]        
		public string SMaritalStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ACTIVE.DUTY", InBoundData = true)]        
		public string ActiveDuty { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "BORN.B4.DT", InBoundData = true)]        
		public string BornB4Dt { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "DATE.CMPL", InBoundData = true)]        
		public Nullable<DateTime> DateCmpl { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "DEG.OR.CERT", InBoundData = true)]        
		public string DegOrCert { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "DEGREE.BY", InBoundData = true)]        
		public string DegreeBy { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "DEPEND.CHILDREN", InBoundData = true)]        
		public string DependChildren { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "EMANCIPATED.MINOR", InBoundData = true)]        
		public string EmancipatedMinor { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GRAD.PROF", InBoundData = true)]        
		public string GradProf { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "GRADE.LEVEL", InBoundData = true)]        
		public string GradeLevel { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOMELESS.AT.RISK", InBoundData = true)]        
		public string HomelessAtRisk { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOMELESS.BY.HUD", InBoundData = true)]        
		public string HomelessByHud { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOMELESS.BY.SCHOOL", InBoundData = true)]        
		public string HomelessBySchool { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.1", InBoundData = true)]        
		public string Housing1 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.1.PLAN", InBoundData = true)]        
		public string Housing1Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.2", InBoundData = true)]        
		public string Housing2 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.2.PLAN", InBoundData = true)]        
		public string Housing2Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.10", InBoundData = true)]        
		public string Housing10 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.10.PLAN", InBoundData = true)]        
		public string Housing10Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.3", InBoundData = true)]        
		public string Housing3 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.3.PLAN", InBoundData = true)]        
		public string Housing3Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.4", InBoundData = true)]        
		public string Housing4 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.4.PLAN", InBoundData = true)]        
		public string Housing4Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.5", InBoundData = true)]        
		public string Housing5 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.5.PLAN", InBoundData = true)]        
		public string Housing5Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.6", InBoundData = true)]        
		public string Housing6 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.6.PLAN", InBoundData = true)]        
		public string Housing6Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.7", InBoundData = true)]        
		public string Housing7 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.7.PLAN", InBoundData = true)]        
		public string Housing7Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.8", InBoundData = true)]        
		public string Housing8 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.8.PLAN", InBoundData = true)]        
		public string Housing8Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.9", InBoundData = true)]        
		public string Housing9 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HOUSING.9.PLAN", InBoundData = true)]        
		public string Housing9Plan { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HS.CITY", InBoundData = true)]        
		public string HsCity { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HS.CODE", InBoundData = true)]        
		public string HsCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HS.GRAD.TYPE", InBoundData = true)]        
		public string HsGradType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HS.NAME", InBoundData = true)]        
		public string HsName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "HS.STATE", InBoundData = true)]        
		public string HsState { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "LEGAL.GUARDIANSHIP", InBoundData = true)]        
		public string LegalGuardianship { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "MARRIED", InBoundData = true)]        
		public string Married { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ORPHAN.WARD", InBoundData = true)]        
		public string OrphanWard { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "OTHER.DEPEND", InBoundData = true)]        
		public string OtherDepend { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.AGI", InBoundData = true)]        
		public Nullable<int> PAgi { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.BUS.NET.WORTH", InBoundData = true)]        
		public Nullable<int> PBusNetWorth { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.CASH", InBoundData = true)]        
		public Nullable<int> PCash { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.CHILD.SUP.RCVD", InBoundData = true)]        
		public Nullable<int> PChildSupRcvd { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.CHILD.SUPPORT.PD", InBoundData = true)]        
		public Nullable<int> PChildSupportPd { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.CO.OP.EARNINGS", InBoundData = true)]        
		public Nullable<int> PCoOpEarnings { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.DIS.WORKER", InBoundData = true)]        
		public string PDisWorker { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.EDU.CREDIT", InBoundData = true)]        
		public Nullable<int> PEduCredit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.FOOD.STAMPS", InBoundData = true)]        
		public string PFoodStamps { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.GRANT.SCHOL.AID", InBoundData = true)]        
		public Nullable<int> PGrantScholAid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.INV.NET.WORTH", InBoundData = true)]        
		public Nullable<int> PInvNetWorth { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.IRA.PYMTS", InBoundData = true)]        
		public Nullable<int> PIraPymts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.LEGAL.RES.B4", InBoundData = true)]        
		public string PLegalResB4 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.LEGAL.RES.DATE", InBoundData = true)]        
		public string PLegalResDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.LEGAL.RES.ST", InBoundData = true)]        
		public string PLegalResSt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.LUNCH.BEN", InBoundData = true)]        
		public string PLunchBen { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.MARITAL.DATE", InBoundData = true)]        
		public string PMaritalDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.MARITAL.STATUS", InBoundData = true)]        
		public string PMaritalStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.MIL.CLER.ALLOW", InBoundData = true)]        
		public Nullable<int> PMilClerAllow { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.NBR.COLLEGE", InBoundData = true)]        
		public Nullable<int> PNbrCollege { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.NBR.FAMILY", InBoundData = true)]        
		public Nullable<int> PNbrFamily { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.NEED.BASED.EMP", InBoundData = true)]        
		public Nullable<int> PNeedBasedEmp { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.OTHER.UNTX.INC", InBoundData = true)]        
		public Nullable<int> POtherUntxInc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.PENSION.PYMTS", InBoundData = true)]        
		public Nullable<int> PPensionPymts { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.SCHED1", InBoundData = true)]        
		public string PSched1 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.SSI.BEN", InBoundData = true)]        
		public string PSsiBen { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.TANF", InBoundData = true)]        
		public string PTanf { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.TAX.FILING.STATUS", InBoundData = true)]        
		public string PTaxFilingStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.TAX.FORM.TYPE", InBoundData = true)]        
		public string PTaxFormType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.TAX.RETURN.FILED", InBoundData = true)]        
		public string PTaxReturnFiled { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.UNTX.INT.INC", InBoundData = true)]        
		public Nullable<int> PUntxIntInc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.UNTX.IRA.PEN", InBoundData = true)]        
		public Nullable<int> PUntxIraPen { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.US.TAX.PAID", InBoundData = true)]        
		public Nullable<int> PUsTaxPaid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.VET.NON.ED.BEN", InBoundData = true)]        
		public Nullable<int> PVetNonEdBen { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.WIC", InBoundData = true)]        
		public string PWic { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "P1.DOB", InBoundData = true)]        
		public Nullable<DateTime> P1Dob { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P1.FIRST.INIT", InBoundData = true)]        
		public string P1FirstInit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P1.GRADE.LVL", InBoundData = true)]        
		public string P1GradeLvl { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P1.INCOME", InBoundData = true)]        
		public Nullable<int> P1Income { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P1.LAST.NAME", InBoundData = true)]        
		public string P1LastName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P1.SSN", InBoundData = true)]        
		public Nullable<int> P1Ssn { get; set; }

		[DataMember]
		[DisplayFormat(DataFormatString = "{0:d}")]
		[SctrqDataMember(AppServerName = "P2.DOB", InBoundData = true)]        
		public Nullable<DateTime> P2Dob { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P2.FIRST.INIT", InBoundData = true)]        
		public string P2FirstInit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P2.GRADE.LVL", InBoundData = true)]        
		public string P2GradeLvl { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P2.INCOME", InBoundData = true)]        
		public Nullable<int> P2Income { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P2.LAST.NAME", InBoundData = true)]        
		public string P2LastName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P2.SSN", InBoundData = true)]        
		public Nullable<int> P2Ssn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PARENT.EMAIL", InBoundData = true)]        
		public string ParentEmail { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PREPARER.EIN", InBoundData = true)]        
		public Nullable<int> PreparerEin { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PREPARER.SIGNED", InBoundData = true)]        
		public string PreparerSigned { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "PREPARER.SSN", InBoundData = true)]        
		public Nullable<int> PreparerSsn { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.AGI", InBoundData = true)]        
		public Nullable<int> SAgi { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.BUS.NET.WORTH", InBoundData = true)]        
		public Nullable<int> SBusNetWorth { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.CASH", InBoundData = true)]        
		public Nullable<int> SCash { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.CHILD.SUP.RECV", InBoundData = true)]        
		public Nullable<int> SChildSupRecv { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.CHILD.SUPPORT.PD", InBoundData = true)]        
		public Nullable<int> SChildSupportPd { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.CO.OP.EARNINGS", InBoundData = true)]        
		public Nullable<int> SCoOpEarnings { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.COMBAT.PAY", InBoundData = true)]        
		public Nullable<int> SCombatPay { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.DISL.WORKER", InBoundData = true)]        
		public string SDislWorker { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.EDU.CREDIT", InBoundData = true)]        
		public Nullable<int> SEduCredit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.FOOD.STAMPS", InBoundData = true)]        
		public string SFoodStamps { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.GRANT.SCHOL.AID", InBoundData = true)]        
		public Nullable<int> SGrantScholAid { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.INTEREST.INCOME", InBoundData = true)]        
		public Nullable<int> SInterestIncome { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.INV.NET.WORTH", InBoundData = true)]        
		public Nullable<int> SInvNetWorth { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.IRA.PAYMENTS", InBoundData = true)]        
		public Nullable<int> SIraPayments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.LEGAL.RES.B4", InBoundData = true)]        
		public string SLegalResB4 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.LEGAL.RES.DATE", InBoundData = true)]        
		public string SLegalResDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.LEGAL.RES.ST", InBoundData = true)]        
		public string SLegalResSt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.LUNCH.BEN", InBoundData = true)]        
		public string SLunchBen { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.MARITAL.DATE", InBoundData = true)]        
		public string SMaritalDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.MILITARY.CLERGY.ALLOW", InBoundData = true)]        
		public Nullable<int> SMilitaryClergyAllow { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.NBR.COLLEGE", InBoundData = true)]        
		public Nullable<int> SNbrCollege { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.NBR.FAMILY", InBoundData = true)]        
		public Nullable<int> SNbrFamily { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.NEED.BASED.EMP", InBoundData = true)]        
		public Nullable<int> SNeedBasedEmp { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.OTHER.NON.REP.MONEY", InBoundData = true)]        
		public Nullable<int> SOtherNonRepMoney { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.OTHER.UNTAXED.INC", InBoundData = true)]        
		public Nullable<int> SOtherUntaxedInc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.PENSION.PAYMENTS", InBoundData = true)]        
		public Nullable<int> SPensionPayments { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.SCHED1", InBoundData = true)]        
		public string SSched1 { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.SSI.BEN", InBoundData = true)]        
		public string SSsiBen { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.STUDENT.INC", InBoundData = true)]        
		public Nullable<int> SStudentInc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.TANF", InBoundData = true)]        
		public string STanf { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.TAX.FILING.STATUS", InBoundData = true)]        
		public string STaxFilingStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.TAX.FORM.TYPE", InBoundData = true)]        
		public string STaxFormType { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.TAX.RETURN.FILED", InBoundData = true)]        
		public string STaxReturnFiled { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.US.TAX.PD", InBoundData = true)]        
		public Nullable<int> SUsTaxPd { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.VET.NON.ED.BEN", InBoundData = true)]        
		public Nullable<int> SVetNonEdBen { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.WIC", InBoundData = true)]        
		public string SWic { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SIGNED.FLAG", InBoundData = true)]        
		public string SignedFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "SPOUSE.INC", InBoundData = true)]        
		public Nullable<int> SpouseInc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "VETERAN", InBoundData = true)]        
		public string Veteran { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "P.COMBAT.PAY", InBoundData = true)]        
		public Nullable<int> PCombatPay { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "S.UNTX.IRA.PEN", InBoundData = true)]        
		public Nullable<int> SUntxIraPen { get; set; }

		public UpdateAidApplRequest()
		{	
			ExtendedNames = new List<string>();
			ExtendedValues = new List<string>();
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "UPDATE.AID.APPL", GeneratedDateTime = "30-01-2023 6.35.51 PM", User = "deepak_p")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class UpdateAidApplResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "APPL.ID", OutBoundData = true)]        
		public string ApplId { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR", UseEnvisionBooleanConventions = EnvisionBooleanTypesEnum.OneZero, OutBoundData = true)]        
		public bool ErrorFlag { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.CODE", OutBoundData = true)]        
		public List<string> ErrorCode { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "ERROR.MESSAGE", OutBoundData = true)]        
		public List<string> ErrorMessage { get; set; }

		public UpdateAidApplResponse()
		{	
			ErrorCode = new List<string>();
			ErrorMessage = new List<string>();
		}
	}
}