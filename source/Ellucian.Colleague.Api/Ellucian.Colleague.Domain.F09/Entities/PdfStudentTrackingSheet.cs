using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet
{
    [Serializable]
    public class Phones
    {
        public string PhoneNo { get; set; }

        public string PhoneExt { get; set; }

        public string PhoneType { get; set; }
    }

    [Serializable]
    public class Emails
    {
        public string EmailAddrs { get; set; }

        public string EmailTypes { get; set; }

        public string EmailAuth { get; set; }
    }

    [Serializable]
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

    [Serializable]
    public class ProgExtras
    {
        public string ProgExtraDesc { get; set; }

        public string ProgExtraStartDate { get; set; }
    }

    [Serializable]
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

    [Serializable]
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

    [Serializable]
    public class SAs
    {
        public string SaStcCrsName { get; set; }

        public string SaStcTitle { get; set; }

        public string SaEndDate { get; set; }
    }

    [Serializable]
    public class CEs
    {
        public string CeStcCrsName { get; set; }

        public string CeStcTitle { get; set; }

        public string CeEndDate { get; set; }

        public string CeStcCredCmpl { get; set; }
    }

    [Serializable]
    public class KAs
    {
        public string KaCrsName { get; set; }

        public string KaTitle { get; set; }

        public string KaFaculty { get; set; }

        public string KaGrade { get; set; }

        public string KaTerm { get; set; }

        public string KaCred { get; set; }
    }

    [Serializable]
    public class IPs
    {
        public string IpCrsName { get; set; }

        public string IpTitle { get; set; }

        public string IpTerm { get; set; }

        public string IpFaculty { get; set; }

        public string IpCred { get; set; }
    }

    [Serializable]
    public class INs
    {
        public string InSite { get; set; }

        public string InStartDate { get; set; }

        public string InEndDate { get; set; }

        public string InHours { get; set; }
    }

    [Serializable]
    public class RPs
    {
        public string RpSite { get; set; }

        public string RpHours { get; set; }

        public string RpProjectTitle { get; set; }
    }

    [Serializable]
    public class Ms
    {
        public string MSite { get; set; }

        public string MHours { get; set; }

        public string MProjectTitle { get; set; }
    }

    [Serializable]
    public class DisSteps
    {
        public string DisStep { get; set; }

        public string DisStepDesc { get; set; }

        public string DisStepApprDate { get; set; }
    }

    [Serializable]
    public class Leaves
    {
        public string LeaveStartDate { get; set; }

        public string LeaveEndDate { get; set; }

        public string LeaveDesc { get; set; }
    }

    [Serializable]
    public class Evals
    {
        public string EvalStartDate { get; set; }

        public string EvalEndDate { get; set; }

        public string EvalProg { get; set; }

        public string EvalStatus { get; set; }
    }

    [Serializable]
    public class PRs
    {
        public string PrSite { get; set; }

        public string PrStartDate { get; set; }

        public string AlPrEndDate { get; set; }

        public string AlPrHours { get; set; }
    }

    [Serializable]
    public class PdfTrackingSheetRequest
    {
        public string Id { get; set; }
    }

    [Serializable]
    public class PdfTrackingSheetResponse
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
    }
}

