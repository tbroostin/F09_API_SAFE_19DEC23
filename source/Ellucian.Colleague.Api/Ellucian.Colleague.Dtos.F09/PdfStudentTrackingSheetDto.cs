using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet
{
    public class Phones
    {
        public string PhoneNo { get; set; }

        public string PhoneExt { get; set; }

        public string PhoneType { get; set; }
    }
    
    public class Emails
    {
        public string EmailAddrs { get; set; }

        public string EmailTypes { get; set; }

        public string EmailAuth { get; set; }
    }
    
    public class Programs
    {
        public string Prog { get; set; }

        public string ProgStartDate { get; set; }

        public string ProgStatus { get; set; }

        public string ProgYearsEnrl { get; set; }

        public string ProgAntCmpl { get; set; }

        public string ProgFac { get; set; }

        public string ProgAd { get; set; }
    }
    
    public class ProgExtras
    {
        public string ProgExtraDesc { get; set; }

        public string ProgExtraStartDate { get; set; }
    }

    public class TEs
    {
        public string TeInst { get; set; }

        public string TeTranCourse { get; set; }

        public string TeTranCredit { get; set; }

        public string TeTranGrade { get; set; }

        public string TeEquivCourse { get; set; }

        public string TeEquivCredit { get; set; }

        public string TeEquivStatus { get; set; }
    }
    
    public class GRs
    {
        public string GrStcTerm { get; set; }

        public string GrStcCrsName { get; set; }

        public string GrStcTitle { get; set; }

        public string GrStcCredAtt { get; set; }

        public string GrStcCredCmpl { get; set; }

        public string GrStcGrade { get; set; }

        public string GrStcFaculty { get; set; }
    }

    public class SAs
    {
        public string SaStcCrsName { get; set; }

        public string SaStcTitle { get; set; }

        public string SaEndDate { get; set; }
    }
    
    public class CEs
    {
        public string CeStcCrsName { get; set; }

        public string CeStcTitle { get; set; }

        public string CeEndDate { get; set; }

        public string CeStcCredCmpl { get; set; }
    }
    
    public class KAs
    {
        public string KaCrsName { get; set; }

        public string KaTitle { get; set; }

        public string KaFaculty { get; set; }

        public string KaGrade { get; set; }

        public string KaTerm { get; set; }

        public string KaCred { get; set; }
    }
    
    public class IPs
    {
        public string IpCrsName { get; set; }

        public string IpTitle { get; set; }

        public string IpTerm { get; set; }

        public string IpFaculty { get; set; }

        public string IpCred { get; set; }
    }
    
    public class INs
    {
        public string InSite { get; set; }

        public string InStartDate { get; set; }

        public string InEndDate { get; set; }

        public string InHours { get; set; }
    }

    public class RPs
    {
        public string RpSite { get; set; }

        public string RpHours { get; set; }

        public string RpProjectTitle { get; set; }
    }
    
    public class Ms
    {
        public string MSite { get; set; }

        public string MHours { get; set; }

        public string MProjectTitle { get; set; }
    }
    
    public class DisSteps
    {
        public string DisStep { get; set; }

        public string DisStepDesc { get; set; }

        public string DisStepApprDate { get; set; }
    }

    public class Leaves
    {
        public string LeaveStartDate { get; set; }

        public string LeaveEndDate { get; set; }

        public string LeaveDesc { get; set; }
    }
    
    public class Evals
    {
        public string EvalStartDate { get; set; }

        public string EvalEndDate { get; set; }

        public string EvalProg { get; set; }

        public string EvalStatus { get; set; }
    }
    
    public class PRs
    {
        public string PrSite { get; set; }

        public string PrStartDate { get; set; }

        public string AlPrEndDate { get; set; }

        public string AlPrHours { get; set; }
    }
    
    public class PdfTrackingSheetRequestDto
    {
        public string Id { get; set; }

        public PdfTrackingSheetRequestDto()
        {
        }
    }
    
    public class PdfTrackingSheetResponseDto
    {
        public string Id { get; set; }

        public string StuName { get; set; }

        public string StuAddr { get; set; }

        public string BusAddr { get; set; }

        public string FamiliarName { get; set; }

        public string GradProgAdvisor { get; set; }

        public string TranEquivText { get; set; }

        public string TkResdyHours { get; set; }

        public string TkReshrHours { get; set; }

        public string ADisChair { get; set; }

        public string DisAd { get; set; }

        public string DisFacRdr { get; set; }

        public string DisStuRdr { get; set; }

        public string DisConFac { get; set; }

        public string DisExtExam { get; set; }

        public string DisPrApprDate { get; set; }

        public string DisReApprDate { get; set; }

        public string DisReWaivDate { get; set; }

        public string DisPrOralDate { get; set; }

        public string AdLabel { get; set; }

        public List<string> Degrees { get; set; }

        public List<Phones> Phones { get; set; }

        public List<Emails> Emails { get; set; }

        public List<Programs> Programs { get; set; }

        public List<ProgExtras> ProgExtras { get; set; }

        public List<TEs> TEs { get; set; }

        public List<GRs> GRs { get; set; }

        public List<SAs> SAs { get; set; }

        public List<CEs> CEs { get; set; }

        public List<KAs> KAs { get; set; }

        public List<IPs> IPs { get; set; }

        public List<INs> INs { get; set; }

        public List<RPs> RPs { get; set; }

        public List<Ms> Ms { get; set; }

        public List<DisSteps> DisSteps { get; set; }

        public List<Leaves> Leaves { get; set; }

        public List<Evals> Evals { get; set; }

        public List<PRs> PRs { get; set; }

        public PdfTrackingSheetResponseDto(
            string id, string stuName, string stuAddr, string busAddr, string familiarName, string gradProgAdvisor, string transEquivText, string tkResdyHours, string tkReshrHours,
            string aDisChair, string disAd, string disFacRdr, string disStuRdr, string disConFac, string disExtExam,
            string disPrApprDate, string disPrOralDate, string disReApprDate, string disReWaivDate, string adLabel,
            List<string> degrees, List<Phones> phones, List<Emails> emails, List<Programs> programs, List<ProgExtras> progExtras,
            List<TEs> tes, List<GRs> grs, List<SAs> sas, List<CEs> ces, List<KAs> kas, List<IPs> ips, List<INs> ins, List<RPs> rps, List<Ms> ms,
            List<DisSteps> disSteps, List<Leaves> leaves, List<Evals> evals, List<PRs> prs
            )
        {
            this.Id = id;
            this.StuName  = stuName;
            this.StuAddr  = stuAddr;
            this.BusAddr  = busAddr;
            this.FamiliarName  = familiarName;
            this.GradProgAdvisor  = gradProgAdvisor;
            this.TranEquivText  = transEquivText;
            this.TkResdyHours  = tkResdyHours;
            this.TkReshrHours  = tkReshrHours;
            this.ADisChair  = aDisChair;
            this.DisAd  = disAd;
            this.DisFacRdr  = disFacRdr;
            this.DisStuRdr  = disStuRdr;
            this.DisConFac  = disConFac;
            this.DisExtExam  = disExtExam;
            this.DisPrApprDate = disPrApprDate;
            this.DisPrOralDate = disPrOralDate;
            this.DisReApprDate = disReApprDate; 
            this.DisReWaivDate = disReWaivDate;
            this.AdLabel  = adLabel;
            this.Degrees = degrees;
            this.Phones = phones;
            this.Emails = emails;
            this.Programs = programs;
            this.ProgExtras = progExtras;
            this.TEs = tes;
            this.GRs = grs;
            this.SAs = sas;
            this.CEs = ces;
            this.KAs = kas;
            this.IPs = ips;
            this.INs = ins;
            this.RPs = rps;
            this.Ms = ms;
            this.DisSteps = disSteps;
            this.Leaves = leaves;
            this.Evals = evals;
            this.PRs = prs;
        }
    }
}
