// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    [RegisterType]
    public class AccountActivityService : BaseCoordinationService, IAccountActivityService
    {
        private IAccountActivityRepository _activityRepository;
        private IAccountsReceivableRepository _arRepository;
        private IFinancialAidReferenceDataRepository faReferenceDataRepository;

        public AccountActivityService(IAdapterRegistry adapterRegistry, IAccountActivityRepository activityRepository, IAccountsReceivableRepository arRepository,
            IFinancialAidReferenceDataRepository faReferenceDataRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _activityRepository = activityRepository;
            _arRepository = arRepository;
            this.faReferenceDataRepository = faReferenceDataRepository;
        }

        private void CheckPermission(string studentId)
        {
            bool hasAdminPermission = HasPermission(FinancePermissionCodes.ViewStudentAccountActivity);

            var proxySubject = CurrentUser.ProxySubjects.FirstOrDefault();

            // They're allowed to see another's data if they are a proxy for that user or have the admin permission
            if (!CurrentUser.IsPerson(studentId) && !HasProxyAccessForPerson(studentId) && !hasAdminPermission)
            {
                logger.Info(CurrentUser + " does not have permission code " + FinancePermissionCodes.ViewStudentAccountActivity);
                throw new PermissionsException();
            }
        }

        public AccountActivityPeriods GetAccountActivityPeriodsForStudent(string studentId)
        {
            CheckPermission(studentId);

            var termPeriods = _activityRepository.GetAccountPeriods(studentId);
            var nonTermPeriod = _activityRepository.GetNonTermAccountPeriod(studentId);

            var activityDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.AccountPeriod, Dtos.Finance.AccountActivity.AccountPeriod>();
            var activityDto = new AccountActivityPeriods();
            var activityDtoCollection = new List<Dtos.Finance.AccountActivity.AccountPeriod>();

            foreach (var period in termPeriods)
            {
                activityDtoCollection.Add(activityDtoAdapter.MapToType(period));
            }

            activityDto.Periods = activityDtoCollection;
            activityDto.NonTermActivity = activityDtoAdapter.MapToType(nonTermPeriod);

            return activityDto;
        }

        [Obsolete("Obsolete as of API version 1.8, use GetAccountActivityByTermForStudent2 instead")]
        public Dtos.Finance.AccountActivity.DetailedAccountPeriod GetAccountActivityByTermForStudent(string termId, string personId)
        {
            CheckPermission(personId);

            var accountPeriodEntity = _activityRepository.GetTermActivityForStudent(termId, personId);

            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Dtos.Finance.AccountActivity.DetailedAccountPeriod>();

            var accountPeriodDto = adapter.MapToType(accountPeriodEntity);

            return accountPeriodDto;
        }

        /// <summary>
        /// Retrieves account activity details for a student for a term
        /// </summary>
        /// <param name="termId">Term for which to retrieve account activity</param>
        /// <param name="personId">Person for whom to retrieve account activity</param>
        /// <returns>Account activity details for a student for a term</returns>
        public Dtos.Finance.AccountActivity.DetailedAccountPeriod GetAccountActivityByTermForStudent2(string termId, string personId)
        {
            CheckPermission(personId);

            var accountPeriodEntity = _activityRepository.GetTermActivityForStudent2(termId, personId);

            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Dtos.Finance.AccountActivity.DetailedAccountPeriod>();

            var accountPeriodDto = adapter.MapToType(accountPeriodEntity);

            return accountPeriodDto;
        }

        [Obsolete("Obsolete as of API version 1.8, use PostAccountActivityByPeriodForStudent2 instead")]
        public Dtos.Finance.AccountActivity.DetailedAccountPeriod PostAccountActivityByPeriodForStudent(IEnumerable<string> periods, DateTime? startDate, DateTime? endDate, string personId)
        {
            CheckPermission(personId);

            var accountPeriodEntity = _activityRepository.GetPeriodActivityForStudent(periods, startDate, endDate, personId);

            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Dtos.Finance.AccountActivity.DetailedAccountPeriod>();

            var accountPeriodDto = adapter.MapToType(accountPeriodEntity);

            return accountPeriodDto;
        }

        /// <summary>
        /// Retrieves account activity details for a student for a collection of periods
        /// </summary>
        /// <param name="periods">Periods for which to retrieve account activity</param>
        /// <param name="startDate">Date on which retrieved account activity details start</param>
        /// <param name="endDate">Date on which retrieved account activity details end</param>
        /// <param name="personId">Person for whom to retrieve account activity</param>
        /// <returns>Account activity details for a student for a term</returns>
        public Dtos.Finance.AccountActivity.DetailedAccountPeriod PostAccountActivityByPeriodForStudent2(IEnumerable<string> periods, DateTime? startDate, DateTime? endDate, string personId)
        {
            CheckPermission(personId);

            var accountPeriodEntity = _activityRepository.GetPeriodActivityForStudent2(periods, startDate, endDate, personId);

            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Dtos.Finance.AccountActivity.DetailedAccountPeriod>();

            var accountPeriodDto = adapter.MapToType(accountPeriodEntity);

            return accountPeriodDto;
        }

        public IEnumerable<DepositDue> GetDepositsDue(string id)
        {
            CheckPermission(id);

            var depositsDueEntity = _arRepository.GetDepositsDue(id);
            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.DepositDue, DepositDue>();
            List<DepositDue> depositsDueDto = new List<DepositDue>();
            foreach (var item in depositsDueEntity)
            {
                depositsDueDto.Add(adapter.MapToType(item));
            }
            return depositsDueDto;
        }

        public AccountHolder GetAccountHolder(string id)
        {
            CheckPermission(id);

            var accountHolderEntity = _arRepository.GetAccountHolder(id);
            var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountHolder, AccountHolder>();
            var accountHolderDto = adapter.MapToType(accountHolderEntity);
            
            return accountHolderDto;
        }

        /// <summary>
        /// Gets student award disbursement information for the specified award for the specified year
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <param name="awardId">award id</param>
        /// <returns>StudentAwardDisbursementInfo DTO</returns>
        public async Task<Dtos.Finance.AccountActivity.StudentAwardDisbursementInfo> GetStudentAwardDisbursementInfoAsync(string studentId, string awardYearCode, string awardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }

            CheckPermission(studentId);

            var awards = await faReferenceDataRepository.GetFinancialAidAwardsAsync();
            var refAward = awards.FirstOrDefault(a => a.Code == awardId);
            if(refAward == null)
            {
                string message = string.Format("Cannot get disbursement info for the specified student award: {0} - unknown award", awardId);
                logger.Error(message);
                throw new ApplicationException(message);
            }
            var awardCategory = refAward.AwardCategory;
            if(awardCategory == null)
            {
                string message = string.Format("Cannot get disbursement info for the specified award: {0} - award category is missing", awardId);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            TIVAwardCategory? tivCategory = null;
            switch (awardCategory.Code)
            {
                case "GPLUS":
                    tivCategory = TIVAwardCategory.Loan;
                    break;
                case "PLUS":
                    tivCategory = TIVAwardCategory.Loan;
                    break;
                case "GRTCH":
                    tivCategory = TIVAwardCategory.Teach;
                    break;
                case "UGTCH":
                    tivCategory = TIVAwardCategory.Teach;
                    break;
                case "GSL":
                    tivCategory = TIVAwardCategory.Loan;
                    break;
                case "USTF":
                    tivCategory = TIVAwardCategory.Loan;
                    break;
                case "PELL":
                    tivCategory = TIVAwardCategory.Pell;
                    break;
            }

            if(tivCategory == null)
            {
                string message = string.Format("Could not assign a TIV category to the specified award: {0}", awardId);
                logger.Error(message);
                throw new ApplicationException(message);
            }
            TIVAwardCategory category = tivCategory.Value;

            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo, Dtos.Finance.AccountActivity.StudentAwardDisbursementInfo>();
            try
            {
                Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo disbursementInfoEntity = await _activityRepository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, category);
                disbursementInfoEntity.AwardDescription = refAward.Description;

                Dtos.Finance.AccountActivity.StudentAwardDisbursementInfo disbursementInfoDto = adapter.MapToType(disbursementInfoEntity);

                return disbursementInfoDto;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw;
            }
            catch(KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw;
            }
            catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
