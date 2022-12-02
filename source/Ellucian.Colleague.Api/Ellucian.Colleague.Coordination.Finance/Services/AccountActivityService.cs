// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    [RegisterType]
    public class AccountActivityService : BaseCoordinationService, IAccountActivityService
    {
        private IAccountActivityRepository _activityRepository;
        private IAccountsReceivableRepository _arRepository;
        private IFinancialAidReferenceDataRepository faReferenceDataRepository;
        private IFinancialAidRepository _faRepository;

        public AccountActivityService(IAdapterRegistry adapterRegistry, IAccountActivityRepository activityRepository,
            IAccountsReceivableRepository arRepository, IFinancialAidReferenceDataRepository faReferenceDataRepository,
            IFinancialAidRepository faRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _activityRepository = activityRepository;
            _arRepository = arRepository;
            this.faReferenceDataRepository = faReferenceDataRepository;
            _faRepository = faRepository;
        }

        private void CheckPermission(string studentId)
        {
            bool hasAdminPermission = HasPermission(FinancePermissionCodes.ViewStudentAccountActivity);

            var proxySubject = CurrentUser.ProxySubjects.FirstOrDefault();

            // They're allowed to see another's data if they are a proxy for that user or have the admin permission
            if (!CurrentUser.IsPerson(studentId) && !HasProxyAccessForPerson(studentId) && !hasAdminPermission)
            {
                logger.Error(CurrentUser.PersonId + " does not have permission code " + FinancePermissionCodes.ViewStudentAccountActivity);
                throw new PermissionsException();
            }
        }

        public AccountActivityPeriods GetAccountActivityPeriodsForStudent(string studentId)
        {
            CheckPermission(studentId);

            try
            {
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
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
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

            try
            {
                var accountPeriodEntity = _activityRepository.GetTermActivityForStudent2(termId, personId);

                var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Dtos.Finance.AccountActivity.DetailedAccountPeriod>();

                var accountPeriodDto = adapter.MapToType(accountPeriodEntity);

                return accountPeriodDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
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

            try
            {
                var accountPeriodEntity = _activityRepository.GetPeriodActivityForStudent2(periods, startDate, endDate, personId);

                var adapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Dtos.Finance.AccountActivity.DetailedAccountPeriod>();

                var accountPeriodDto = adapter.MapToType(accountPeriodEntity);

                return accountPeriodDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
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

            var accountHolderEntity = Task.Run(async () => await _arRepository.GetAccountHolderAsync(id)).GetAwaiter().GetResult();
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

            try {
                var awards = await faReferenceDataRepository.GetFinancialAidAwardsAsync();
                var refAward = awards.FirstOrDefault(a => a.Code == awardId);
                if (refAward == null)
                {
                    string message = string.Format("Cannot get disbursement info for the specified student award: {0} - unknown award", awardId);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }
                var awardCategory = refAward.AwardCategory;
                if (awardCategory == null)
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

                if (tivCategory == null)
                {
                    string message = string.Format("Could not assign a TIV category to the specified award: {0}", awardId);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }
                TIVAwardCategory category = tivCategory.Value;

                var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo, Dtos.Finance.AccountActivity.StudentAwardDisbursementInfo>();
                Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo disbursementInfoEntity = await _activityRepository.GetStudentAwardDisbursementInfoAsync(studentId, awardYearCode, awardId, category);
                disbursementInfoEntity.AwardDescription = refAward.Description;

                Dtos.Finance.AccountActivity.StudentAwardDisbursementInfo disbursementInfoDto = adapter.MapToType(disbursementInfoEntity);

                return disbursementInfoDto;
                }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw;
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Returns information about potentially untransmitted D7 financial aid, based on
        /// current charges, credits, and awarded aid.
        /// </summary>
        /// <param name="criteria">The <see cref="PotentialD7FinancialAidCriteria"/> criteria of
        /// potential financial aid for which to search.</param>
        /// <returns>Enumeration of <see cref="Dtos.Finance.AccountActivity.PotentialD7FinancialAid"/>  
        /// awards and potential award amounts.</returns>
        public async Task<IEnumerable<Dtos.Finance.AccountActivity.PotentialD7FinancialAid>> GetPotentialD7FinancialAidAsync(PotentialD7FinancialAidCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            if (string.IsNullOrEmpty(criteria.StudentId))
            {
                throw new ArgumentNullException("StudentId");
            }
            if (string.IsNullOrEmpty(criteria.TermId))
            {
                throw new ArgumentNullException("TermId");
            }
            if (criteria.AwardPeriodAwardsToEvaluate == null)
            {
                throw new ArgumentNullException("awardsToEvaluate");
            }

            try
            {
                // criteria contains a list of tuples.  The first field of the tuple is a string
                // and cannot be null or empty
                if (criteria.AwardPeriodAwardsToEvaluate
                    .Where(x => string.IsNullOrEmpty(x.AwardPeriodAward))
                    .ToList()
                    .Any())
                {
                    throw new ArgumentException("Received a null award to evaluate.");
                }

                CheckPermission(criteria.StudentId);

                var dtoAdapter = _adapterRegistry.GetAdapter<Dtos.Finance.PotentialD7FinancialAidCriteria, Domain.Finance.Entities.PotentialD7FinancialAidCriteria>();
                var entityCriteria = dtoAdapter.MapToType(criteria);
                var d7Entity = await _faRepository.GetPotentialD7FinancialAidAsync(entityCriteria);
                var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.AccountActivity.PotentialD7FinancialAid, Dtos.Finance.AccountActivity.PotentialD7FinancialAid>();
                var d7Dto = d7Entity.Select(x => adapter.MapToType(x));
                return d7Dto.ToList();
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }
    }
}
