using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.F09.Transactions;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.F09.Repositories
{
    [RegisterType]
    class GetF09StuTrackingSheetRepository : BaseColleagueRepository, IGetF09StuTrackingSheetRepository
    {
        public GetF09StuTrackingSheetRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<GetF09StuTrackingSheetResponse> GetF09StuTrackingSheetAsync(string personId)
        {
            var request = new CtxF09StuTrackingSheetRequest();
            request.Id = personId;

            GetF09StuTrackingSheetResponse profile;

            try
            {
                CtxF09StuTrackingSheetResponse response = await transactionInvoker.ExecuteAsync<CtxF09StuTrackingSheetRequest, CtxF09StuTrackingSheetResponse>(request);
                profile = this.CreateStuTrackingSheetObject(response);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetF09StuTrackingSheetAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetF09StuTrackingSheetAsync': " + String.Join("\n", ex.Message));
            }

            return profile;
        }

        private GetF09StuTrackingSheetResponse CreateStuTrackingSheetObject(CtxF09StuTrackingSheetResponse response)
        {
            GetF09StuTrackingSheetResponse studentTrackingSheet = new GetF09StuTrackingSheetResponse();
            studentTrackingSheet.Html = response.Html;

            return studentTrackingSheet;
        }


        public async Task<PdfTrackingSheetResponse> GetPdfStudentTrackingSheetAsync(string personId)
        {
            var request = new ctxF09PdfTrackingSheetRequest();
            request.Id = personId;

            PdfTrackingSheetResponse application;

            try
            {
                ctxF09PdfTrackingSheetResponse response = await transactionInvoker.ExecuteAsync<ctxF09PdfTrackingSheetRequest, ctxF09PdfTrackingSheetResponse>(request);
                application = this.CreatePdfStudentTrackingSheetObject(response, personId);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetPdfStudentTrackingSheetAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetPdfStudentTrackingSheetAsync': " + String.Join("\n", ex.Message));
            }

            return application;
        }

        private PdfTrackingSheetResponse CreatePdfStudentTrackingSheetObject(ctxF09PdfTrackingSheetResponse response, string personId)
        {
            PdfTrackingSheetResponse application = new PdfTrackingSheetResponse();
            application.Id = personId;
            application.StuName = response.StuName;
            application.StuAddr = response.StuAddr;
            application.BusAddr = response.BusAddr;
            application.FamiliarName = response.FamiliarName;
            application.GradProgAdvisor = response.GradProgAdvisor;
            application.TranEquivText = response.TranEquivText;
            application.TkResdyHours = response.TkResdyHours;
            application.TkReshrHours = response.TkReshrHours;
            application.ADisChair = response.ADisChair;
            application.DisAd = response.DisAd;
            application.DisFacRdr = response.DisFacRdr;
            application.DisStuRdr = response.DisStuRdr;
            application.DisConFac = response.DisConFac;
            application.DisExtExam = response.DisExtExam;
            application.AdLabel = response.AdLabel;
            application.Degrees = response.Degrees;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones> phones = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones>();
            foreach (Transactions.Phones respOptions in response.Phones)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones();
                option.PhoneNo = respOptions.PhoneNo;
                option.PhoneExt = respOptions.PhoneExt;
                option.PhoneType = respOptions.PhoneType;
                phones.Add(option);
            }
            application.Phones = phones;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Emails> emails = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Emails>();
            foreach (Transactions.Emails respOptions in response.Emails)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Emails option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Emails();
                option.EmailAddrs = respOptions.EmailAddrs;
                option.EmailTypes = respOptions.EmailTypes;
                option.EmailAuth = respOptions.EmailAuth;
                emails.Add(option);
            }
            application.Emails = emails;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Programs> programs = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Programs>();
            foreach (Transactions.Programs respOptions in response.Programs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Programs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Programs();
                option.Prog = respOptions.Prog;
                option.ProgStartDate = respOptions.ProgStartDate;
                option.ProgStatus = respOptions.ProgStatus;
                option.ProgYearsEnrl = respOptions.ProgYearsEnrl;
                option.ProgAntCmpl = respOptions.ProgAntCmpl;
                option.ProgFac = respOptions.ProgFac;
                option.ProgAd = respOptions.ProgAd;
                programs.Add(option);
            }
            application.Programs = programs;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.ProgExtras> progExtras = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.ProgExtras>();
            foreach (Transactions.ProgExtras respOptions in response.ProgExtras)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.ProgExtras option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.ProgExtras();
                option.ProgExtraDesc = respOptions.ProgExtraDesc;
                option.ProgExtraStartDate = respOptions.ProgExtraStartDate;
                progExtras.Add(option);
            }
            application.ProgExtras = progExtras;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.TEs> tes = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.TEs>();
            foreach (Transactions.TEs respOptions in response.TEs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.TEs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.TEs();
                option.TeInst = respOptions.TeInst;
                option.TeTranCourse = respOptions.TeTranCourse;
                option.TeTranCredit = respOptions.TeTranCredit;
                option.TeTranGrade = respOptions.TeTranGrade;
                option.TeEquivCourse = respOptions.TeEquivCourse;
                option.TeEquivCredit = respOptions.TeEquivCredit;
                option.TeEquivStatus = respOptions.TeEquivStatus;
                tes.Add(option);
            }
            application.TEs = tes;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.GRs> grs = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.GRs>();
            foreach (Transactions.GRs respOptions in response.GRs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.GRs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.GRs();
                option.GrStcTerm = respOptions.GrStcTerm;
                option.GrStcCrsName = respOptions.GrStcCrsName;
                option.GrStcTitle = respOptions.GrStcTitle;
                option.GrStcCredAtt = respOptions.GrStcCredAtt;
                option.GrStcCredCmpl = respOptions.GrStcCredCmpl;
                option.GrStcGrade = respOptions.GrStcGrade;
                option.GrStcFaculty = respOptions.GrStcFaculty;
                grs.Add(option);
            }
            application.GRs = grs;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.SAs> sas = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.SAs>();
            foreach (Transactions.SAs respOptions in response.SAs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.SAs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.SAs();
                option.SaStcCrsName = respOptions.SaStcCrsName;
                option.SaStcTitle = respOptions.SaStcTitle;
                option.SaEndDate = respOptions.SaEndDate;
                sas.Add(option);
            }
            application.SAs = sas;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.CEs> ces = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.CEs>();
            foreach (Transactions.CEs respOptions in response.CEs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.CEs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.CEs();
                option.CeStcCrsName = respOptions.CeStcCrsName;
                option.CeStcTitle = respOptions.CeStcTitle;
                option.CeEndDate = respOptions.CeEndDate;
                option.CeStcCredCmpl = respOptions.CeStcCredCmpl;
                ces.Add(option);
            }
            application.CEs = ces;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.KAs> kas = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.KAs>();
            foreach (Transactions.KAs respOptions in response.KAs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.KAs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.KAs();
                option.KaCrsName = respOptions.KaCrsName;
                option.KaTitle = respOptions.KaTitle;
                option.KaFaculty = respOptions.KaFaculty;
                option.KaGrade = respOptions.KaGrade;
                option.KaTerm = respOptions.KaTerm;
                option.KaCred = respOptions.KaCred;
                kas.Add(option);
            }
            application.KAs = kas;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.IPs> ips = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.IPs>();
            foreach (Transactions.IPs respOptions in response.IPs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.IPs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.IPs();
                option.IpCrsName = respOptions.IpCrsName;
                option.IpTitle = respOptions.IpTitle;
                option.IpTerm = respOptions.IpTerm;
                option.IpFaculty = respOptions.IpFaculty;
                option.IpCred = respOptions.IpCred;
                ips.Add(option);
            }
            application.IPs = ips;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.INs> ins = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.INs>();
            foreach (Transactions.INs respOptions in response.INs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.INs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.INs();
                option.InSite = respOptions.InSite;
                option.InStartDate = respOptions.InStartDate;
                option.InEndDate = respOptions.InEndDate;
                option.InHours = respOptions.InHours;
                ins.Add(option);
            }
            application.INs = ins;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.RPs> rps = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.RPs>();
            foreach (Transactions.RPs respOptions in response.RPs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.RPs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.RPs();
                option.RpSite = respOptions.RpSite;
                option.RpHours = respOptions.RpHours;
                option.RpProjectTitle = respOptions.RpProjectTitle;
                rps.Add(option);
            }
            application.RPs = rps;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Ms> ms = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Ms>();
            foreach (Transactions.Ms respOptions in response.Ms)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Ms option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Ms();
                option.MSite = respOptions.MSite;
                option.MHours = respOptions.MHours;
                option.MProjectTitle = respOptions.MProjectTitle;
                ms.Add(option);
            }
            application.Ms = ms;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.DisSteps> disSteps = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.DisSteps>();
            foreach (Transactions.DisSteps respOptions in response.DisSteps)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.DisSteps option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.DisSteps();
                option.DisStep = respOptions.DisStep;
                option.DisStepDesc = respOptions.DisStepDesc;
                option.DisStepApprDate = respOptions.DisStepApprDate;
                disSteps.Add(option);
            }
            application.DisSteps = disSteps;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Leaves> leaves = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Leaves>();
            foreach (Transactions.Leaves respOptions in response.Leaves)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Leaves option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Leaves();
                option.LeaveStartDate = respOptions.LeaveStartDate;
                option.LeaveEndDate = respOptions.LeaveEndDate;
                option.LeaveDesc = respOptions.LeaveDesc;
                leaves.Add(option);
            }
            application.Leaves = leaves;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Evals> evals = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Evals>();
            foreach (Transactions.Evals respOptions in response.Evals)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Evals option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Evals();
                option.EvalStartDate = respOptions.EvalStartDate;
                option.EvalEndDate = respOptions.EvalEndDate;
                option.EvalProg = respOptions.EvalProg;
                option.EvalStatus = respOptions.EvalStatus;
                evals.Add(option);
            }
            application.Evals = evals;

            List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.PRs> prs = new List<Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.PRs>();
            foreach (Transactions.PRs respOptions in response.PRs)
            {
                Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.PRs option = new Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.PRs();
                option.PrSite = respOptions.PrSite;
                option.PrStartDate = respOptions.PrStartDate;
                option.AlPrEndDate = respOptions.AlPrEndDate;
                option.AlPrHours = respOptions.AlPrHours;
                prs.Add(option);
            }
            application.PRs = prs;

            return application;
        }

    }
}

