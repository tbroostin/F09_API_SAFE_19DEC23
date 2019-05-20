//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the DSL/T4 Generator - Version 1.2
//     Last generated on 5/20/2019 9:05:27 AM by user tshadley
//
//     Type: CTX
//     Transaction ID: XCTX.F09.PDF.TRACKING.SHEET
//     Application: ST
//     Environment: FGU3
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

namespace Ellucian.Colleague.Data.F09.Transactions
{
	[DataContract]
	public class Phones
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PHONE.NO", OutBoundData = true)]
		public string PhoneNo { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PHONE.EXT", OutBoundData = true)]
		public string PhoneExt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PHONE.TYPE", OutBoundData = true)]
		public string PhoneType { get; set; }
	}

	[DataContract]
	public class Emails
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.ADDRS", OutBoundData = true)]
		public string EmailAddrs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.TYPES", OutBoundData = true)]
		public string EmailTypes { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EMAIL.AUTH", OutBoundData = true)]
		public string EmailAuth { get; set; }
	}

	[DataContract]
	public class Programs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG", OutBoundData = true)]
		public string Prog { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.START.DATE", OutBoundData = true)]
		public string ProgStartDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.STATUS", OutBoundData = true)]
		public string ProgStatus { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.YEARS.ENRL", OutBoundData = true)]
		public string ProgYearsEnrl { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.ANT.CMPL", OutBoundData = true)]
		public string ProgAntCmpl { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.FAC", OutBoundData = true)]
		public string ProgFac { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PROG.AD", OutBoundData = true)]
		public string ProgAd { get; set; }
	}

	[DataContract]
	public class ProgExtras
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EXTRA.DESC", OutBoundData = true)]
		public string ProgExtraDesc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EXTRA.START.DATE", OutBoundData = true)]
		public string ProgExtraStartDate { get; set; }
	}

	[DataContract]
	public class TEs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TE.INST", OutBoundData = true)]
		public string TeInst { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TE.TRAN.COURSE", OutBoundData = true)]
		public string TeTranCourse { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TE.TRAN.CREDIT", OutBoundData = true)]
		public string TeTranCredit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TE.TRAN.GRADE", OutBoundData = true)]
		public string TeTranGrade { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TE.EQUIV.COURSE", OutBoundData = true)]
		public string TeEquivCourse { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TE.EQUIV.CREDIT", OutBoundData = true)]
		public string TeEquivCredit { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TE.EQUIV.STATUS", OutBoundData = true)]
		public string TeEquivStatus { get; set; }
	}

	[DataContract]
	public class GRs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GR.STC.TERM", OutBoundData = true)]
		public string GrStcTerm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GR.STC.CRS.NAME", OutBoundData = true)]
		public string GrStcCrsName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GR.STC.TITLE", OutBoundData = true)]
		public string GrStcTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GR.STC.CRED.ATT", OutBoundData = true)]
		public string GrStcCredAtt { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GR.STC.CRED.CMPL", OutBoundData = true)]
		public string GrStcCredCmpl { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GR.STC.GRADE", OutBoundData = true)]
		public string GrStcGrade { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GR.STC.FACULTY", OutBoundData = true)]
		public string GrStcFaculty { get; set; }
	}

	[DataContract]
	public class SAs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SA.STC.CRS.NAME", OutBoundData = true)]
		public string SaStcCrsName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SA.STC.TITLE", OutBoundData = true)]
		public string SaStcTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.SA.END.DATE", OutBoundData = true)]
		public string SaEndDate { get; set; }
	}

	[DataContract]
	public class CEs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CE.STC.CRS.NAME", OutBoundData = true)]
		public string CeStcCrsName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CE.STC.TITLE", OutBoundData = true)]
		public string CeStcTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CE.END.DATE", OutBoundData = true)]
		public string CeEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.CE.STC.CRED.CMPL", OutBoundData = true)]
		public string CeStcCredCmpl { get; set; }
	}

	[DataContract]
	public class KAs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.KA.CRS.NAME", OutBoundData = true)]
		public string KaCrsName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.KA.TITLE", OutBoundData = true)]
		public string KaTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.KA.FACULTY", OutBoundData = true)]
		public string KaFaculty { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.KA.GRADE", OutBoundData = true)]
		public string KaGrade { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.KA.TERM", OutBoundData = true)]
		public string KaTerm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.KA.CRED", OutBoundData = true)]
		public string KaCred { get; set; }
	}

	[DataContract]
	public class IPs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IP.CRS.NAME", OutBoundData = true)]
		public string IpCrsName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IP.TITLE", OutBoundData = true)]
		public string IpTitle { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IP.TERM", OutBoundData = true)]
		public string IpTerm { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IP.FACULTY", OutBoundData = true)]
		public string IpFaculty { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IP.CRED", OutBoundData = true)]
		public string IpCred { get; set; }
	}

	[DataContract]
	public class INs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IN.SITE", OutBoundData = true)]
		public string InSite { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IN.START.DATE", OutBoundData = true)]
		public string InStartDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IN.END.DATE", OutBoundData = true)]
		public string InEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.IN.HOURS", OutBoundData = true)]
		public string InHours { get; set; }
	}

	[DataContract]
	public class RPs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.RP.SITE", OutBoundData = true)]
		public string RpSite { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.RP.HOURS", OutBoundData = true)]
		public string RpHours { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.RP.PROJECT.TITLE", OutBoundData = true)]
		public string RpProjectTitle { get; set; }
	}

	[DataContract]
	public class Ms
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.M.SITE", OutBoundData = true)]
		public string MSite { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.M.HOURS", OutBoundData = true)]
		public string MHours { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.M.PROJECT.TITLE", OutBoundData = true)]
		public string MProjectTitle { get; set; }
	}

	[DataContract]
	public class DisSteps
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DIS.STEP", OutBoundData = true)]
		public string DisStep { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DIS.STEP.DESC", OutBoundData = true)]
		public string DisStepDesc { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DIS.STEP.APPR.DATE", OutBoundData = true)]
		public string DisStepApprDate { get; set; }
	}

	[DataContract]
	public class Leaves
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.LEAVE.START.DATE", OutBoundData = true)]
		public string LeaveStartDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.LEAVE.END.DATE", OutBoundData = true)]
		public string LeaveEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.LEAVE.DESC", OutBoundData = true)]
		public string LeaveDesc { get; set; }
	}

	[DataContract]
	public class Evals
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EVAL.START.DATE", OutBoundData = true)]
		public string EvalStartDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EVAL.END.DATE", OutBoundData = true)]
		public string EvalEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EVAL.PROG", OutBoundData = true)]
		public string EvalProg { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.EVAL.STATUS", OutBoundData = true)]
		public string EvalStatus { get; set; }
	}

	[DataContract]
	public class PRs
	{
		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PR.SITE", OutBoundData = true)]
		public string PrSite { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PR.START.DATE", OutBoundData = true)]
		public string PrStartDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PR.END.DATE", OutBoundData = true)]
		public string AlPrEndDate { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.PR.HOURS", OutBoundData = true)]
		public string AlPrHours { get; set; }
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.PDF.TRACKING.SHEET", GeneratedDateTime = "5/20/2019 9:05:27 AM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09PdfTrackingSheetRequest
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember(IsRequired = true)]
		[SctrqDataMember(AppServerName = "A.ID", InBoundData = true)]        
		public string Id { get; set; }

		public ctxF09PdfTrackingSheetRequest()
		{	
		}
	}

	[GeneratedCodeAttribute("Colleague Data Contract Generator", "1.2")]
	[DataContract]
	[ColleagueDataContract(ColleagueId = "XCTX.F09.PDF.TRACKING.SHEET", GeneratedDateTime = "5/20/2019 9:05:27 AM", User = "tshadley")]
	[SctrqDataContract(Application = "ST", DataContractVersion = 1)]
	public class ctxF09PdfTrackingSheetResponse
	{
		/// <summary>
		/// Version
		/// </summary>
		[DataMember]
		public int _AppServerVersion { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.STU.NAME", OutBoundData = true)]        
		public string StuName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.STU.ADDR", OutBoundData = true)]        
		public string StuAddr { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.BUS.ADDR", OutBoundData = true)]        
		public string BusAddr { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.FAMILIAR.NAME", OutBoundData = true)]        
		public string FamiliarName { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.GRAD.PROG.ADVISOR", OutBoundData = true)]        
		public string GradProgAdvisor { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.TRAN.EQUIV.TEXT", OutBoundData = true)]        
		public string TranEquivText { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.TK.RESDY.HOURS", OutBoundData = true)]        
		public string TkResdyHours { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.TK.RESHR.HOURS", OutBoundData = true)]        
		public string TkReshrHours { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DIS.CHAIR", OutBoundData = true)]        
		public string ADisChair { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DIS.AD", OutBoundData = true)]        
		public string DisAd { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DIS.FAC.RDR", OutBoundData = true)]        
		public string DisFacRdr { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DIS.STU.RDR", OutBoundData = true)]        
		public string DisStuRdr { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DIS.CON.FAC", OutBoundData = true)]        
		public string DisConFac { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.DIS.EXT.EXAM", OutBoundData = true)]        
		public string DisExtExam { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "A.AD.LABEL", OutBoundData = true)]        
		public string AdLabel { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "AL.DEGREES", OutBoundData = true)]        
		public List<string> Degrees { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.PHONE.NO", OutBoundData = true)]
		public List<Phones> Phones { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.EMAIL.ADDRS", OutBoundData = true)]
		public List<Emails> Emails { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.PROG", OutBoundData = true)]
		public List<Programs> Programs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.EXTRA.DESC", OutBoundData = true)]
		public List<ProgExtras> ProgExtras { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.TE.INST", OutBoundData = true)]
		public List<TEs> TEs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.GR.STC.TERM", OutBoundData = true)]
		public List<GRs> GRs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.SA.STC.CRS.NAME", OutBoundData = true)]
		public List<SAs> SAs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.CE.STC.CRS.NAME", OutBoundData = true)]
		public List<CEs> CEs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.KA.CRS.NAME", OutBoundData = true)]
		public List<KAs> KAs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.IP.CRS.NAME", OutBoundData = true)]
		public List<IPs> IPs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.IN.SITE", OutBoundData = true)]
		public List<INs> INs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.RP.SITE", OutBoundData = true)]
		public List<RPs> RPs { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.M.SITE", OutBoundData = true)]
		public List<Ms> Ms { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.DIS.STEP", OutBoundData = true)]
		public List<DisSteps> DisSteps { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.LEAVE.DESC", OutBoundData = true)]
		public List<Leaves> Leaves { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.EVAL.START.DATE", OutBoundData = true)]
		public List<Evals> Evals { get; set; }

		[DataMember]
		[SctrqDataMember(AppServerName = "Grp:AL.PR.SITE", OutBoundData = true)]
		public List<PRs> PRs { get; set; }

		public ctxF09PdfTrackingSheetResponse()
		{	
			Degrees = new List<string>();
			Phones = new List<Phones>();
			Emails = new List<Emails>();
			Programs = new List<Programs>();
			ProgExtras = new List<ProgExtras>();
			TEs = new List<TEs>();
			GRs = new List<GRs>();
			SAs = new List<SAs>();
			CEs = new List<CEs>();
			KAs = new List<KAs>();
			IPs = new List<IPs>();
			INs = new List<INs>();
			RPs = new List<RPs>();
			Ms = new List<Ms>();
			DisSteps = new List<DisSteps>();
			Leaves = new List<Leaves>();
			Evals = new List<Evals>();
			PRs = new List<PRs>();
		}
	}
}
